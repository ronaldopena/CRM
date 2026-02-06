using System.Text.Json;
using Poliview.crm.domain;
using Poliview.crm.infra;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;

namespace poliview.crm.instalador
{
    public class Sqlserver
    {
        private static string NOME_ARQUIVO_JSON = "instalador.json";

        public static string RetornarConnectionString()
        {
            var pathexe = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            var configcrm = JsonSerializer.Deserialize<ConfigInstaladorCRM>(File.ReadAllText(pathexe + $"\\{NOME_ARQUIVO_JSON}"), options);

            var txtServidor = Util.Decodificar(configcrm.ServidorSqlServer);
            var txtBanco = Util.Decodificar(configcrm.BancoSqlServer);
            var txtInstancia = Util.Decodificar(configcrm.InstanciaSqlServer);
            var txtUsuario = Util.Decodificar(configcrm.UsuarioSqlServer);
            var txtSenha = Util.Decodificar(configcrm.SenhaSqlServer);
            var srv = txtServidor;

            if (!String.IsNullOrEmpty(txtInstancia))
            {
                srv += "\\" + txtInstancia;
            }

            var conexao = "";

            if (!string.IsNullOrEmpty(txtServidor) && !string.IsNullOrEmpty(txtSenha) && !string.IsNullOrEmpty(txtUsuario))
            {
                conexao = @"Server=" + srv + ";Database=" + txtBanco + ";User Id=" + txtUsuario + ";Password=" + txtSenha + ";";
            }

            return conexao;

        }


        public static string ConnectionString(ConfigInstaladorCRM configcrm)
        {
            var txtServidor = Util.Decodificar(configcrm.ServidorSqlServer);
            var txtInstancia = Util.Decodificar(configcrm.InstanciaSqlServer);
            var txtUsuario = Util.Decodificar(configcrm.UsuarioSqlServer);
            var txtSenha = Util.Decodificar(configcrm.SenhaSqlServer);
            var srv = txtServidor;

            if (!String.IsNullOrEmpty(txtInstancia))
            {
                srv += "\\" + txtInstancia;
            }
            else
            {
                srv += "," + configcrm.PortaSqlServer.ToString();
            }

            var conexao = "";

            if (!string.IsNullOrEmpty(txtServidor) && !string.IsNullOrEmpty(txtSenha) && !string.IsNullOrEmpty(txtUsuario))
            {
                conexao = @"Server=" + srv + ";Database=master;User Id=" + txtUsuario + ";Password=" + txtSenha + ";";
            }

            return conexao;

        }
        public static string runSqlScript(string script, string connectionString, string versaoScript = "")
        {
            var retorna = "OK";
            var cmdStr = "";
            try
            {

                // split script on GO command
                System.Collections.Generic.IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$",
                                         RegexOptions.Multiline | RegexOptions.IgnoreCase);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    foreach (string commandString in commandStrings)
                    {
                        if (commandString.Trim() != "")
                        {
                            cmdStr = commandString;
                            //cmdStr = commandString.Replace("\n", "");
                            //cmdStr = cmdStr.Replace("\r", "");
                            using (var command = new SqlCommand(cmdStr, connection))
                            {
                                try
                                {
                                    if (commandString.Trim() != "")
                                    {
                                        command.CommandText = commandString;
                                        command.ExecuteNonQuery();
                                    }
                                    // command.ExecuteNonQuery();
                                }
                                catch (SqlException ex)
                                {
                                    // string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                    // retorna = string.Format("Please check the SqlServer script.\nLine: {1} \nError: {2} \nSQL Command: \n{3}", ex.LineNumber, ex.Message, spError);
                                    retorna = ex.Message + "\n\n" + versaoScript + "\n\n" + commandString;
                                }
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                retorna = ex.Message;
            }
            return retorna;
        }

        public static string RetornaBancoPadrao()
        {
            var pathexe = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            var configcrm = JsonSerializer.Deserialize<ConfigInstaladorCRM>(File.ReadAllText(pathexe + $"\\{NOME_ARQUIVO_JSON}"), options);
            return Util.Decodificar(configcrm.BancoSqlServer);
        }

    }
}
