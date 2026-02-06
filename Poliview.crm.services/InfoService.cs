using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.services
{
    public class InfoService : IInfoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public InfoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public object execute()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "exec dbo.crm_config";
            Console.WriteLine(query);
            return connection.QueryFirst<object>(query);
        }

        public Object execSQL(string sql)
        {
            using var connection = new SqlConnection(_connectionString);
            // ATENÇÃO: Este método executa SQL dinâmico e deve ser usado com extrema cautela
            // Considere remover este método ou implementar validação rigorosa
            var query = sql;
            Console.WriteLine(query);
            return connection.Query<object>(query);
        }

    }
}
