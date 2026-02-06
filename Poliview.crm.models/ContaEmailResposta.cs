using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class ContaEmailResposta : IRetorno
    {
        public IEnumerable<ContaEmail> objeto { get; set; }
    }

    public class ContaEmailporIdResposta : IRetorno
    {
        public ContaEmail objeto { get; set; }
    }
}
