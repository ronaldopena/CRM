using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface ISLAService
    {
        public Retorno RecalcularSLA();
    }

    public class SLAService : ISLAService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public SLAService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public Retorno RecalcularSLA()
        {
            var retorno = new Retorno();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "exec dbo.CRM_RECALCULAR_SLA_CHAMADOS ";
                var result = connection.Query(query);
                retorno.mensagem = "OK";
                retorno.objeto = result;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.sucesso = false;
                return retorno;
            }
        }

		public Retorno MonitoramentoSLA()
		{
			var retorno = new Retorno();
			try
			{
				using var connection = new SqlConnection(_connectionString);
				var query = "exec dbo.CRM_MONITORAR_SLA ";
				var result = connection.Query(query);
				retorno.mensagem = "OK";
				retorno.objeto = result;
				retorno.sucesso = true;
				return retorno;
			}
			catch (Exception e)
			{
				retorno.mensagem = e.Message;
				retorno.objeto = null;
				retorno.sucesso = false;
				return retorno;
			}
		}


		public Retorno CalcularSLAPrevisto(DateTime dataabertura, int idjornada, int totalminutos, string tiposla, int detalhe, string connectionString)
        {            
            var retorno = new Retorno();
            try
            {
                using var connection = new SqlConnection(connectionString);
                var query = $"exec [CRM_Calcular_SLA_Previsto] @DataAbertura='${dataabertura.ToString("dd/MM/yyyy HH:mm:ss")}', @idJornada=${idjornada}, @totalMinutos=${totalminutos}, @tiposla='${tiposla}', @detalhe=${detalhe}";
                var result = connection.Query(query);
                retorno.mensagem = "OK";
                retorno.objeto = result;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.sucesso = false;
                return retorno;
            }
        }
    }
}
