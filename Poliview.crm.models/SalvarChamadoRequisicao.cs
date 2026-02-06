using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class SalvarChamadoRequisicao
    {
        public int idempreendimento { get; set; }
        public int idbloco { get; set; }
        public int idunidade { get; set; }
        public int idcliente { get; set; }
        public int idcontrato { get; set; }
        public int idocorrencia { get; set; }
        public string? descricao { get; set; }
        public int origemchamado { get; set; }
        public string? anexos { get; set; }
    }
}
