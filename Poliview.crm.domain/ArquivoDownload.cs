using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class ArquivoDownload
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public string? link { get; set; }
        public string? nome { get; set; }
        public int idgrupomidia { get; set; }
        public int variosempreendimentos { get; set; }
        public int idempreendimento { get; set; }
        public string? empreendimento { get; set; }
        public int idbloco { get; set; }
        public string? bloco { get; set; }
        public int idunidade { get; set; }
        public string? unidade { get; set; }
        public DateTime datainicial { get; set; }
        public DateTime datafinal { get; set; }
        public string? empreendimentos { get; set; }
    }
}