using System.Data.SqlClient;
using Dapper;
using apimobuss.Entidades;


namespace apimobuss.Repositorio
{
    public sealed class ChamadoRepositorio : IChamadoRepositorio
    {
        private readonly string _connectionString;

        public ChamadoRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public object fechar(ChamadoFecharRequisicao dados)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = string.Format("EXEC [dbo].[API_MOBBUS_CHAMADO_FECHAR] " +
            "@CDCHAMADO = {0}," +
            "@CDOCORRENCIA = {1}," +
            "@DATAHORAENCERRAMENTO = '{2}', " +
            "@SOLUCAO = '{3}'", dados.idchamado, dados.idocorrencia, dados.datahorasolucao, dados.solucao);

            var result = connection.Query(query);

            return result;
        }

        public NovoChamado incluir(ChamadoIncluirRequisicao dados)
        {
            using var connection = new SqlConnection(_connectionString);

            dados.descricaochamado = dados.descricaochamado.Substring(0, 1024);

            var query = string.Format("EXEC [dbo].[API_MOBBUS_CHAMADO_INCLUIR] " +
                                      "@IDCLIENTE = {0}, " +
                                      "@IDUNIDADE = {1}, " +
                                      "@DESCRICAOCHAMADO = N'{2}'", dados.idcliente, dados.idunidade, dados.descricaochamado);
            var result = connection.Query<NovoChamado>(query).First();

            return result;
        }

        public object cancelar(ChamadoCancelarRequisicao dados)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = string.Format("EXEC [dbo].[API_MOBBUS_CHAMADO_CANCELAR] " +
            "@CDCHAMADO = {0}," +
            "@CDOCORRENCIA = {1}," +
            "@DATAHORACANCELAMENTO = '{2}', " +
            "@MOTIVOCANCELAMENTO = '{3}'", dados.idchamado, dados.idocorrencia, dados.datahoracancelamento, dados.motivo);

            var result = connection.Query(query);

            return result;
        }

        public Boolean TokenValido(string token)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = string.Format("select TokenApiCrmMobuss from OPE_PARAMETRO where CD_BancoDados=1 and CD_Mandante=1 ");

            var result = connection.Query<string>(query).First();

            // "f0dbc5d6-e43b-4249-b1ad-777473b55b7c"

            return (token == result);
        }

        public string FormatData(string data)
        {
            //           1
            // 0123456789012345
            // 01/01/2000 10:10
            var ret = data.Substring(0, 2) + // dia
                      data.Substring(3, 2) + // mes
                      data.Substring(6, 4) + // ano
                      data.Substring(11, 5); // hora:minuto

            return ret;
        }

        public Boolean validarProponentePrincipal(int idunidade, int idcliente)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = string.Format("SELECT DISTINCT cl.CD_Cliente AS idproponenteprincipal " +
                "FROM CAD_UNIDADE un " +
                "LEFT JOIN CAD_BLOCO bl ON bl.CD_BlocoSP7 = un.CD_BlocoSP7 " +
                "    AND bl.CD_EmpreeSP7 = un.CD_EmpreeSP7 " +
                "LEFT JOIN CAD_EMPREENDIMENTO emp ON emp.CD_EmpreeSP7 = un.CD_EmpreeSP7 " +
                "LEFT JOIN cad_contrato ct ON ct.CD_EmpreeSP7 = emp.CD_EmpreeSP7 " +
                "    AND ct.CD_BlocoSP7 = bl.CD_BlocoSP7 " +
                "    AND ct.NR_UnidadeSP7 = un.NR_UnidadeSP7 " +
                "LEFT JOIN CAD_CLIENTE cl ON cl.CD_ClienteSP7 = ct.CD_ClienteSP7 " +
                "LEFT JOIN CAD_PROPONENTE prop ON prop.CD_ClienteSP7 = cl.CD_ClienteSP7 " +
                "    AND prop.CD_ContratoSP7 = ct.CD_ContratoSP7 " +
                "where cl.CD_Cliente is not null and un.CD_Unidade={0} " +
                "UNION	 " +
                "SELECT CD_Cliente AS idproponenteprincipal " +
                "FROM CAD_CLIENTE_CRM crm " +
                "where crm.CD_Unidade={0}", idunidade);

            var registro = connection.Query<int>(query).First();

            return (registro == idcliente);

        }

        public string retornaDadosUnidade(int idunidade)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = string.Format("select " +
                "('Emprendimento: ' + e.NM_Empree +' | Bloco: ' + b.NM_Bloco +' | Unidade: ' + u.NR_UnidadeSP7) as unidade " +
                "from cad_unidade u " +
                "left join CAD_EMPREENDIMENTO e on e.CD_EmpreeSP7 = u.CD_EmpreeSP7 " +
                "left join CAD_BLOCO b on b.CD_EmpreeSP7 = u.CD_EmpreeSP7 and b.CD_BlocoSP7 = u.CD_BlocoSP7 " +
                "where u.CD_Unidade = {0} ", idunidade);

            var registro = connection.Query<string>(query).First();

            return (registro);

        }

    }
}
