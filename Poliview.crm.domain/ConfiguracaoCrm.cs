using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracaoMobussService.dominio
{
    public class ConfiguracaoCrm
    {
        public int integracaoMobuss { get; set; }
        public string UrlApiMobuss { get; set; }
        public string TokenApiMobuss { get; set; }
        public string TokenApiCrmMobuss { get; set; }
        public int TipoOcorrenciaMobuss { get; set; }
        public int StatusCancelamentoMobuss { get; set; }
        public int StatusEncerramentoMobuss { get; set; }
        public int idOcorrenciaRaizIntegracaoMobuss { get; set; }
    }
}
