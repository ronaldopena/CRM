using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class RelatorioCienciaArquivoRequisicao
    {
        public DateTime datainicial { get; set; }
        public DateTime datafinal { get; set; }
        public string? idarquivoList { get; set; }
        public string? idempreendimentoList { get; set; }
        public int idbloco { get; set; }
        public int idunidade { get; set; }
        public int idorigem { get; set; }
    }
}
