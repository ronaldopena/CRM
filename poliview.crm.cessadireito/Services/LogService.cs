using Microsoft.Extensions.Logging;
using poliview.crm.cessadireito.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poliview.crm.cessadireito.Services
{
    public class LogService
    {
        private readonly ILogger<LogService> _logger;
        private readonly LogRepository _logRepository;

        public LogService(ILogger<LogService> logger, LogRepository logRepository)
        {
            _logger = logger;
            _logRepository = logRepository;
        }

        public async Task Log(LogLevel logLevel, string logText)
        {
            if (logLevel == LogLevel.Information) 
            {
                _logger.LogInformation(logText);
                await _logRepository.Log(LogLevel.Information, logText);
            } 
            else if (logLevel == LogLevel.Warning) 
            {
                _logger.LogWarning(logText);
                await _logRepository.Log(LogLevel.Warning, logText);
            }
            else if (logLevel == LogLevel.Error)
            {
                _logger.LogError(logText);
                await _logRepository.Log(LogLevel.Error, logText);
            }
            else if (logLevel == LogLevel.Debug)
            {
                _logger.LogDebug(logText);
                await _logRepository.Log(LogLevel.Debug, logText);
            }
        }
    }
}
