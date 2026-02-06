using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class MenusPermissao
    {
        public int boleto { get; set; }
        public int FichaFinanceira { get; set; }
        public int InformeRendimento { get; set; }
        public int Chamado { get; set; }
        public int habilitarEspacoCliente { get; set; }
        public int empreendimentoTesteEspacoCliente { get; set; }
    }
}
