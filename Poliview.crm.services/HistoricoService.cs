using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public class HistoricoService : IHistoricoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public HistoricoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public bool Incluir(HistoricoChamadosIncluirRequisicao obj)
        {      
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec API_INCLUIR_HISTORICO_CHAMADO " +
                            $"@chamado = {obj.idchamado}, " +
                            $"@ocorrencia = {obj.idocorrencia}, " +
                            $"@descricao = '{obj.descricao}', " +
                            $"@visibilidade = {obj.visibilidade}, " +
                            $"@idusuario = {obj.idusuario} ";

                Console.WriteLine(query);
                connection.Execute(query);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<ChamadoHistorico> Listar(HistoricoChamadosRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"exec CRM_Listar_Historico " +
                        $"@CD_CHAMADO = {obj.idchamado}, " +
                        $"@CD_OCORRENCIA = {obj.idocorrencia}, " +
                        $"@VISIBILIDADE = {obj.visibilidade} ";

            var result = connection.Query<ListarHistoricoResposta>(query);

            IEnumerable<ChamadoHistorico> retorno = result.Select(x => new ChamadoHistorico
            {
                datahora = x.DATA_CONTROLE_LOG,
                descricao = x.DS_DESCRICAO,
                status = x.DS_STATUS
            });

            return retorno;
        }
    }
}

/*
public IEnumerable<ChamadoHistorico> Listar(HistoricoChamadosRequisicao obj);
{
    using var connection = new SqlConnection(_connectionString);
    var query = $"select * from OPE_CONFIG where chaveacesso='${chaveacesso}'";
    Console.WriteLine(query);
    return connection.QueryFirst<Acesso>(query);
}

public Boolean Incluir(HistoricoChamadosIncluirRequisicao obj);
{
    using var connection = new SqlConnection(_connectionString);
    var query = $"select * from OPE_CONFIG where chaveacesso='${chaveacesso}'";
    Console.WriteLine(query);
    return connection.QueryFirst<Acesso>(query);
    return true;
}
*/