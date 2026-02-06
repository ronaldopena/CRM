using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class SalvarAnexosChamadoRequisicao
    {
        public int idchamado { get; set; }
        public string? anexos { get; set; }
    }

    public class SalvarAnexosChamadoResposta
    {
        public string? mensagem { get; set; }
    }
}
