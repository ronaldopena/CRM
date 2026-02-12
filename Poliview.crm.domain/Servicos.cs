using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Servicos
    {
        public string NomeServico { get; set; } = "";
        public string CaminhoServico { get; set; } = "";
        public string ExecutavelServico { get; set; } = "";
        public string Ativo { get; set; } = "S";
        public DateTime DataUltimaExecucao { get; set; }
        public DateTime DataUltimoProcessamento { get; set; }
    }

}
