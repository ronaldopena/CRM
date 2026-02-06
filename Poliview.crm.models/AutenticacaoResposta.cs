using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class AutenticacaoResposta: IRetorno
{
        public Usuario? objeto { get; set; }

    }
}
