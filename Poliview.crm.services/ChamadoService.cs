using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Poliview.crm.domain;
using Poliview.crm.models;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;

namespace Poliview.crm.services
{
    public class ChamadoService : IChamadoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public ChamadoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public IEnumerable<ChamadoHistorico> ListaHistoricoChamados(HistoricoChamadosRequisicao obj)
        {
            if (obj.idocorrencia == 0) obj.idocorrencia = 1;

            using var connection = new SqlConnection(_connectionString);

            var query = $"exec CRM_Listar_Historico " +
                        $"@CD_CHAMADO = {obj.idchamado}, " +
                        $"@CD_OCORRENCIA = {obj.idocorrencia}, " +
                        $"@VISIBILIDADE = {obj.visibilidade} ";

            var result = connection.Query<ListarHistoricoResposta>(query);

            IEnumerable<ChamadoHistorico> retorno = result.Select(x => new ChamadoHistorico
            {
                datahora = x.DATA_CONTROLE_LOG,
                descricao = RetiraTextoInformacoesDoChamado(x.DS_DESCRICAO),
                status = x.DS_STATUS,
                usuario = x.usuario
            });

            return retorno;
        }

        private string RetiraTextoInformacoesDoChamado(string texto)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            //Decode html specific characters
            texto = WebUtility.HtmlDecode(texto);
            //Remove tag whitespace/line breaks
            texto = tagWhiteSpaceRegex.Replace(texto, "><");
            //Replace <br /> with line breaks
            texto = lineBreakRegex.Replace(texto, Environment.NewLine);
            //Strip formatting
            texto = stripFormattingRegex.Replace(texto, string.Empty);

            texto = Regex.Replace(texto, "<style>(.|\n)*?<style>", String.Empty);
            texto = Regex.Replace(texto, @"<[^\>]*>", String.Empty);

            texto = texto.Replace(@"v\:* {behavior:url(#default#VML);}", String.Empty);
            texto = texto.Replace(@"o\:* {behavior:url(#default#VML);}", String.Empty);
            texto = texto.Replace(@"w\:* {behavior:url(#default#VML);}", String.Empty);
            texto = texto.Replace(@".shape {behavior:url(#default#VML);}", String.Empty);

            var pos = texto.LastIndexOf("Informações do Chamado:");
            if (pos > 0)
            {
                texto = texto.Substring(0, pos);
            }           
            return texto;
        }


        public IEnumerable<ChamadoAnexos> ListarAnexosChamado(AnexosChamadoRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "SELECT " +
                            "[CD_Anexo] AS id" +
                            ",[NM_Anexo] as nome" +
                            ",[DS_ANEXO] as descricao" +
                            ",OPE_ARQUIVOS.DS_Extensao as extensao " +
                            ",OPE_PARAMETRO.DS_IpExterno + '/Download.ashx?file=' + cast(CD_Anexo as varchar(5)) as urlanexo " +
                            "FROM [OPE_CHAMADO_ANEXO] " +
                            "left join OPE_ARQUIVOS ON OPE_ARQUIVOS.ID = OPE_CHAMADO_ANEXO.CD_Anexo " +
                            "left join OPE_PARAMETRO ON OPE_PARAMETRO.CD_BancoDados = 1 and OPE_PARAMETRO.CD_Mandante = 1 " +
                            $"where CD_Chamado ={obj.idchamado} and CD_Ocorrencia ={obj.idocorrencia} and OPE_CHAMADO_ANEXO.visibilidade=0 ";

            var result = connection.Query<ChamadoAnexos>(query);

            return result;

        }

        public Chamado ListarChamado(int idchamado)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "exec API_LISTAR_CHAMADO " +
                        $"@idchamado = {idchamado} ";
            Console.WriteLine(query);
            var result = connection.QueryFirst<Chamado>(query);

            return result;
        }

        public ChamadoDetalhe ListarChamadoDetalhe(int idchamado, int idocorrencia = 1)
        {
            idocorrencia = idocorrencia > 0 ? idocorrencia : 1;

            using var connection = new SqlConnection(_connectionString);

            var query = $"select cd_chamado as idchamado, cd_ocorrencia as idocorrencia, naoenviaremails, " +
                        $"NM_cliente as nomecliente, ds_email_cli as emailcliente, NM_EMPREE as empreendimento, " +
                        $"NM_BLOCO as bloco, NR_UNIDADESP7 as unidade, DS_Resumo as tipoocorrencia, " +
                        $"DS_CHAMADO as descricao, NR_DDD as ddd, NR_Telefone as telefone, " +
                        $"NR_Celular as celular, CD_GRUPO as idgrupo, NM_GRUPO as grupo, CD_USUARIO as idatentente, NM_RECURSO as atendente, " +
                        $"IN_ENCERRARCHAMADO as encerrado, DS_STATUSCHAMADO as statusChamado, DS_STATUSOCORRENCIA as statusOcorrencia " +
                        $",nomeempresa, logoempresa " +
                        $"from vListaChamados " +
                        $"where CD_CHAMADO = {idchamado} and  cd_ocorrencia = {idocorrencia}";
            // Console.WriteLine(query);

            try
            {
                var result = connection.QueryFirst<ChamadoDetalhe>(query);
                return result;
            }
            catch (Exception ex)
            {
                // Console.WriteLine("erro: " + ex.Message + " - " + query);
                return new ChamadoDetalhe();
            }
        }

        public IEnumerable<Chamado> ListarChamados(ListarChamadosRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "exec API_LISTAR_CHAMADOS " +
                  $"@empreendimento = {obj.idempreendimento}, " +
                  $"@bloco = {obj.idbloco}, " +
                  $"@unidade = {obj.idunidade}, " +
                  $"@cliente = {obj.idcliente} ";

            var result = connection.Query<Chamado>(query);

            Console.WriteLine(result);

            return result;
        }

        public IEnumerable<ChamadoHistorioEmail> ListarHistoricoEmails(HistoricoEmailsRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"exec CRM_Listar_Historico_Email @CD_CHAMADO = {obj.idchamado}, @EMAIL = '{obj.email}' ";

            var result = connection.Query<ChamadoHistorioEmail>(query);

            return result;
        }

        public IEnumerable<ChamadoOcorrencias> ListarOcorrenciasChamado(int idchamado)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "select ocd.CD_Chamado as idchamado, ocd.CD_Ocorrencia as idocorrencia, " +
                        "dbo.API_DATA_CONTROLE(ocd.DT_Controle, ocd.HR_Controle) as abertura, ocd.DS_Resumo as ocorrencia, ocd.DS_Chamado as descricao " +
                        "from OPE_CHAMADO_DET ocd  " +
                        "left JOIN OPE_CHAMADO oc on oc.CD_Chamado = ocd.CD_Chamado " +
                        $"where ocd.cd_chamado = {idchamado} ";

            var result = connection.Query<ChamadoOcorrencias>(query);

            return result;
        }

        public IEnumerable<OcorrenciasAberturaChamado> ListarOcorrenciasParaAberturaChamado()
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "exec [API_LISTA_OCORRENCIAS_ABERTURA_CHAMADO] ";

            var result = connection.Query<OcorrenciasAberturaChamado>(query);

            return result;
        }

        public SalvarAnexosChamadoResposta SalvarAnexosChamado(SalvarAnexosChamadoRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);
            var ret = new SalvarAnexosChamadoResposta();

            var query = "exec [API_CHAMADOS_SALVAR_ANEXOS]  " +
                        $"@idchamado = { obj.idchamado }, " +
                        $"@anexos = '{ obj.anexos }' ";

            Console.WriteLine(query);

            try
            {
                connection.Query(query);
                ret.mensagem = "ok";
                return ret;
            }
            catch (Exception e)
            {
                ret.mensagem = e.Message;
                return ret;
            }
        }

        public AberturaChamadoResposta SalvarChamado(SalvarChamadoRequisicao obj)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@idCliente", obj.idcliente);
                parameters.Add("@idEmpreendimento", obj.idempreendimento);
                parameters.Add("@idBloco", obj.idbloco);
                parameters.Add("@idUnidade", obj.idunidade);
                parameters.Add("@idEstruturaTipo", obj.idocorrencia);
                parameters.Add("@descricaochamado", obj.descricao);
                parameters.Add("@OrigemChamado", obj.origemchamado);
                parameters.Add("@anexos", obj.anexos);
                parameters.Add("@idNovoChamado", dbType: DbType.Int32, direction: ParameterDirection.Output);
                connection.Execute("CRM_CHAMADO_INCLUIR", parameters, commandType: CommandType.StoredProcedure);

                int returnValue = parameters.Get<int>("@idNovoChamado");

                var result = new AberturaChamadoResposta();
                result.idnovochamado = returnValue;

                return result;
            }
        }

        public IEnumerable<usuariosGrupo> retornaUsuariosSupervisorGrupo(int idgrupo)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"SELECT cd_usuario as idusuario, NM_usuario as nome, " +
                        $"DS_Email as email, CD_Grupo as idgrupo, NM_Grupo as grupo, " +
                        $"IN_Master as master, IN_Supervisor as supervisor " +
                        $"FROM vUsuariosGrupo WHERE CD_Grupo = {idgrupo} and in_supervisor=1";

            var result = connection.Query<usuariosGrupo>(query);

            return result;
        }

        public string retornaAtendenteDoChamado(int idchamado, int idocorrencia, int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"select usu.DS_Email as email from ope_chamado_det det " +
                        $"left join ope_usuario usu on usu.CD_Usuario = det.CD_UsuRecurso " +
                        $"left join ope_usuario_email uem on uem.CD_Usuario = usu.CD_Usuario " +
                        $"left join cad_empresa emp on emp.id = usu.idempresa " +
                        $"where det.CD_Chamado = {idchamado} and det.CD_Ocorrencia={idocorrencia} and emp.idcontaemail={idcontaemail}";
            try
            {
                var result = connection.QueryFirst<string>(query);
                return result;
            }
            catch (Exception ex)
            {
                // Console.WriteLine("erro: " + ex.Message + " - " + query);
                return "";
            }
        }

        public IEnumerable<usuariosGrupo> retornaUsuariosMasterGrupo(int idgrupo)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"SELECT cd_usuario as idusuario, NM_usuario as nome, " +
                        $"DS_Email as email, CD_Grupo as idgrupo, NM_Grupo as grupo, " +
                        $"IN_Master as master, IN_Supervisor as supervisor " +
                        $"FROM vUsuariosGrupo WHERE CD_Grupo = {idgrupo} and in_master=1 and IN_Status='A'";

            var result = connection.Query<usuariosGrupo>(query);

            return result;
        }

        public IEnumerable<usuariosGrupo> retornaUsuariosGrupo(int idgrupo)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"SELECT cd_usuario as idusuario, NM_usuario as nome, " +
                        $"DS_Email as email, CD_Grupo as idgrupo, NM_Grupo as grupo, " +
                        $"IN_Master as master, IN_Supervisor as supervisor " +
                        $"FROM vUsuariosGrupo WHERE CD_Grupo = {idgrupo} and IN_Status='A'";

            var result = connection.Query<usuariosGrupo>(query);

            return result;
        }

        public Boolean LerMensagensChamado(int idchamado)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var query = $"UPDATE OPE_CHAMADO set NovasMensagens=0 where CD_CHAMADO = {idchamado} ";
                Console.WriteLine(query);
                connection.Execute(query);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public Boolean MarcarMensagensChamado(int idchamado)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var query = $"UPDATE OPE_CHAMADO set NovasMensagens=1 where CD_CHAMADO = {idchamado} ";
                Console.WriteLine(query);
                connection.Execute(query);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public string EmailDoCliente(int idchamado)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var query = $@"
                            select cc.DS_Email from OPE_CHAMADO c 
                            left join CAD_CLIENTE cc on cc.CD_Cliente = c.CD_Cliente
                            where c.CD_Chamado={idchamado}
                            ";
                Console.WriteLine(query);
                var result = connection.QueryFirst<string>(query);
                return result;
            }
            catch (Exception ex)
            {
                return $"ERRO: {ex.Message}";
            }

        }

    }
}
