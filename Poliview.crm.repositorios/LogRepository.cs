using Dapper;
using SQLitePCL;

namespace Poliview.crm.repositorios
{
    public class LogRepository
    {
        private readonly Poliview.crm.repositorios.SqliteConnectionFactory _connectionFactory;

        public enum TipoLog
        {
            info = 1,
            aviso = 2,
            debug = 3,
            erro = 9
        }
        public enum OrigemLog
        {
            mobuss = 9,
            integracao = 1,
            api = 2,
        }

        public class logs 
        {
            public DateTime data { get; set; }
            public string? mensagem { get; set; }
        }

        public LogRepository(SqliteConnectionFactory connectionFactory)
        {
      
            SQLitePCL.raw.SetProvider(new SQLite3Provider_e_sqlite3());
            _connectionFactory = connectionFactory;
            this.InitializeDatabase().Wait();
        }

        public async Task InitializeDatabase()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
            CREATE TABLE IF NOT EXISTS Logs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                chamado int NULL,
                origem TEXT NOT NULL,
                tipo TEXT NOT NULL,
                data DATETIME NOT NULL,
                mensagem TEXT NOT NULL                
            );";
            await connection.ExecuteAsync(sql);
        }

        public async Task Log(OrigemLog origem, TipoLog tipo, string mensagemlog, int idchamado)
        {
            var origemstr = Enum.GetName(typeof(OrigemLog), origem);
            var tipostr = Enum.GetName(typeof(TipoLog), tipo);
            
            Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} | {origem} | {tipo} | {mensagemlog} | {idchamado} ");
            if (tipo == (TipoLog)9)
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = "INSERT INTO Logs (chamado, origem, tipo, data, mensagem) VALUES (@chamado, @origem, @tipo, @data, @mensagem)";
                await connection.ExecuteAsync(sql, new { chamado = idchamado, origem = origemstr, tipo = tipostr, data = DateTime.Now, mensagem = mensagemlog });
                connection.Close();
            }
            
        }

        public async Task<IEnumerable<logs>> ListaErros(string datahora)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = $"select data, mensagem from Logs where data >= '${datahora}'";
            return await connection.QueryAsync<logs>(sql);
        }

    }
}
