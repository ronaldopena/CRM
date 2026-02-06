using Microsoft.Extensions.Configuration;
using Poliview.crm.repositorios;
using static Poliview.crm.repositorios.LogRepository;

namespace Poliview.crm.services
{
    public interface ILogService
    {
        public Task Log(OrigemLog origem, TipoLog tipo, string mensagemlog, int idchamado = 0);        
    }

    public class LogService : ILogService
    {
        private readonly LogRepository _logRepository;
        private readonly IConfiguration _config;
        private readonly Boolean _mostrarsomenteerros;

        public LogService(LogRepository logRepository, IConfiguration configuration)
        {
            _logRepository = logRepository;
            _config = configuration;
            _mostrarsomenteerros = Convert.ToBoolean(_config["mostrarsomenteerros"]);
        }

        public async Task Log(OrigemLog origem, TipoLog tipo, string mensagemlog, int idchamado = 0)
        {
            
            if (tipo == TipoLog.info)
            {
                if (!_mostrarsomenteerros) await _logRepository.Log(origem, tipo, mensagemlog, idchamado);
            }
            else if (tipo == TipoLog.aviso)
            {
                if (!_mostrarsomenteerros) await _logRepository.Log(origem, tipo, mensagemlog, idchamado);
            }
            else if (tipo == TipoLog.erro)
            {
                await _logRepository.Log(origem, tipo, mensagemlog, idchamado);
            }
            else if (tipo == TipoLog.debug)
            {
                if (!_mostrarsomenteerros) await _logRepository.Log(origem, tipo, mensagemlog, idchamado);
            }
        }        
    }
}
