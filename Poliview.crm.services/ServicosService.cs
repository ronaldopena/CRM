using Dapper;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Poliview.crm.services
{
    public class ServicosService: IServicosService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public ServicosService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        protected void AlterarDataExecucao(Servicos servico)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $@"UPDATE OPE_SERVICOS
                            set DataUltimaExecucao=GetDate()                                
                            where NomeServico=@NomeServico";
            var parameters = new
            {
                NomeServico = servico.NomeServico
            };

            connection.Execute(query, parameters);

        }

        protected void AlterarDataProcessamento(Servicos servico)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $@"UPDATE OPE_SERVICOS
                            set DataUltimoProcessamento=GetDate()                                
                            where NomeServico=@NomeServico";
            var parameters = new
            {
                NomeServico = servico.NomeServico
            };
            connection.Execute(query, parameters);
        }

        public void ExecutarAtivos()
        {
            var servicos = ListarAtivos();

            foreach (var servico in servicos)
            {
                Executar(servico);
            }
        }

        protected void Executar(Servicos servico)
        {
            var retorno = "";
            try
            {
                AlterarDataProcessamento(servico);
                var comando = $@"{servico.CaminhoServico}\{servico.ExecutavelServico}";
                Console.WriteLine($@"Executando serviço: {servico.NomeServico} comando: {comando}");
                var utilService = new UtilsService(_configuration);
                retorno = utilService.RunCommand(comando);
                AlterarDataExecucao(servico);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Servicos> ListarTodos()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select * from OPE_SERVICOS";
            return connection.Query<Servicos>(query).ToList();
        }

        public List<Servicos> ListarAtivos()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select * from OPE_SERVICOS where Ativo='S'";
            return connection.Query<Servicos>(query).ToList();
        }
    }
}

