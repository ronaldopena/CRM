using apimobuss.Dominio;
using Dapper;
using System.Data.SqlClient;

namespace apimobuss.Repositorio
{
    public enum tpLog
    {
        aviso = 1,
        informacao = 2,
        debug = 5,
        erro = 9
    }

    public class LogRepositorio : ILogRepositorio
    {

        private readonly string _connectionString;

        public LogRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }
      
        public void incluir(int chamado, string mensagem, tpLog tipoLog)
        {
            var tipo = (int)tipoLog;

            if (tipo == (int)tpLog.erro)
            {
                Console.WriteLine(mensagem);
            }
            using var connection = new SqlConnection(_connectionString);
            var query = string.Format("insert into OPE_LOG " + 
                                      "([chamado],[idorigem],[idtipo],[data],[mensagem]) values ({0},{1},{2},getDate(),'{3}') "
                                      , chamado, 9, tipo, mensagem);
            var result = connection.Query(query);            
        }
    }
}
