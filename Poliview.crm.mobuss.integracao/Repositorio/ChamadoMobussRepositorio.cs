using IntegracaoMobussService.dominio;
using System.Data.SqlClient;
using Dapper;

namespace IntegracaoMobussService.Repositorio
{
    public class ChamadoMobussRepositorio
    {
        private readonly string _connectionString;

        public ChamadoMobussRepositorio(string connectioString)
        {
            _connectionString = connectioString;
        }

        public IEnumerable<ChamadoMobuss> Listar(int idOcorrenciaRaiz, int idStatusEmAtendimento)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "EXEC DBO.CRM_LISTA_CHAMADOS_ENVIAR_MOBUSS ";
            var result = connection.Query<ChamadoMobuss>(query);

            connection.Close();
            connection.Dispose();

            Console.WriteLine(query);

            return result;

        }

        public Boolean AlterarStatusChamadoMobuss(int chamado, int status, string mensagem)
        {
            var result = false;

            using var connection = new SqlConnection(_connectionString);
            var query = String.Format("update ope_chamado set idStatusIntegracaoMobuss={1}, mensagemIntegracaoMobuss='{2}' where cd_chamado={0}", chamado, status, mensagem);

            try
            {
                connection.Execute(query);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            finally {
                connection.Close();
                connection.Dispose();
            }

            return result;

        }

        public void Log(int tipo, string mensagem, int chamado = 0)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = String.Format("insert into OPE_LOG " +
                                      "(chamado, idOrigem, idTipo, Data, mensagem) "+
                                      "values " +
                                      "({0},9,{1},GetDate(),'{2}')", chamado,tipo,mensagem);

            var data = DateTime.Now.ToString("dd/MM/yyyy hh:mm");
            var msg = data + " | " + tipo + " | " + mensagem;

            Console.WriteLine(msg);

            if (tipo == 9)
            {
                this.EnviarEmail(msg, chamado);
                if (chamado > 0)
                {
                    IncrementaTentativaIntegracao(chamado);
                }
            }
            
            connection.Execute(query);

            connection.Close();
            connection.Dispose();

        }

        private void EnviarEmail(string mensagem, int chamado)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = String.Format("EXEC dbo.CRM_ENVIAR_EMAIL_ERRO_MOBUSS @idchamado={0}, @mensagem='{1}'", chamado, mensagem);
            connection.Execute(query);
            connection.Close();
            connection.Dispose();
        }

        public void PrepararIntegracao()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = String.Format("EXEC dbo.CRM_PREPARAR_INTEGRACAO_MOBUSS");
            connection.Execute(query);
            connection.Close();
            connection.Dispose();
        }

        public void IncrementaTentativaIntegracao(int chamado)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = String.Format("update OPE_CHAMADO set tentativasIntegracao=tentativasIntegracao+1 where cd_chamado={0}", chamado);
            connection.Execute(query);
            connection.Close();
            connection.Dispose();
        }
    }
}
