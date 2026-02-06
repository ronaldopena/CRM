using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class BlocosIntegracao
    {
        public int idbloco { get; set; }
        public int codigoblocosp7 { get; set; }
        public int codigoempreendimentosp7 { get; set; }
        public string nome { get; set; }
        public string abreviatura { get; set; }
        public string endereco { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string cep { get; set; }
        public DateTime datahabitese { get; set; }
        public DateTime datahoraultimaatualizacao { get; set; }
    }
}
