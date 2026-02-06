using System.Text.Json;
using Poliview.crm.domain;
using Poliview.crm.infra;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;

namespace poliview.crm.instalador
{
    public class ConfigCrmFile
    {
        public string NomeCliente { get; set; }
        public string AcessoExterno { get; set; }
        public int PortaApiCrm { get; set; }
        public string PastaInstalacaoCRM { get; set; }
        public string ConnectionStringMSSQL { get; set; } = "";
        public string ConnectionStringFB { get; set; } = "";
        public string ServidorSqlServer { get; set; } = "";
        public string BancoSqlServer { get; set; } = "";
        public string UsuarioSqlServer { get; set; } = "";
        public string SenhaSqlServer { get; set; } = "";
        public string ServidorFirebird { get; set; } = "";
        public string BancoFirebird { get; set; } = "";
        public string UsuarioFirebird { get; set; } = "";
        public string SenhaFirebird { get; set; } = "";
        public int PortaFirebird { get; set; } = 3050;

        public static ConfigCrmFile LoadFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };
                return JsonSerializer.Deserialize<ConfigCrmFile>(jsonContent, options) ?? new ConfigCrmFile();
            }
            return new ConfigCrmFile();
        }

        public void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonContent = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, jsonContent);
        }
    }

    public static class ConfigurarCRM
    {
        private static string[] VERSAO = new string[] { "3.5.15", "3.5.16", "3.5.17", "3.5.18", "3.5.19", "3.5.20", "3.5.21", "3.5.22", "3.5.23", "3.5.24", "3.5.25",
                                                        "3.6.0", "3.6.1", "3.6.2", "3.6.3", "3.6.4", "3.6.5", "3.6.7", "3.6.8", "3.6.9",
                                                        "3.7.0", "3.7.1", "3.7.2", "3.7.3", "3.7.4", "3.7.5", "3.7.6", "3.7.7", "3.7.8", "3.7.9", "3.7.10", "3.7.11", "3.7.12", "3.7.13", "3.7.14", "3.7.15", "3.7.16", "3.7.17", "3.7.18", "3.7.19","3.7.20", "3.7.21", "3.7.22", "3.7.23",
                                                        "3.8.0",
                                                        "3.9.0", "3.9.1", "3.9.2", "3.9.3", "3.9.4", "3.9.5", "3.9.6", "3.9.7", "3.9.8", "3.9.9",
                                                        "3.10.0", "3.10.1", "3.10.2", "3.10.3", "3.10.4", "3.10.5", "3.10.6", "3.10.7", "3.10.8",
                                                        "3.11.0", "3.11.1", "3.11.2",
                                                        "3.12.0", "3.12.1", "3.12.2","3.12.3",
                                                        "3.13.0", "3.13.1",
                                                        "3.14.0", "3.14.1", "3.14.2", "3.14.3", "3.14.4", "3.14.5",
                                                        "3.15.0",
                                                        "3.16.0", "3.16.1",
                                                        "4.0.0",
                                                        "4.1.0",
                                                        "4.2.0","4.2.1",
                                                        "4.3.0","4.3.1","4.3.2","4.3.3","4.3.4","4.3.5","4.3.6","4.3.7","4.3.8","4.3.9","4.3.10"};

        private static string USUARIO_SQL = "crm";
        private static string SENHA_USUARIO_SQL = "poliview";
        private static string NOME_ARQUIVO_JSON = "configcrm.json";

        public static string? Servidor { get; set; }
        public static string? Usuario { get; set; }
        public static string? Senha { get; set; }
        public static string? Mensagem { get; set; }


        public class ApiCrmConfig
        {
            public string? AllowedHosts { get; set; }
            public string? teste { get; set; }
            public string? conexao { get; set; }
            public string? conexaoFirebird { get; set; }
            public string? urlapicrm { get; set; }
            public string? urlApiSiecon { get; set; }
            public string? usuarioSiecon { get; set; }
            public string? senhaSiecon { get; set; }
            public string? Urls { get; set; }
            public Jwt? Jwt { get; set; } = new Jwt();
        }

        public class Jwt
        {
            public string? key { get; set; }
            public string? Issuer { get; set; }
            public string? Audience { get; set; }
            public string? Subject { get; set; }
        }

        public static string RetornaPathConfigCrm()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        public static ConfigCrmFile RetornaConfigCrm(string pathexe = "")
        {
            if (pathexe == "")
            {
                pathexe = RetornaPathConfigCrm();
            }

            if (!File.Exists(pathexe + $"\\{NOME_ARQUIVO_JSON}"))
            {
                var configcrm = ConfigCrmFile.LoadFromFile(pathexe + $"\\{NOME_ARQUIVO_JSON}");
            }
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            return JsonSerializer.Deserialize<ConfigCrmFile>(File.ReadAllText(pathexe + $"\\{NOME_ARQUIVO_JSON}"), options) ?? new ConfigCrmFile();
        }

        public static void SalvarConfigCrm(ConfigInstaladorCRM configcrm)
        {
            configcrm.Salvar(RetornaPathConfigCrm());
        }

        private static string RetornaConnectionString()
        {
            var configcrm = RetornaConfigCrm();
            var conexao = configcrm.ConnectionStringMSSQL;
            return conexao;
        }

        public static Boolean Conectar()
        {

            var retorna = false;
            Mensagem = "OK";
            var conexao = RetornaConnectionString();

            if (conexao != "")
            {
                try
                {
                    var connection = new SqlConnection(conexao);
                    connection.Open();
                    retorna = true;
                }
                catch (Exception e)
                {
                    Mensagem = e.Message;
                    retorna = false;
                }
            }

            return retorna;
        }

        public static Boolean UsuarioPadraoCriado()
        {

            var retorna = false;
            Mensagem = "OK";
            var conexao = RetornaConnectionString();

            if (conexao != "")
            {
                try
                {
                    var connection = new SqlConnection(conexao);
                    connection.Open();

                    var cmd = new SqlCommand(string.Format("SELECT coalesce(count(*),0) as qtde FROM sys.sysusers where name = '{0}'", USUARIO_SQL), connection);

                    var dr = cmd.ExecuteReader();
                    dr.Read();
                    retorna = Convert.ToInt16(dr["qtde"].ToString()) > 0;

                }
                catch (Exception e)
                {
                    Mensagem = e.Message;
                    retorna = false;
                }
            }

            return retorna;

        }

        public static string CriarUsuarioPadrao(string connection_string = "")
        {
            if (connection_string == "") connection_string = Sqlserver.RetornarConnectionString();

            var retorna = "OK, usuário criado!";

            var script = "";

            script += String.Format("CREATE LOGIN {0} WITH PASSWORD = '{1}' \n\r", USUARIO_SQL, SENHA_USUARIO_SQL);
            script += "GO \n\r";
            script += String.Format("CREATE USER {0} FOR LOGIN {0} \n\r", USUARIO_SQL);
            script += "GO \n\r";
            script += String.Format("EXEC sp_addrolemember 'db_datareader', '{0}' \n\r", USUARIO_SQL);
            script += "GO \n\r";
            script += String.Format("EXEC sp_addrolemember 'db_datawriter', '{0}' \n\r", USUARIO_SQL);
            script += "GO \n\r";
            script += String.Format("GRANT EXECUTE TO {0} \n\r", USUARIO_SQL); ;
            script += "GO \n\r";

            try
            {
                retorna = Sqlserver.runSqlScript(script, connection_string);
            }
            catch (Exception e)
            {

                retorna = e.Message;
            }

            return retorna;

        }

        public static Boolean PermissaoLeituraUsuarioPadraoConfigurada()
        {
            var retorna = false;
            Mensagem = "OK";
            var conexao = RetornaConnectionString();

            if (conexao != "")
            {
                try
                {
                    var connection = new SqlConnection(conexao);
                    connection.Open();
                    var sql = string.Format("SELECT coalesce(count(*),0) as qtde " +
                    "FROM sys.database_principals " +
                    "left join sys.database_role_members drm on drm.member_principal_id=sys.database_principals.principal_id " +
                    "left join sys.database_principals dp on dp.principal_id=drm.role_principal_id " +
                    "WHERE sys.database_principals.name='{0}' and dp.name='db_datareader'", USUARIO_SQL);

                    var cmd = new SqlCommand(sql, connection);

                    var dr = cmd.ExecuteReader();

                    dr.Read();
                    retorna = Convert.ToInt16(dr["qtde"].ToString()) > 0;

                }
                catch (Exception e)
                {
                    Mensagem = e.Message;
                    retorna = false;
                }
            }

            return retorna;

        }

        public static Boolean PermissaoGravacaoUsuarioPadraoConfigurada()
        {
            var retorna = false;
            Mensagem = "OK";
            var conexao = RetornaConnectionString();

            if (conexao != "")
            {

                try
                {
                    var connection = new SqlConnection(conexao);
                    connection.Open();

                    var sql = string.Format("SELECT coalesce(count(*),0) as qtde " +
                    "FROM sys.database_principals " +
                    "left join sys.database_role_members drm on drm.member_principal_id=sys.database_principals.principal_id " +
                    "left join sys.database_principals dp on dp.principal_id=drm.role_principal_id " +
                    "WHERE sys.database_principals.name='{0}' and dp.name='db_datawriter'", USUARIO_SQL);

                    var cmd = new SqlCommand(sql, connection);

                    var dr = cmd.ExecuteReader();

                    dr.Read();
                    retorna = Convert.ToInt16(dr["qtde"].ToString()) > 0;

                }
                catch (Exception e)
                {
                    Mensagem = e.Message;
                    retorna = false;
                }
            }

            return retorna;

        }
        private static DataTable ExecutarSQL(string query, string connection_string = "")
        {
            if (connection_string == "") connection_string = Sqlserver.RetornarConnectionString();
            var connection = new SqlConnection(connection_string);

            var cmd = new SqlCommand(query, connection);

            using (SqlDataAdapter da = new SqlDataAdapter { SelectCommand = cmd })
            {
                DataTable dt = new DataTable();

                try
                {
                    cmd.Connection.Open();
                    da.Fill(dt);
                    dt.Dispose();
                    return dt;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }

        }

        public static Boolean PrecisaAtualizarBaseDados(string connection_string = "")
        {
            var versao_atual = RetornaVersaoBancoDados(connection_string);
            var versao_instalador = VersaoDoInstalador();
            return (versao_atual != versao_instalador);
        }

        public static string VersaoDoInstalador()
        {
            return VERSAO[VERSAO.Length - 1];
        }

        public static string RetornaVersaoBancoDados(string connection_string = "")
        {
            DataTable dt = ExecutarSQL("select NR_VersaoSistema as versao from OPE_PARAMETRO where CD_BancoDados=1 and CD_Mandante=1", connection_string);
            DataRow dr = dt.Rows[0];
            return dr["versao"].ToString();
        }

        public static Boolean AtribuirPermissaoPastaInstalacao(string pastainstalacao)
        {
            try
            {
                var nomeUsuarioIIS1 = @"BUILTIN\IIS_IUSRS";
                var nomeUsuarioIIS2 = @"IUSR";
                var dirInfo = new DirectoryInfo(pastainstalacao);
                var dirSecurity = dirInfo.GetAccessControl();

                dirSecurity.AddAccessRule(new FileSystemAccessRule(nomeUsuarioIIS1, FileSystemRights.FullControl
                                                               , InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit
                                                               , PropagationFlags.InheritOnly, AccessControlType.Allow));

                dirSecurity.AddAccessRule(new FileSystemAccessRule(nomeUsuarioIIS2, FileSystemRights.FullControl
                                                               , InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit
                                                               , PropagationFlags.InheritOnly, AccessControlType.Allow));

                dirInfo.SetAccessControl(dirSecurity);

                return true;

            }
            catch (Exception ex)
            {
                Mensagem = "Erro ao tentar gravar a permissão do usuário IIS na pasta " + pastainstalacao + ". Erro: " + ex.Message;
                return false;
            }
        }

        public static string RetornaVersaoAtual()
        {
            return VERSAO[VERSAO.Length - 1].ToString();
        }

        public static string[] RetornaVERSAO()
        {
            return VERSAO;
        }

        public static bool IncluirServicoaSerMonitorado(string nomeservico, string descricao, string chave)
        {
            StringBuilder sSQL = new StringBuilder();

            sSQL.Append(" INSERT INTO OPE_SERVICOS_MONITORADOS (NomeServico, Ativo, Descricao, Status, chave) values (@nomeservico, 'N', @descricao, '', @chave) ");

            using (SqlCommand cmd = new SqlCommand(sSQL.ToString(), Conexao()) { CommandType = CommandType.Text })
            {
                cmd.Parameters.AddWithValue("nomeservico", nomeservico);
                cmd.Parameters.AddWithValue("descricao", descricao);
                cmd.Parameters.AddWithValue("chave", chave);
                DataTable dt = ExecutarSQLcmd(cmd);
                cmd.Dispose();
                dt.Dispose();
                return true;
            }
        }
        public static bool ExcluirServicoaSerMonitorado(string nomeservico)
        {
            StringBuilder sSQL = new StringBuilder();

            sSQL.Append(" DELETE FROM OPE_SERVICOS_MONITORADOS WHERE NomeServico=@nomeservico ");

            using (SqlCommand cmd = new SqlCommand(sSQL.ToString(), Conexao()) { CommandType = CommandType.Text })
            {
                cmd.Parameters.AddWithValue("nomeservico", nomeservico);
                DataTable dt = ExecutarSQLcmd(cmd);
                cmd.Dispose();
                dt.Dispose();
                return true;
            }
        }

        private static SqlConnection Conexao()
        {
            var conexao = RetornaConnectionString();
            SqlConnection conn = new SqlConnection(conexao);
            return conn;
        }

        private static DataTable ExecutarSQLcmd(SqlCommand cmd)
        {
            var configcrm = RetornaConfigCrm();

            using (SqlDataAdapter da = new SqlDataAdapter { SelectCommand = cmd })
            {
                DataTable dt = new DataTable();

                try
                {
                    cmd.Connection = Conexao();
                    cmd.CommandTimeout = Convert.ToInt16("120");
                    cmd.Connection.Open();
                    da.Fill(dt);
                    dt.Dispose();
                    return dt;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }

        public static Boolean AtualizarVersaoBase(string pastainstalacao, string versaoAtual, string novaVersao, ListBox log, string connection_string = "")
        {
            var erro = false;

            if (pastainstalacao == "")
            {
                Log("Pasta de instalação não definida. Não é possivel atualizar", log);
            }
            else
            {

                var versao_atual = RetornaVersaoBancoDados(connection_string);
                if (connection_string == "") connection_string = Sqlserver.RetornarConnectionString();
                int ind = Array.IndexOf(VERSAO, versao_atual);

                if (ind == VERSAO.Length - 1)
                {
                    Log("Base de dados já está atualizada para a última versão!", log);
                }
                else
                {

                    for (int i = ind + 1; i <= VERSAO.Length - 1; i++)
                    {
                        var nomeArquivoSql = pastainstalacao + @"\scripts\v" + VERSAO[i] + ".sql";

                        if (File.Exists(nomeArquivoSql))
                        {
                            Log("Atualizando base para a versão " + VERSAO[i], log);
                            string script = File.ReadAllText(nomeArquivoSql);
                            Sqlserver.runSqlScript(script, connection_string);
                        }
                        else
                        {
                            Log("arquivo " + nomeArquivoSql + " não encontrado!", log);
                            erro = true;
                        }
                    }

                    if (erro) Log("Atualização NÃO realizada. Verifique a mensagem de erro.", log);
                    else Log("Atualização Finalizada!", log);
                }
            }
            return erro;
        }

        private static void Log(string mensagem, ListBox log)
        {
            mensagem = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + mensagem;
            log.Items.Insert(0, mensagem);
        }

        private static void salvarConfigParametros(ConfigParametros config)
        {
            StringBuilder sSQL = new StringBuilder();

            sSQL.Append("update OPE_PARAMETROS SET caminhoPDF=@caminhoPDF, urlExternaHTML=@urlExternaHTML, caminhoHTML=@caminhoHTML, ");
            sSQL.Append("NM_usuarioInteg=@NM_usuarioInteg, DS_SenhaUserInteg=@DS_SenhaUserInteg, ");
            sSQL.Append("DS_PAthDBInteg=@DS_PAthDBInteg, NM_BancoDadosInteg, DS_PortaServidorInteg=@DS_PortaServidorInteg, ");
            sSQL.Append("DS_IpExterno=@DS_IpExterno, DS_PathInstallSistemaSiecon=@DS_PathInstallSistemaSiecon, ");
            sSQL.Append("usuarioAPIsiecon=@usuarioAPIsiecon, senhaApiSiecon=@senhaApiSiecon where CD_BancoDados=1 and CD_Mandante=1");

            using (SqlCommand cmd = new SqlCommand(sSQL.ToString(), Conexao()) { CommandType = CommandType.Text })
            {
                cmd.Parameters.AddWithValue("caminhoPDF", config.caminhoPDF);
                cmd.Parameters.AddWithValue("urlExternaHTML", config.urlExternaHTML);
                cmd.Parameters.AddWithValue("caminhoHTML", config.caminhoHTML);

                cmd.Parameters.AddWithValue("NM_usuarioInteg", config.usuarioSiecon);
                cmd.Parameters.AddWithValue("DS_SenhaUserInteg", config.senhaSiecon);
                cmd.Parameters.AddWithValue("DS_PAthDBInteg", config.caminhoBancoSiecon);

                cmd.Parameters.AddWithValue("NM_BancoDadosInteg", config.servidorSiecon);
                cmd.Parameters.AddWithValue("DS_PortaServidorInteg", config.portaSiecon);
                cmd.Parameters.AddWithValue("DS_IpExterno", config.urlAcessoExterno);

                cmd.Parameters.AddWithValue("DS_PathInstallSistemaSiecon", config.pathInstalacaoSiecon);
                cmd.Parameters.AddWithValue("usuarioAPIsiecon", config.usuarioSiecon);
                cmd.Parameters.AddWithValue("senhaApiSiecon", config.senhaSiecon);

                DataTable dt = ExecutarSQLcmd(cmd);
                cmd.Dispose();
                dt.Dispose();
            }
        }
    }

}
