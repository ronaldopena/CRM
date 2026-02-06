using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Empresa
    {
        public int id { get; set; }
        public string nomeempresa { get; set; }
        public string dominioempresa { get; set; }
        public int idcontaemail { get; set; }
        public int ativo { get; set; }
        public string principal { get; set; }
        public string principaldark { get; set; }
        public string principallight { get; set; }
        public string fundo { get; set; }
        public string texto { get; set; }
        public string urllogo { get; set; }
        public string urlimgprincipal { get; set; }
    }
}