using Dapper;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.infra
{
    public static class Relatorio
    {
        public static string Incluir(string relatorio, string _connectionString)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"insert into OPE_RELATORIO_STATUS (nomeRelatorio) values ('{relatorio}')";
                connection.Open();
                connection.Execute(query);
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string SalvarStatus(string relatorio, string status, string _connectionString, int concluido = 0)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"update OPE_RELATORIO_STATUS  set status='{status}', concluido={concluido} ";

                if (concluido == 1)
                    query += ", dataconclusao=GetDate() ";

                query += $"where nomeRelatorio='{relatorio}'";
                connection.Open();
                connection.Execute(query);
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string Excluir(string relatorio, string _connectionString)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"DELETE FROM OPE_RELATORIO_STATUS where nomeRelatorio='{relatorio}'";
                connection.Open();
                connection.Execute(query);
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static StatusRelatorio? ConsultarStatus(string relatorio, string _connectionString)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select concluido, status FROM OPE_RELATORIO_STATUS where nomeRelatorio='{relatorio}'";
                connection.Open();
                var result = connection.Query<StatusRelatorio>(query).FirstOrDefault();
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static string GerarNomeRelatorio(string prefixo)
        {
            DateTime agora = DateTime.Now;
            string dataFormatada = agora.ToString("yyyyMMddHHmmss");
            return $"{prefixo}-" + dataFormatada + ".xlsx";
        }
    }
}
