using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class LeituraNotificacaoRequisicao
    {
        public string? idnotificacao { get; set; }
        public int idunidade { get; set; }
        public int idorigem { get; set; }
        public string? cpfcnpj { get; set; }
    }
}
