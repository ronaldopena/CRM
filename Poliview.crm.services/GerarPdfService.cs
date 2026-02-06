using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class GerarPdfService : IGerarPdfService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public GerarPdfService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public void download(string arquivo)
        {
            throw new NotImplementedException();
        }

        public GerarPdfResposta listar(GerarPdfRequisicao pdf)
        {
            var retorno = new GerarPdfResposta();
            try
            {
                retorno.mensagem = "";
                if (pdf == null) 
                {
                    retorno.mensagem = "Não foi passado os dados para geração do PDF";
                    retorno.sucesso = 0;
                    retorno.url = "";
                    retorno.arquivo = "";
                    return retorno;
                }
                else if (string.IsNullOrEmpty(pdf.codigoclientesp7))
                {
                    retorno.mensagem = "Não foi passado o codigoclientesp7 para geração do PDF";
                    retorno.sucesso = 0;
                    retorno.url = "";
                    retorno.arquivo = "";
                    return retorno;
                }
                else
                {
                    return GerarDocumento(pdf);
                }                
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.sucesso = 0;
                retorno.url = "";
                retorno.arquivo = "";
                return retorno;
            }
        }

        protected GerarPdfResposta GerarDocumento(GerarPdfRequisicao pdf)
        {
            var retorno = new GerarPdfResposta();
            var parametros = ParametrosService.consultar(_connectionString);
            string nickNameFile = "";
            string tipoArquivo = "0";
            string msgErro = "";
            string assunto = "";

            switch (pdf.tiporelatorio)
            {
                case 1:
                    nickNameFile = "Boleto";
                    tipoArquivo = "1";
                    msgErro = "Erro na geração do Boleto.";
                    assunto = "BOLETO BANCÁRIO";
                    break;
                case 3:
                    nickNameFile = "FichaFinanceira";
                    tipoArquivo = "3";
                    msgErro = "Erro na geração da Ficha Financeira.";
                    assunto = "FICHA FINANCEIRA";
                    break;
                case 2:
                    nickNameFile = "InformeRendimentos";
                    tipoArquivo = "2";
                    msgErro = "Erro na geração do Informe de Rendimentos.";
                    assunto = "INFORME DE RENDIMENTOS";
                    break;
                default:
                    break;
            }

            try
            {
                string fileName = String.Format("{0}_{1}", nickNameFile, DateTime.Now.ToString("yyyyMMddHHmmss"));

                if (string.IsNullOrEmpty(pdf.contrato))
                    pdf.contrato = "0";

                pdf.contratooriginal = pdf.contrato; // Armazena o contrato original para uso posterior

                // Sanitização melhorada dos parâmetros
                string aspas = "\"";
                if (pdf.contrato.Contains(aspas))
                {
                    pdf.contrato = EncodeTo64("*B64*" + pdf.contrato);
                }

                var nomearquivo = Path.Combine(parametros.caminhoPdf, fileName);
                var comando = Path.Combine(parametros.caminhoSiecon, "PORTALCLIENTE.EXE");

                // Verifica se o executável existe antes de prosseguir
                if (!File.Exists(comando))
                {
                    retorno.sucesso = 0;
                    retorno.mensagem = $"Executável não encontrado: {comando}";
                    retorno.arquivo = "";
                    retorno.url = "";
                    return retorno;
                }

                // Sanitização adicional dos argumentos
                var contratoSanitizado = pdf.contrato?.Replace("\"", "\\\"") ?? "0";
                var args = $"{tipoArquivo} \"{nomearquivo}\" {pdf.cobranca} \"{contratoSanitizado}\" {pdf.ano} {pdf.recebimento} 0 {pdf.codigoclientesp7}";

                Console.WriteLine($"Executando comando: {comando}");
                Console.WriteLine($"Argumentos: {args}");

                // Executa o comando com timeout aumentado
                var resultadoComando = RunCommand(comando, args, 120); // 2 minutos de timeout

                Console.WriteLine($"Resultado do comando: {resultadoComando}");

                // Aguarda um pouco para o arquivo ser criado
                Thread.Sleep(1000);


                string arquivoResultado = nomearquivo + ".txt";

                // Verifica se o arquivo foi criado com retry
                int tentativas = 0;
                while (!File.Exists(arquivoResultado) && tentativas < 10)
                {
                    Thread.Sleep(500);
                    tentativas++;
                }

                if (File.Exists(arquivoResultado))
                {
                    try
                    {
                        var retornoTxt = File.ReadAllLines(arquivoResultado);

                        if (retornoTxt.Length > 0 && retornoTxt[0].ToString().Equals("Ok", StringComparison.OrdinalIgnoreCase))
                        {
                            retorno.sucesso = 1;
                            retorno.mensagem = "PDF gerado com sucesso!";
                            retorno.arquivo = fileName + ".pdf";
                            retorno.url = parametros.urlExterna + "/pdf/" + fileName + ".pdf";
                        }
                        else
                        {
                            retorno.sucesso = 0;
                            retorno.mensagem = retornoTxt.Length > 0 ? retornoTxt[0].ToString() : "Resposta vazia do arquivo de resultado";
                            retorno.arquivo = fileName + ".txt";
                            retorno.url = "";
                        }
                    }
                    catch (Exception exFile)
                    {
                        retorno.sucesso = 0;
                        retorno.mensagem = $"Erro ao ler arquivo de resultado: {exFile.Message}";
                        retorno.arquivo = "";
                        retorno.url = "";
                    }
                }
                else
                {
                    retorno.mensagem = "Arquivo de resultado não foi gerado";
                    if (!string.IsNullOrEmpty(resultadoComando))
                    {
                        retorno.mensagem += " - Saída do comando: " + resultadoComando;
                    }
                    retorno.sucesso = 0;
                    retorno.arquivo = "";
                    retorno.url = "";
                }

                // Envio de email apenas se teve sucesso
                if (retorno.sucesso == 1 && !String.IsNullOrEmpty(pdf.email))
                {
                    try
                    {
                        var dadosremetente = DadosRemetentePorContrato(pdf.contratooriginal);
                        Console.WriteLine("Enviar Email para " + pdf.email);
                        EnviarEmailPDF(pdf.email, nomearquivo + ".pdf", assunto, dadosremetente[0].ToString(), dadosremetente[1].ToString(), Convert.ToInt32(dadosremetente[2]));
                        retorno.mensagem = retorno.mensagem + " Enviado Email " + assunto + " para " + pdf.email;
                    }
                    catch (Exception exEmail)
                    {
                        Console.WriteLine($"Erro no envio de email: {exEmail.Message}");
                        // Não falha a operação por causa do email
                        retorno.mensagem += " (Erro no envio do email: " + exEmail.Message + ")";
                    }
                }

                return retorno;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral no GerarDocumento: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                retorno.sucesso = 0;
                retorno.mensagem = msgErro + " " + ex.Message;
                retorno.arquivo = "";
                retorno.url = "";

                return retorno;
            }
        }

        // Método auxiliar para validar parâmetros
        private bool ValidarParametrosComando(GerarPdfRequisicao pdf, out string mensagemErro)
        {
            mensagemErro = "";

            if (pdf == null)
            {
                mensagemErro = "Requisição não pode ser nula";
                return false;
            }

            if (pdf.tiporelatorio < 1 || pdf.tiporelatorio > 3)
            {
                mensagemErro = "Tipo de relatório inválido";
                return false;
            }

            // Adicione outras validações conforme necessário

            return true;
        }
        protected string RunCommand(string command, string args, int timeoutSeconds = 60)
        {
            try
            {
                // Verifica se o executável existe
                if (!File.Exists(command))
                {
                    return $"Erro: Arquivo não encontrado: {command}";
                }

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(command),
                        // Configurações adicionais para melhor compatibilidade
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
                    };

                    // Usar StringBuilder para capturar output de forma assíncrona
                    var outputBuilder = new System.Text.StringBuilder();
                    var errorBuilder = new System.Text.StringBuilder();

                    process.OutputDataReceived += (sender, e) => {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) => {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);
                        }
                    };

                    Console.WriteLine($"Command: {command} {args}");
                    Console.WriteLine($"Working Directory: {process.StartInfo.WorkingDirectory}");

                    process.Start();

                    // Iniciar leitura assíncrona
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Aguardar com timeout
                    bool finished = process.WaitForExit(timeoutSeconds * 1000);

                    if (!finished)
                    {
                        Console.WriteLine("Processo excedeu o timeout, forçando finalização...");
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                process.WaitForExit(5000); // Aguarda 5 segundos para finalizar
                            }
                        }
                        catch (Exception killEx)
                        {
                            Console.WriteLine($"Erro ao finalizar processo: {killEx.Message}");
                        }
                        return "Erro: Processo excedeu o tempo limite de execução.";
                    }

                    // Aguarda um pouco mais para garantir que toda saída foi capturada
                    Thread.Sleep(100);

                    string output = outputBuilder.ToString().Trim();
                    string error = errorBuilder.ToString().Trim();

                    Console.WriteLine($"Exit Code: {process.ExitCode}");
                    Console.WriteLine("Output: " + output);
                    if (!string.IsNullOrEmpty(error))
                        Console.WriteLine("Erro: " + error);

                    // Verifica códigos de erro específicos
                    if (process.ExitCode == 217)
                    {
                        return "Erro 217: Problema na inicialização/finalização da aplicação externa.";
                    }

                    return process.ExitCode == 0 ? output : $"Error {process.ExitCode}: {error}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return $"Exceção na execução: {ex.Message}";
            }
        }

        //protected string RunCommand(string command, string args)
        //{
        //    try
        //    {
        //        // Verifica se o executável existe
        //        if (!File.Exists(command))
        //        {
        //            return $"Erro: Arquivo não encontrado: {command}";
        //        }

        //        var process = new Process()
        //        {
        //            StartInfo = new ProcessStartInfo
        //            {
        //                FileName = command,
        //                Arguments = args,
        //                RedirectStandardOutput = true,
        //                RedirectStandardError = true,
        //                UseShellExecute = false,
        //                CreateNoWindow = true,
        //                // Define o diretório de trabalho
        //                WorkingDirectory = Path.GetDirectoryName(command)
        //            }
        //        };

        //        process.Start();
        //        string output = process.StandardOutput.ReadToEnd();
        //        string error = process.StandardError.ReadToEnd();
        //        process.WaitForExit();

        //        Console.WriteLine($"Command: {command} {args}");
        //        Console.WriteLine($"Working Directory: {process.StartInfo.WorkingDirectory}");
        //        Console.WriteLine($"Exit Code: {process.ExitCode}");
        //        Console.WriteLine("Output: " + output);
        //        Console.WriteLine("Erro: " + error);

        //        return process.ExitCode == 0 ? output : $"Error {process.ExitCode}: {error}";
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception: " + ex.Message);
        //        throw;
        //    }
        //}

        protected string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        protected void EnviarEmailPDF(string emaildestinatario, string urlanexo, string assunto, string nomeremetente, string emailremetente, int idcontaemail)
        {

            var email = new Email();
            email.emailremetente = emailremetente;
            email.nomeremetente = nomeremetente;
            email.emaildestinatario = emaildestinatario;
            email.assunto = assunto;
            email.corpo = "Segue documento anexo";
            email.urlanexo = urlanexo;
            email.tipoemail = "E";
            email.prioridade = 0;
            email.idstatusenvio = 0;            
            email.idcontaemail = idcontaemail;

            using var connection = new SqlConnection(_connectionString);
            var query = "INSERT INTO [dbo].[OPE_EMAIL] " +
                        "           ([DT_EmailInclusao] " +
                        "           ,[DS_EmailNomeRemetente] " +
                        "           ,[DS_EmailRemetente] " +
                        "           ,[DS_EmailDestinatario] " +
                        "           ,[DS_EmailCopia] " +
                        "           ,[DS_EmailCopiaOculta] " +
                        "           ,[DS_EmailAssunto] " +
                        "           ,[DS_EmailCorpo] " +
                        "           ,[IN_EmailCorpoHTML] " +
                        "           ,[IN_EmailStatusEnvio] " +
                        "           ,[DT_EmailEnvio] " +
                        "           ,[IN_EmailPrioridade] " +
                        "           ,[CD_Documento] " +
                        "           ,[IN_Processado] " +
                        "           ,[IN_TipoEmail] " +
                        "           ,[QTD_TentativasEnvio] " +
                        "           ,[DS_ErroEnvio] " +
                        "           ,[ID_Chamado] " +
                        "           ,[Entregue] " +
                        "           ,[CD_Aviso] " +
                        "           ,[urlanexo]" +
                        "           ,[idEmailOrigem]" +
                        "           ,[idcontaemail]) " +
                        "     VALUES " +
                        $"           (GetDate() " +
                        $"           ,@nomeremetente " +
                        $"           ,@emailremetente " +
                        $"           ,@emaildestinatario " +
                        "           ,'' " +
                        "           ,'' " +
                        $"           ,@assunto" +
                        $"           ,@corpo " +
                        $"           ,@corpohtml " +
                        $"           ,@idstatusenvio " +
                        $"           , null " +
                        $"           ,@prioridade " +
                        $"           ,@iddocumento " +
                        $"           ,@processado " +
                        $"           ,@tipoemail " +
                        $"           ,0" +
                        $"           ,@erroenvio " +
                        $"           ,@idchamado " +
                        $"           ,@entregue " +
                        $"           ,@idaviso " +
                        $"           ,@urlanexo" +
                        $"           ,@idEmailOrigem" +
                        $"           ,@idcontaemail)";

            connection.Execute(query, email);

        }

        private ArrayList DadosRemetentePorContrato(string contrato)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select emp.idcontaemail, em.nomeRemetente as nome, em.emailRemetente as email from CAD_CONTRATO c " +
            "left join CAD_EMPREENDIMENTO e on e.CD_EmpreeSP7 = c.CD_EmpreeSP7 " +
            "left join CAD_EMPRESA emp on emp.id = e.idempresa " +
            "left join CAD_CONTA_EMAIL em on em.id = emp.idcontaemail " +
            $"where c.CD_ContratoSP7 = '{contrato}'";
            var result = connection.QueryFirstOrDefault(query);
            var ret = new ArrayList();
            ret.Add(result?.nome);
            ret.Add(result?.email);
            ret.Add(result?.idcontaemail);
            return ret;
        }

        protected void EnviarEmailPDFAnt(string email, string urlanexo, string assunto)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@email", email);
                queryParameters.Add("@assunto", assunto);
                queryParameters.Add("@corpo", "Segue documento anexo");
                queryParameters.Add("@urlanexo", urlanexo);

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.QueryAsync("CRM_API_ENVIAR_EMAIL", queryParameters, commandType: CommandType.StoredProcedure);
                }

                /*
                using var connection = new SqlConnection(_connectionString);
                var query = $"EXEC [dbo].[CRM_API_ENVIAR_EMAIL] " +
                            $"@email = '{email}'," +
                            $"@assunto = '{assunto}'," +
                            $"@corpo = 'Segue documento em anexo. ', " +
                            $"@urlanexo = '{urlanexo}'";
                Console.WriteLine(_connectionString);
                Console.WriteLine(query);
                connection.ExecuteAsync(query);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no envio do email: " + email);
                Console.WriteLine(ex.Message);
                throw;
            }
        }        

    }

    
}
