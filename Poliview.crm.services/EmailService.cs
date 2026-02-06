using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using System.Collections;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class EmailService
    {
        private readonly string _connectionString;
        private IConfiguration configuration;
        private bool verQuery = true;
        private bool verDebug = true;
        private bool verErros = true;
        private bool verInfos = true;
        public int tipoAutenticacaoEmail { get; set; }

        public EmailService(IConfiguration _configuration, ILogger log)
        {
            configuration = _configuration;
            _connectionString = configuration["conexao"].ToString();
            tipoAutenticacaoEmail = this.RetornaTipoAutenticacaoEmail(log);
            verQuery = Convert.ToBoolean(configuration["verQuery"]);
            verDebug = Convert.ToBoolean(configuration["verDebug"]);
            verErros = Convert.ToBoolean(configuration["verErros"]);
            verInfos = Convert.ToBoolean(configuration["verInfos"]);
        }

        public List<Email> PendentesParaEnvio(ILogger log, ContaEmail conta)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"exec dbo.CRM_EMAILS_PENDENTE_ENVIO @idcontaemail={conta.id}";
            if (verQuery) log.Debug(query);
            var registros = (List<Email>)connection.Query<Email>(query);
            return registros;
        }

        public List<Email> PendentesParaProcessamento(ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "exec dbo.CRM_EMAILS_PENDENTE_PROCESSAMENTO  ";
            if (verQuery) log.Debug(query);
            return (List<Email>)connection.Query<Email>(query);
        }

        public int Salvar(Email email, ILogger log, ref string msg)
        {
            if (email.emailremetente != email.emaildestinatario)
            {
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
                            $"           ,@idcontaemail); SELECT @@IDENTITY;";

                if (verQuery) log.Debug(query);
                msg = query;
                return connection.ExecuteScalar<int>(query, email);
            }
            else return -1;

        }

        public int SalvarQuarentena(Email email, ILogger log, ref string msg)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "INSERT INTO [dbo].[OPE_EMAIL_QUARENTENA] " +
                        "           ([nome] " +
                        "           ,[remetente] " +
                        "           ,[destinatario] " +
                        "           ,[assunto] " +
                        "           ,[corpo] " +
                        "           ,[idcontaemail]) " +
                        "     VALUES " +
                        $"           (@nome " +
                        $"           ,@remetente " +
                        $"           ,@destinatario " +
                        $"           ,@assunto" +
                        $"           ,@corpo " +
                        $"           ,@idcontaemail); SELECT @@IDENTITY;";

            if (verQuery) log.Debug(query);
            msg = query;

            var parameters = new DynamicParameters();
            parameters.Add("nome", email.nomeremetente);
            parameters.Add("remetente", email.emailremetente);
            parameters.Add("destinatario", email.emaildestinatario);
            parameters.Add("assunto", email.assunto);
            parameters.Add("corpo", email.corpo);
            parameters.Add("idcontaemail", email.idcontaemail);

            return connection.ExecuteScalar<int>(query, parameters);

        }

        public void Excluir(int idEmail, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"delete from OPE_EMAIL_ARQUIVOS where ID_Email={idEmail} ";
            if (verQuery) log.Debug(query);
            connection.Query(query);

            query = $"delete from OPE_EMAIL where id_email={idEmail} ";
            if (verQuery) log.Debug(query);
            connection.Query(query);
        }

        public void MarcarEmailComoProcessado(int id, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"update OPE_EMAIL set IN_Processado=1, DS_ErroEnvio='', DT_EmailEnvio=GetDate() where id_email={id} ";
            if (verQuery) log.Debug(query);
            connection.Query(query);
        }

        public void MarcarEmailComoEnviado(int idemail, ILogger log, string erroEnvio = "", string requestid = "", string statuscode = "")
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"update OPE_EMAIL set IN_EmailStatusEnvio=1, DT_EmailEnvio=GetDate(),DS_ErroEnvio=@MensagemErro, requestid=@requestid, statuscode=@statuscode where id_email=@Idemail ";
            if (verQuery) log.Debug(query);

            var parameters = new
            {
                MensagemErro = erroEnvio,
                Idemail = idemail,
                requestid = requestid,
                statuscode = statuscode
            };

            connection.Open();
            connection.Execute(query, parameters);
            connection.Close();
            
        }

        public void MarcarEmailComErro(int idemail, string mensagemErro, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "UPDATE OPE_EMAIL SET IN_EmailStatusEnvio = 9, DS_ErroEnvio = @mensagemErro, QTD_TentativasEnvio = COALESCE(QTD_TentativasEnvio, 0) + 1 WHERE id_email = @idemail";
            if (verQuery) log.Debug(query);

            var parameters = new
            {
                MensagemErro = mensagemErro,
                Idemail = idemail
            };

            connection.Open();
            connection.Execute(query, parameters);
            connection.Close();            
        }

        public bool VerificaEmailParaChamadoConcluido(Email email, int processar, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"exec [dbo].[CRM_Email_Enviado_Chamado_Concluido] @idchamado={email.idchamado}, @idemail={email.id}, @processar={processar}";
            if (verQuery) log.Debug(query);
            var result = connection.QueryFirstOrDefault<int>(query);
            return result == 1;
        }

        public bool VerificaEmailParaChamadoCancelado(Email email, int processar, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"exec [dbo].[CRM_Email_Enviado_Chamado_Cancelado] @idchamado={email.idchamado}, @idemail={email.id}, @processar={processar}";
            if (verQuery) log.Debug(query);
            var result = connection.QueryFirstOrDefault<int>(query);
            return result == 1;
        }

        public bool EmailCrm(Email email)
        {
            string pattern = @"<<([^}]+)>>";
            Match m = Regex.Match(email.assunto, pattern);
            return m.Success;
        }

        public Boolean ReplicarEmailParaEnvolvidosComChamado(Email email, ILogger log, ref string msg)
        {
            var emailatual = "";

            try
            {
                var idcontaemail = email.idcontaemail>0 ? email.idcontaemail : 1;
                if (email.idocorrencia == 0) email.idocorrencia = 1;
                var chamadoService = new ChamadoService(configuration);
                var usuarioService = new UsuarioService(configuration);
                var chamadoDetalhe = chamadoService.ListarChamadoDetalhe(email.idchamado, email.idocorrencia);
                var emailsSupervisor = usuarioService.retornaUsuariosSupervisorGrupo(chamadoDetalhe.idgrupo, idcontaemail);
                var emailatendente = usuarioService.retornaAtendenteDoChamado(email.idchamado, email.idocorrencia, idcontaemail);
                var emailService = new EmailService(configuration, log);
                var arquivoservice = new ArquivoService(configuration);

                var emailsParaEnviar = new ArrayList();

                if (!string.IsNullOrEmpty(emailatendente))
                {
                    if (email.emailremetente.Trim().ToLower() != emailatendente.Trim().ToLower())
                        emailsParaEnviar.Add(emailatendente.Trim().ToLower());

                }

                if (!string.IsNullOrEmpty(chamadoDetalhe.emailcliente))
                {
                    if (email.emailremetente.Trim().ToLower() != chamadoDetalhe.emailcliente.Trim().ToLower())
                        emailsParaEnviar.Add(chamadoDetalhe.emailcliente.Trim().ToLower());
                }

                if (emailsSupervisor != null)
                {
                    foreach (var emailstr in emailsSupervisor)
                    {
                        if (emailstr.email.Trim().ToLower() != email.emailremetente.Trim().ToLower() 
                            && emailstr.email.Trim().ToLower() != "usuario@empresa.com.br")
                        {
                            if (emailsParaEnviar.IndexOf(emailstr.email.Trim().ToLower()) == -1)
                                emailsParaEnviar.Add(emailstr.email.Trim().ToLower());
                        }
                    }


                }
                var novoEmail = email;
                novoEmail.tipoemail = "E";
                novoEmail.idemailorigem = email.id;
                novoEmail.idcontaemail = email.idcontaemail;

                var remetente = this.DadosRemetentePadrao(email.idcontaemail, log);

                if (emailsParaEnviar != null)
                {
                    foreach (string emailstr in emailsParaEnviar)
                    {
                        emailatual = $"REPLICANDO EMAIL PARA {emailstr} ID_EMAIL={email.id}";

                        if (email.emailremetente != emailstr && email.emaildestinatario != "usuario@empresa.com.br" && emailstr != "")
                        {
                            log.Debug($"REPLICANDO EMAIL PARA {emailstr} ID_EMAIL={email.id}");

                            novoEmail.nomeremetente = remetente[0]?.ToString().Trim();
                            novoEmail.emailremetente = remetente[1]?.ToString().Trim();
                            novoEmail.emaildestinatario = emailstr.Trim();
                            novoEmail.corpo = email.corpo;
                            //novoEmail.corpohtml = email.corpohtml;

                            var idnovoemail = emailService.Salvar(novoEmail, log, ref msg);

                            var anexosemail = arquivoservice.AnexosDeUmEmail(email.id, log);

                            foreach (var idanexo in anexosemail)
                            {
                                if (idnovoemail != 0 && idanexo != 0)
                                    arquivoservice.SalvarAnexoEmail(idnovoemail, idanexo, log);
                            }
                        }
                    }
                }                
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Erro na replicação de email " + ex.Message + " " + emailatual);
                return false;
            }
        }

        public int EmailDeConfirmacaoEntrega(Email email)
        {
            
            var entregue = 0;
            if (email.assunto.ToUpper().Contains("UNDELIVERED") ||
                email.assunto.ToUpper().Contains("UNDELIVERABLE") ||
                email.assunto.ToUpper().Contains("NÃO É POSSÍVEL ENTREGAR") ||
                email.assunto.ToUpper().Contains("NÃO ENTREGUE"))
            {
                entregue = 2;
            }            
            else if (email.assunto.ToUpper().Contains("SUCCESSFUL MAIL DELIVERY") ||                
                     email.assunto.ToUpper().Contains("RETRANSMITIDAS") ||
                     email.assunto.ToUpper().Contains("ENTREGUE") ||
                     email.assunto.ToUpper().Contains("DELIVERED") ||
                     email.assunto.ToUpper().Contains("LIDA:") ||
                     email.assunto.ToUpper().Contains("RELAYED:") ||
                     email.assunto.ToUpper().Contains("READ:") ||
                     email.assunto.ToUpper().Contains("EXPANDIDO:") ||
                     email.assunto.ToUpper().Contains("CONFIRMAÇÃO DE LEITURA"))
            {
                entregue = 1;
            }            
            else if (email.corpo.Contains("Esta é uma confirmação de leitura da sua mensagem"))
            {
                entregue = 1;
            }
            else if (email.corpo.Contains("Your message has been delivered to the following recipients"))
            {
                entregue = 1;
            }
            else if (email.corpo.Contains("A sua mensagem foi entregue"))
            {
                // A sua mensagem foi entregue aos seguintes destinatários:
                entregue = 1;
            }
            else if (email.corpo.Contains("foi lida em:"))
            {
                entregue = 1;
            }
            else if (email.emailremetente.Contains("postmaster@outlook.com"))
            {
                entregue = 1;
            }
            else if (email.emailremetente.Contains("microsoftexchange"))
            {
                entregue = 1;
            }

            return entregue;
        }

        public int RetornaIdEmail(Email email)
        {
            string pattern = @"<#([^}]+)#>";
            Match m1 = Regex.Match(email.assunto, pattern);
            if (string.IsNullOrEmpty(email.assunto))
                return 0;

            if (! m1.Success)
            {
                Match m2 = Regex.Match(email.corpo, pattern);
                if (m2.Success)
                {
                    try
                    {
                        return Convert.ToInt32(m2.Groups[1].Value);
                    }
                    catch (Exception)
                    {
                        return 0;
                    }                    
                }
                else
                    return 0;
            }
            else
                return Convert.ToInt32(m1.Groups[1].Value);
        }

        public bool EmailDeConfirmacaoLeitura(Email email)
        {
            var ret = false;
            ret = (email.assunto.ToUpper().Contains("LIDA:"));
            return ret;
        }

        private int RetornaTipoAutenticacaoEmail(ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select TOP 1 TipoAutenticacaoEmail from OPE_PARAMETRO ";
            // if (verQuery) log.Debug(query);
            var result = connection.QueryFirstOrDefault<int>(query);
            return result;
        }

        private int RetornaTamanhoMaximoAnexos(ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select TOP 1 coalesce(TamanhoMaximoAnexos,10) as TamanhoMaximoAnexos from OPE_PARAMETRO ";
            var result = connection.QueryFirstOrDefault<int>(query);
            return result;
        }


        public void MarcarEmailComoConfirmadoProvedor(Email email, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var idemailEntregue = RetornaIdEmail(email);            
            var query = $"UPDATE OPE_EMAIL set entregue=1, IN_EmailStatusEnvio=3 where id_email={idemailEntregue}";
            if (verQuery) log.Debug(query);
            connection.Query(query);
        }

        public void MarcarEmailComoRecusadoProvedor(Email email, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var idemailEntregue = RetornaIdEmail(email);
            var query = $"UPDATE OPE_EMAIL set entregue=0, IN_EmailStatusEnvio=5 where id_email={idemailEntregue}";
            if (verQuery) log.Debug(query);
            connection.Query(query);
        }

        public void EnviarEmailTamanhoMaximoAnexoAtingido(Email email, ILogger log)
        {            
            using var connection = new SqlConnection(_connectionString);
            // var idemailEntregue = RetornaIdEmail(email);
            var tamanhoMaximoAnexos = RetornaTamanhoMaximoAnexos(log);
            // var query = $"[dbo].[CRM_API_ENVIAR_EMAIL] @email = N'{email.emailremetente}', @assunto = N'ERRO: {email.assunto}', @corpo = N'O Email ultrapassa o limite de "+ tamanhoMaximoAnexos.ToString() +" MBytes e não será registrado pelo CRM. Corrija o problema e reenvie!', @urlanexo = N'', @cdchamado = 0, @cddocumento = 0";
            var query = $"[dbo].[CRM_API_ENVIAR_EMAIL] @email = @emailparam, @assunto = @assuntoparam, @corpo = @corpoparam, @urlanexo = '', @cdchamado = 0, @cddocumento = 0";
            if (verQuery) log.Debug(query);

            var parameters = new
            {
                emailparam = email.emailremetente,
                assuntoparam = $"ERRO: {email.assunto}",
                corpoparam = $"O Email ultrapassa o limite de " + tamanhoMaximoAnexos.ToString() + " MBytes e não será registrado pelo CRM. Corrija o problema e reenvie!"
            };

            connection.Open();
            connection.Execute(query, parameters);
            connection.Close();

        }

        public List<EmailAnexo> ListarAnexosEmail(int idEmail, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"EXEC CRM_LISTAR_ANEXOS_EMAIL @idEmail={idEmail} ";
            if (verQuery) log.Debug(query);
            return connection.Query<EmailAnexo>(query).ToList();
        }

        private string RetornaBancoArquivos(ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"SELECT DB_NAME()+'_arquivos' AS BANCOARQUIVO ";                        
            if (verQuery) log.Debug(query);
            return connection.QueryFirstOrDefault<string>(query);
        }


        private ArrayList DadosRemetentePadrao(int idcontaemail, ILogger log)
        {
            if (idcontaemail == 0) idcontaemail = 1;
            using var connection = new SqlConnection(_connectionString);
            // var query = "SELECT DS_EmailNome as nome, DS_EmailFrom as email FROM OPE_PARAMETRO WHERE CD_BancoDados=1 AND CD_Mandante=1";
            var query = $"select nomeRemetente as nome, emailRemetente as email from CAD_CONTA_EMAIL WHERE id={idcontaemail}";
            if (verQuery) log.Debug(query);
            // Console.WriteLine(query);
            var result = connection.QueryFirstOrDefault(query);
            var ret = new ArrayList();
            ret.Add(result.nome);
            ret.Add(result.email);
            return ret;
        }


        public int SalvarAviso(Email email, ref string msg, ILogger log)
        {
            // email.idchamado = 208;

            var idaviso = -1;

            try
            {
                var idstatuschamado = retornaIdStatusChamado(email.idchamado);
                var idstatusocorrencia = retornaIdStatusOcorrencia(email.idchamado, email.idocorrencia);
                var parameters = new { @CORPO = email.corpo };
                using var connection = new SqlConnection(_connectionString);
                var query = $"INSERT INTO [dbo].[OPE_CHAMADO_DET_AVISOS] ([CD_BancoDados],[CD_Mandante],[CD_Chamado],[CD_Ocorrencia],[CD_StatusChamado],[CD_StatusOcorrencia],[DS_Assunto],[DS_Descricao],[IN_Lido],[CD_Usuario],[IN_TipoUsuario],[CD_UsuarioLeitura],[DT_Controle],[ID_StatusChamado],[ID_StatusOcorrencia],[DS_Email],[IN_AvisoCliente],[EmailDet]) " +
                $"VALUES " +
                $"(1,1,{email.idchamado},{email.idocorrencia},null,null,@assunto,@corpo,'N',1,'A',null,GetDate(),{idstatuschamado}, {idstatusocorrencia},@emaildestinatario,'S',0);  SELECT @@IDENTITY;";
                msg = query;
                idaviso = connection.QueryFirst<int>(query, email);

            }
            catch (Exception ex)
            {
                idaviso = -1;                
                log.Error("Erro ao salvar o aviso: " + ex.Message);
            }

            return idaviso;
        }

        public void SalvarAvisoEmail(int idemail, int idanexo, int idaviso)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"INSERT INTO OPE_EMAIL_ARQUIVOS (ID_EMAIL, ID_ARQUIVO, DT_CONTROLE, CD_AVISO) values ({idemail}, {idanexo}, GetDate(), {idaviso})";
            connection.Query(query);
        }

        public void SalvarEmailNoHistorico(Email email, ref string msg, ILogger log)
        {
            log.Information("Salvar Email no Histórico " + email.id);
            try
            {
                var emailCorpoCorrigido = HtmlToPlainTextSemInformacoesDoChamado(email.corpo);
                emailCorpoCorrigido = TrocarAspasSimplesPorDupla(emailCorpoCorrigido);

                using var connection = new SqlConnection(_connectionString);
                var query = "INSERT INTO [OPE_CHAMADO_DET_LOG] " +
                "([CD_BancoDados],[CD_Mandante],[CD_Chamado],[CD_Ocorrencia],[DT_Controle],[CD_Usuario],[DS_Descricao],[ID_Status] " +
                ",[CD_Aviso],[visibilidade]) " +
                "VALUES " +
                $"(1,1,{email.idchamado},{email.idocorrencia},GetDate(),1, dbo.CRM_Troca_Campos(@emailcorpo,{email.idchamado},1), null, {email.idaviso}, 0)";
                msg = query;
                var parameters = new
                {
                    emailcorpo = emailCorpoCorrigido,
                };

                connection.Open();
                connection.Execute(query, parameters);
                connection.Close();

            }
            catch (Exception ex)
            {
                log.Error("Erro ao salvar o email no histórico: " + ex.Message);
            }
        }

        public int retornaIdStatusChamado(int idchamado)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"SELECT ID_STATUS FROM OPE_CHAMADO WHERE CD_CHAMADO={idchamado} ";
            return connection.QueryFirstOrDefault<int>(query);
        }

        public int retornaIdStatusOcorrencia(int idchamado, int idocorrencia)
        {
            if (idocorrencia == 0)
                idocorrencia = 1;
            using var connection = new SqlConnection(_connectionString);
            var query = $"select id_status from OPE_CHAMADO_DET where CD_Chamado={idchamado} and CD_OCORRENCIA={idocorrencia} ";
            return connection.QueryFirstOrDefault<int>(query);
        }

        public static string HtmlToPlainTextSemInformacoesDoChamado(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            // text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            text = Regex.Replace(text, "<style>(.|\n)*?<style>", String.Empty);
            text = Regex.Replace(text, @"<[^\>]*>", String.Empty);

            text = text.Replace(@"v\:* {behavior:url(#default#VML);}", String.Empty);
            text = text.Replace(@"o\:* {behavior:url(#default#VML);}", String.Empty);
            text = text.Replace(@"w\:* {behavior:url(#default#VML);}", String.Empty);
            text = text.Replace(@".shape {behavior:url(#default#VML);}", String.Empty);

            var pos = text.LastIndexOf("Informações do Chamado:");

            if (pos > 0)
            {
                text = text.Substring(0, pos);
            }

            return text;
        }

        public void ExcluirEmail(int idemail, ILogger log)
        {
            log.Information($"Excluindo email {idemail}");
            using var connection = new SqlConnection(_connectionString);
            var query = $"delete from OPE_EMAIL where ID_Email={idemail}";
            if (verQuery) log.Debug(query);
            connection.Query(query);
        }

        public void ExcluirAviso(int idaviso, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"delete from OPE_CHAMADO_DET_LOG where cd_aviso={idaviso} ";
            if (verQuery) log.Debug(query);
            log.Information($"Excluindo aviso {idaviso}");
            connection.Query(query);

            query = $"delete from OPE_CHAMADO_DET_AVISOS where cd_aviso={idaviso} ";
            if (verQuery) log.Debug(query);            
            connection.Query(query);
        }

        public void ExcluirAnexo(int idanexo, ILogger log)
        {
            log.Information($"Excluindo anexo {idanexo}");
            using var connection = new SqlConnection(_connectionString);
            var query = $"EXEC CRM_Excluir_Anexo @idAnexo={idanexo} ";
            if (verQuery) log.Debug(query);
            connection.Query(query);
        }

        public string TrocarAspasSimplesPorDupla(string texto)
        {
            return texto.Replace("'", "\"");
        }

        public async Task<int> RecebendoEmails(int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select top 1 COALESCE(recebendoEmails,0) from OPE_EMAIL_PROCESSO where idcontaemail={idcontaemail}";
            var id = connection.QueryFirst<int>(query);
            return id;
        }

        public void ConfigurandoRecebimentoEnvioEmails()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "insert into ope_email_processo " +
                        "select e.id, 0, 0 from CAD_CONTA_EMAIL e where e.id not in (select p.idcontaemail from ope_email_processo p)";
            connection.Query(query);
        }

        public async Task IniciarReceberEmails(int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"update OPE_EMAIL_PROCESSO set recebendoEmails=1 where idcontaemail={idcontaemail}";
            connection.Query(query);
        }

        public async Task PararReceberEmails(int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"update OPE_EMAIL_PROCESSO set recebendoEmails=0 where idcontaemail={idcontaemail}";
            connection.Query(query);
        }

        public async Task<int> EnviandoEmails(int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select top 1 COALESCE(enviandoEmails,0) from OPE_EMAIL_PROCESSO where idcontaemail={idcontaemail}";
            var id = connection.QueryFirst<int>(query);
            return id;
        }

        public async Task IniciarEnviarEmails(int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"update OPE_EMAIL_PROCESSO set enviandoEmails=1 where idcontaemail={idcontaemail} ";
            connection.Query(query);
        }

        public async Task PararEnviarEmails(int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"update OPE_EMAIL_PROCESSO set enviandoEmails=0 where idcontaemail={idcontaemail}";
            connection.Query(query);
        }

        public void SalvarCorpoEAssunto(int idemail, string corpo, string assunto, int aviso)
        {
            if (assunto.Count() > 1000) assunto.Substring(0, 1000);

            using var connection = new SqlConnection(_connectionString);
            var query = $"update OPE_EMAIL set DS_EmailCorpo=@corpoparam, DS_EmailAssunto=@assuntoparam where id_email={idemail} ";

            var parameters = new
            {
                corpoparam = corpo,
                assuntoparam = assunto,
            };

            connection.Open();
            connection.Execute(query, parameters);

            var query2 = $"update OPE_CHAMADO_DET_LOG set DS_DESCRICAO=@corpoparam where CD_AVISO={aviso}";

            var parameters2 = new
            {
                corpoparam = corpo,
            };

            connection.Execute(query2, parameters2);

            connection.Close();

        }

        public void SalvarDataHoraUltimoProcessamentoEmail()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"UPDATE OPE_SERVICOS_MONITORADOS set dataUltimoProcessamento=GETDATE() where chave='EMAIL'";
            connection.Query(query);
        }
    }
}
