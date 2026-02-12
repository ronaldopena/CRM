using Dapper;
using Poliview.crm.domain;
using Poliview.crm.models;
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

        public async Task<Retorno> Create(Servicos obj)
        {
            var ret = new Retorno();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = @"INSERT INTO OPE_SERVICOS (NomeServico, CaminhoServico, ExecutavelServico, Ativo)
                             VALUES (@NomeServico, @CaminhoServico, @ExecutavelServico, @Ativo)";
                await connection.ExecuteAsync(query, new
                {
                    obj.NomeServico,
                    obj.CaminhoServico,
                    obj.ExecutavelServico,
                    Ativo = string.IsNullOrEmpty(obj.Ativo) ? "N" : (obj.Ativo == "S" || obj.Ativo == "1" ? "S" : "N")
                });
                ret.sucesso = true;
                ret.mensagem = "Serviço incluído com sucesso.";
            }
            catch (Exception ex)
            {
                ret.sucesso = false;
                ret.mensagem = ex.Message;
            }
            return ret;
        }

        public async Task<Retorno> Update(Servicos obj)
        {
            var ret = new Retorno();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = @"UPDATE OPE_SERVICOS SET CaminhoServico = @CaminhoServico, ExecutavelServico = @ExecutavelServico, Ativo = @Ativo
                             WHERE NomeServico = @NomeServico";
                var rows = await connection.ExecuteAsync(query, new
                {
                    obj.NomeServico,
                    obj.CaminhoServico,
                    obj.ExecutavelServico,
                    Ativo = string.IsNullOrEmpty(obj.Ativo) ? "N" : (obj.Ativo == "S" || obj.Ativo == "1" ? "S" : "N")
                });
                ret.sucesso = rows > 0;
                ret.mensagem = rows > 0 ? "Serviço alterado com sucesso." : "Serviço não encontrado.";
            }
            catch (Exception ex)
            {
                ret.sucesso = false;
                ret.mensagem = ex.Message;
            }
            return ret;
        }

        public async Task<Retorno> Delete(string nomeServico)
        {
            var ret = new Retorno();
            try
            {
                if (string.IsNullOrWhiteSpace(nomeServico))
                {
                    ret.sucesso = false;
                    ret.mensagem = "Nome do serviço não informado.";
                    return ret;
                }
                using var connection = new SqlConnection(_connectionString);
                var query = "DELETE FROM OPE_SERVICOS WHERE NomeServico = @NomeServico";
                var rows = await connection.ExecuteAsync(query, new { NomeServico = nomeServico });
                ret.sucesso = rows > 0;
                ret.mensagem = rows > 0 ? "Serviço excluído com sucesso." : "Serviço não encontrado.";
            }
            catch (Exception ex)
            {
                ret.sucesso = false;
                ret.mensagem = ex.Message;
            }
            return ret;
        }
    }
}

