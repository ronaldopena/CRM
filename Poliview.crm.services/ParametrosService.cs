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
                        "tipoAcessoSiecon, usuarioApiSiecon, senhaApiSiecon, urlApiSiecon, tamanhoMaximoAnexos, emailErrosAdmin, " +
                        "habilitarEspacoCliente, empreendimentoTesteEspacoCliente, " +
                        "NM_ServidorInteg, NM_UsuarioInteg, DS_SenhaUserInteg, DS_PathDBInteg as DS_PathDbInteg, DS_PortaServidorInteg as DS_portaServidorInteg " +
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
    }
}
