using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.services
{
    public interface IInfoService
    {
        public Object execute();
        public Object execSQL(string sql);
    }
}
