using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class ArquivoUsuario
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public string? link { get; set; }
    }

    public class ArquivosUsuarioResposta: IRetorno
    {
        public IEnumerable<ArquivoUsuario>? objeto { get; set; }
    }
}
