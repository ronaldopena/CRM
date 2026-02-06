using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracaoMobussService.Dominio
{
    public class RetornoErroMobuss
    {
        public string status { get; set; } = string.Empty;
        public string codigoInterno { get; set; } = string.Empty;
        public string mensagem { get; set; } = string.Empty;
    }
}
