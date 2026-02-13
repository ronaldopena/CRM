using Dapper;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public static class ParametrosService
    {
        public static domain.Parametros consultar(string _connectionString)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "select DS_EmailFrom as emailremetente, DS_EmailNome as nomeremetente, " +
                        "DS_PathInstallSistemaSiecon as caminhoSiecon, " +
                        "NR_VersaoSistema as versao, " +
                        "DS_IpExterno as urlExterna, " +
                        "CaminhoPdf as caminhoPdf, " +
                        "urlExternaHTML, " +
                        "CaminhoHTML as caminhoHTML, " +
                        "coalesce(cssHTML,'') as cssHTML," +
                        "avisoMostrar, " +
                        "avisoHtml, " +
                        "avisoArquivo, " +
                        "senhaVencimentoDias, senhaComprimento, senhaMinimoMaiusculo, senhaMinimoMinusculo, senhaMinimoNumerico, senhaMinimoAlfanumerico, senhaTentativasLogin, senhaCoincidir, " +
                        "emailDestinatarioSuporte, " +
                        "coalesce(QTD_EmailsEnvioSMTP,0) as qtdeEmailsEnvio, " +
                        "TipoAutenticacaoEmail as TipoAutenticacaoEmail, " +
                        "HR_EmailIntervaloPop3/ 60 as intervaloRecebimentoEmailMinutos, " +
                        "HR_EmailIntervalo / 60 as intervaloEnvioEmailMinutos, " +
                        "tipoAcessoSiecon, usuarioApiSiecon, senhaApiSiecon, urlApiSiecon, TamanhoMaximoAnexos as tamanhoMaximoAnexos, emailErrosAdmin, " +
                        "habilitarEspacoCliente, cast(coalesce(leituraobrigatoria, 0) as int) as leituraobrigatoria, empreendimentoTesteEspacoCliente, " +
                        "NM_ServidorInteg, NM_UsuarioInteg, DS_SenhaUserInteg, DS_PathDBInteg as DS_PathDbInteg, DS_PortaServidorInteg as DS_portaServidorInteg, " +
                        "ID_JornadaSLA, ID_JornadaRecurso, " +
                        "NR_SLACritico, NR_SLAAlerta, cast(coalesce(horasUteisCalcSLA, 0) as bit) as horasUteisCalcSLA, " +
                        "DiasLembrarPesquisaSatisfacao, qtdeAvisosLembrarPesquisa, documentoChamadoConcluido, " +
                        "coalesce(versaoportal,'') as versaoportal, coalesce(PastaInstalacaoCRM,'') as PastaInstalacaoCRM " +
                        "from ope_parametro where cd_bancodados = 1 and cd_mandante = 1";

            // Console.WriteLine(query);
            return connection.QueryFirst<domain.Parametros>(query);
        }

        public static ConfigEspacoCliente consultarEspacoCliente(string cpf, string _connectionString)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"exec dbo.CRM_ConsultarEspacoCliente @cpf='{cpf}'";
            Console.WriteLine(query);
            var result = connection.QueryFirst<ConfigEspacoCliente>(query);
            return result;
        }

        public static BotaoLogin botaoLogin(string _connectionString)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select habilitabotaologin,urliconebotaologin,textoiconebotaologin,alturaiconebotaologin,larguraiconebotaologin,urlexternabotaologin " +
                        "from ope_parametro where cd_bancodados = 1 and cd_mandante = 1";
            return connection.QueryFirst<BotaoLogin>(query);
        }

        public static int AtualizarIntegracaoSieconSP7(string _connectionString, string? NM_ServidorInteg, string? NM_UsuarioInteg, string? DS_SenhaUserInteg, string? DS_PathDbInteg, string? DS_portaServidorInteg)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"UPDATE ope_parametro SET
                NM_ServidorInteg = @NM_ServidorInteg,
                NM_UsuarioInteg = @NM_UsuarioInteg,
                DS_SenhaUserInteg = @DS_SenhaUserInteg,
                DS_PathDBInteg = @DS_PathDbInteg,
                DS_PortaServidorInteg = @DS_portaServidorInteg
                WHERE cd_bancodados = 1 AND cd_mandante = 1";
            return connection.Execute(query, new { NM_ServidorInteg, NM_UsuarioInteg, DS_SenhaUserInteg, DS_PathDbInteg, DS_portaServidorInteg });
        }

        public static List<Jornada> ListarJornadasAtivasSLA(string _connectionString)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT ID_Jornada, NM_Jornada, IN_Tipo, IN_Status, DT_Controle " +
                       "FROM CAD_JORNADA " +
                       "WHERE IN_STATUS = 'A' AND IN_Tipo = 'S' " +
                       "ORDER BY NM_Jornada";
            return connection.Query<Jornada>(query).ToList();
        }

        public static List<Jornada> ListarJornadasAtivasRecurso(string _connectionString)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT ID_Jornada, NM_Jornada, IN_Tipo, IN_Status, DT_Controle " +
                       "FROM CAD_JORNADA " +
                       "WHERE IN_STATUS = 'A' AND IN_Tipo = 'R' " +
                       "ORDER BY NM_Jornada";
            return connection.Query<Jornada>(query).ToList();
        }

        public static int AtualizarJornadas(string _connectionString, int? ID_JornadaSLA, int? ID_JornadaRecurso)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"UPDATE ope_parametro SET
                ID_JornadaSLA = @ID_JornadaSLA,
                ID_JornadaRecurso = @ID_JornadaRecurso
                WHERE cd_bancodados = 1 AND cd_mandante = 1";
            return connection.Execute(query, new { ID_JornadaSLA, ID_JornadaRecurso });
        }

        public static int AtualizarSLA(string _connectionString, int? NR_SLACritico, int? NR_SLAAlerta, bool horasUteisCalcSLA)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"UPDATE ope_parametro SET
                NR_SLACritico = @NR_SLACritico,
                NR_SLAAlerta = @NR_SLAAlerta,
                horasUteisCalcSLA = @horasUteisCalcSLA
                WHERE cd_bancodados = 1 AND cd_mandante = 1";
            return connection.Execute(query, new { 
                NR_SLACritico, 
                NR_SLAAlerta, 
                horasUteisCalcSLA = horasUteisCalcSLA ? 1 : 0 
            });
        }

        public static int AtualizarAvisosEmail(string _connectionString, int? TamanhoMaximoAnexos, string? emailErrosAdmin, int? DiasLembrarPesquisaSatisfacao, int? qtdeAvisosLembrarPesquisa, int? documentoChamadoConcluido)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"UPDATE ope_parametro SET
                TamanhoMaximoAnexos = @TamanhoMaximoAnexos,
                emailErrosAdmin = @emailErrosAdmin,
                DiasLembrarPesquisaSatisfacao = @DiasLembrarPesquisaSatisfacao,
                qtdeAvisosLembrarPesquisa = @qtdeAvisosLembrarPesquisa,
                documentoChamadoConcluido = @documentoChamadoConcluido
                WHERE cd_bancodados = 1 AND cd_mandante = 1";
            return connection.Execute(query, new { 
                TamanhoMaximoAnexos, 
                emailErrosAdmin, 
                DiasLembrarPesquisaSatisfacao, 
                qtdeAvisosLembrarPesquisa, 
                documentoChamadoConcluido 
            });
        }

        public static int AtualizarCaminhos(string _connectionString, string? PastaInstalacaoCRM, string? DS_PathInstallSistemaSiecon)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"UPDATE ope_parametro SET
                PastaInstalacaoCRM = @PastaInstalacaoCRM,
                DS_PathInstallSistemaSiecon = @DS_PathInstallSistemaSiecon
                WHERE cd_bancodados = 1 AND cd_mandante = 1";
            return connection.Execute(query, new { PastaInstalacaoCRM, DS_PathInstallSistemaSiecon });
        }

        public static int AtualizarEspacoCliente(string _connectionString, int habilitarEspacoCliente, int leituraobrigatoria, int empreendimentoTesteEspacoCliente)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"UPDATE ope_parametro SET
                habilitarEspacoCliente = @habilitarEspacoCliente,
                leituraobrigatoria = @leituraobrigatoria,
                empreendimentoTesteEspacoCliente = @empreendimentoTesteEspacoCliente
                WHERE cd_bancodados = 1 AND cd_mandante = 1";
            return connection.Execute(query, new { habilitarEspacoCliente, leituraobrigatoria, empreendimentoTesteEspacoCliente });
        }
    }
}
