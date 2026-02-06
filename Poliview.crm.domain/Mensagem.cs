using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Mensagem
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public string? mensagem { get; set; }
        public int tipomensagem { get; set; }
        public string? urlimagem { get; set; }
        public string? linkimagem { get; set; }
        public int variosempreendimentos { get; set; }
        public int idempreendimento { get; set; }
        public string? empreendimento { get; set; }
        public int idbloco { get; set; }
        public string? bloco { get; set; }
        public int idunidade { get; set; }
        public string? unidade { get; set; }
        public DateTime datainicial { get; set; }
        public DateTime datafinal { get; set; }
        public int fiquepordentro { get; set; }
        public DateTime datainicialfiquepordentro { get; set; }
        public DateTime datafinalfiquepordentro { get; set; }
        public string? empreendimentos { get; set; }
        public int idgrupomidia { get; set; }
        // public IEnumerable<MensagemEmpreendimentos> empreendimentos { get; set; }
    }

}