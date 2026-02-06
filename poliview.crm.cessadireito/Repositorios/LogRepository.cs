using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poliview.crm.cessadireito.Repositorios
{
    public class LogRepository
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public LogRepository(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            this.InitializeDatabase().Wait();
        }

        public async Task InitializeDatabase()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
            CREATE TABLE IF NOT EXISTS Logs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                LogLevel TEXT NOT NULL,
                Message TEXT NOT NULL,
                CreatedAt DATETIME NOT NULL
            );";
            await connection.ExecuteAsync(sql);
        }

        public async Task Log(LogLevel logLevel, string message)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "INSERT INTO Logs (LogLevel, Message, CreatedAt) VALUES (@LogLevel, @Message, @CreatedAt)";
            await connection.ExecuteAsync(sql, new { LogLevel = logLevel.ToString(), Message = message, CreatedAt = DateTime.UtcNow });
        }

        public async Task<IEnumerable<dynamic>> GetLogsByLevel(string logLevel)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM Logs WHERE LogLevel = @LogLevel";
            return await connection.QueryAsync(sql, new { LogLevel = logLevel });
        }
    }
}
