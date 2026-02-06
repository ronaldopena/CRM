using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracaoMobussService.Dominio
{
    public class RetornoErroMobuss
    {
        public string status { get; set; }
        public string codigoInterno { get; set; }
        public string mensagem { get; set; }
    }
}
