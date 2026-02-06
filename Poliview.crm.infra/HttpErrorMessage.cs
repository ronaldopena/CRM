using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.infra
{
    public class HttpErrorMessage
    {
        public HttpErrorMessage(int statusCode, string message)
        {
            this.statusCode = statusCode;
            this.message = message; 
        }

        public int statusCode { get; }
        public string? message { get; }

    }
}
