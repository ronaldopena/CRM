using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class MensagemUsuario
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public string? mensagem { get; set; }
        public int tipomensagem { get; set; }
        public string? urlimagem { get; set; }
        public string? linkimagem { get; set; }
    }

    public class MensagensUsuarioResposta: IRetorno
    {
        public IEnumerable<MensagemUsuario>? objeto { get; set; }
    }
}
