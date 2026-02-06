using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class LeituraArquivoDownloadRequisicao
    {
        public int idarquivo { get; set; }
        public int idunidade { get; set; }
        public int idorigem { get; set; }
        public string? cpfcnpj { get; set; }
    }
}
