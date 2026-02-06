using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class EmpreendimentosIntegracao
    {
        public int empreendimentosp7 { get; set; }
        public string empreendimento { get; set; }
        public string abreviatura { get; set; }
        public string endereco { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string cep { get; set; }
        public int entidade { get; set; }
        public int regiao { get; set; }
        public int tipoempreendimento { get; set; }
        public int codigomunicipio { get; set; }
        public int codigopadrao { get; set;}
        public int boleto { get; set; }
        public int fichafinanceira { get; set; }
        public int informerendimento { get; set; }
        public int chamado { get; set; }
        public int idempresa { get; set; }
        public DateTime datahoraultimaatualizacao { get; set; }
    }
}
