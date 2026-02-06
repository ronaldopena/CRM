using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poliview.crm.conexao.services
{
    public static class Conexao
    {
        public static FbConnection Firebird(string connectionstring)
        {
            return new FbConnection(connectionstring);
        }

        public static SqlConnection SqlServer(string connectionstring)
        {
            return new SqlConnection(connectionstring);
        }

    }
}
