using Poliview.crm.domain;
using Poliview.crm.infra;
using System.Data.SqlClient;
using FirebirdSql.Data.FirebirdClient;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Security.Principal;
using Microsoft.VisualBasic.ApplicationServices;
using System.Xml.Linq;

namespace poliview.crm.instalador
{
    public partial class FrmPrincipal : Form
    {
        private string conexao { get; set; }
        private ConfigInstaladorCRM configcrm { get; set; }
        private string pathexe;
        private string nomeclienteAntigo = "";
        private string pathInstalador = AppContext.BaseDirectory;

        private Boolean conexaoSQLSERVER = false;
        private Boolean conexaoFIREBIRD = false;
        private Boolean PRECISA_ATUALIZAR_BASE = true;
        // Variáveis não utilizadas removidas para melhorar a performance

        private string NOME_SERVICO_API = "PoliviewCRMApi";
        private string NOME_SERVICO_SERVICO = "PoliviewCRMServico";
        private string NOME_SERVICO_SLA = "PoliviewCRMSla";
        private string NOME_SERVICO_MONITOR = "PoliviewCRMMonitor";

        public class LoggingConfiguration
        {
            public LogLevelConfiguration? LogLevel { get; set; }
        }

        public class LogLevelConfiguration
        {
            public string? Default { get; set; }

            [JsonPropertyName("Microsoft.AspNetCore")]
            public string? MicrosoftAspNetCore { get; set; }
        }

        public class TelegramConfiguration
        {
            public string? BotToken { get; set; }
            public string? ChatId { get; set; }
        }

        public class ApiCrmJsonFile
        {
            public LoggingConfiguration? Logging { get; set; }
            public string? AllowedHosts { get; set; }
            public string? conexao { get; set; }
            public string? conexaoFirebird { get; set; }
            public string? Urls { get; set; }
            public TelegramConfiguration? Telegram { get; set; } = new TelegramConfiguration
            {
                BotToken = "7777964235:AAG8SBt-qIFjKTGkUP9C7KqiSUl0WLS7uLE",
                ChatId = "-4821323746"
            };
        }

        public class EmailJsonFile
        {
            public LoggingConfiguration? Logging { get; set; }
            public string? conexao { get; set; }
            public Boolean verQuery { get; set; } = false;
            public Boolean verDebug { get; set; } = true;
            public Boolean verErros { get; set; } = true;
            public Boolean verInfos { get; set; } = true;
        }

        public class IntegracaoJsonFile
        {
            public LoggingConfiguration? Logging { get; set; }
            public string? conexao { get; set; }
            public string? conexaoFirebird { get; set; }
            public string? conexaodb { get; set; } = "Data Source=logIntegracao.db;Cache=Shared;";
            public string? cliente { get; set; }
            public Boolean? mostrarsomenteerros { get; set; }
            public TelegramConfiguration? Telegram { get; set; } = new TelegramConfiguration
            {
                BotToken = "7777964235:AAG8SBt-qIFjKTGkUP9C7KqiSUl0WLS7uLE",
                ChatId = "-4821323746"
            };
        }

        public class MonitorJsonFile
        {
            public LoggingConfiguration? Logging { get; set; }
            public string? conexao { get; set; }
            public string? conexaoFirebird { get; set; }
            public string? conexaodb { get; set; } = "Data Source=logMonitor.db;Cache=Shared;";
            public TelegramConfiguration? Telegram { get; set; } = new TelegramConfiguration
            {
                BotToken = "7777964235:AAG8SBt-qIFjKTGkUP9C7KqiSUl0WLS7uLE",
                ChatId = "-4821323746"
            };
        }

        public class SlaJsonFile
        {
            public LoggingConfiguration? Logging { get; set; }
            public string? conexao { get; set; }
            public string? conexaoFirebird { get; set; }
            public string? conexaodb { get; set; } = "Data Source=logSla.db;Cache=Shared;";
            public TelegramConfiguration? Telegram { get; set; } = new TelegramConfiguration
            {
                BotToken = "7777964235:AAG8SBt-qIFjKTGkUP9C7KqiSUl0WLS7uLE",
                ChatId = "-4821323746"
            };
        }

        // Classes MobussApiJsonFile e MobussIntegracaoJsonFile removidas - não utilizadas no código

        public FrmPrincipal()
        {
            InitializeComponent();
            configcrm = new ConfigInstaladorCRM();
        }

        /// <summary>
        /// Verifica se o usuário atual possui privilégios de administrador
        /// </summary>
        /// <returns>True se for administrador, False caso contrário</returns>
        private bool VerificarPrivilegiosAdministrador()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception ex)
            {
                Log($"ERRO ao verificar privilégios de administrador: {ex.Message}");
                return false;
            }
        }

        private async void FrmPrincipal_Load(object sender, EventArgs e)
        {
            this.Log("Instalador Carregado!");

            // Verificar se o usuário tem privilégios de administrador
            if (!VerificarPrivilegiosAdministrador())
            {
                var mensagem = "ATENÇÃO: Este aplicativo precisa ser executado como Administrador para instalar os serviços do Windows.\n\n" +
                              "Por favor, feche o aplicativo e execute-o novamente como Administrador\n" +
                              "(clique com botão direito no executável e selecione 'Executar como administrador').";

                MessageBox.Show(mensagem, "Privilégios Insuficientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("ERRO: Aplicativo executado sem privilégios de administrador!");

                // Desabilitar botões críticos que precisam de privilégios
                btnSalvarConfiguracoes.Enabled = false;
                btnSalvarConfiguracoes.Text = "Executar como Administrador";
            }
            else
            {
                Log("Privilégios de administrador verificados - OK!");
            }

            this.Log("VERSÃO DO INSTALADOR: " + ConfigurarCRM.VersaoDoInstalador());
            this.Text = "Instalador Poliview CRM - versão " + ConfigurarCRM.VersaoDoInstalador();
            var obj = await CarregarDadosInstalador(pathInstalador + @"\configcrm.json");
            ClassToTextBox(obj);
            Log($"Pasta Executável: {pathInstalador}");

            if (!string.IsNullOrEmpty(obj.ConnectionStringMSSQL))
            {
                txtConnectionString.Text = obj.ConnectionStringMSSQL;
                conectarBancoSql(txtConnectionString.Text);
                ConectarBancoFirebird(txtConnectionStringFB.Text);
                PreencherDadosAPartirDoBancoDeDados(txtConnectionString.Text);
            }
            button1.Visible = PRECISA_ATUALIZAR_BASE;
            this.nomeclienteAntigo = obj.NomeCliente;
        }

        private void ClassToTextBox(ConfigCrmFile obj)
        {
            txtNomeCliente.Text = obj.NomeCliente;
            txtPortaAPI.Text = obj.PortaApiCrm.ToString();
            txtConnectionString.Text = obj.ConnectionStringMSSQL;
            txtConnectionStringFB.Text = obj.ConnectionStringFB;
            txtAcessoExterno.Text = obj.AcessoExterno;
            txtPastaInstalacaoCRM.Text = obj.PastaInstalacaoCRM;

            txtServidorFirebird.Text = obj.ServidorFirebird;
            txtBancoFirebird.Text = obj.BancoFirebird;
            txtUsuarioFirebird.Text = obj.UsuarioFirebird;
            txtSenhaFirebird.Text = obj.SenhaFirebird;
            txtPortaFirebird.Text = obj.PortaFirebird.ToString();

            txtServidorSqlServer.Text = obj.ServidorSqlServer;
            txtBancoSqlServer.Text = obj.BancoSqlServer;
            txtUsuarioSqlServer.Text = obj.UsuarioSqlServer;
            txtSenhaSqlServer.Text = obj.SenhaSqlServer;
        }

        private ConfigCrmFile TextBoxToClass()
        {
            var obj = new ConfigCrmFile
            {
                NomeCliente = txtNomeCliente.Text,
                AcessoExterno = txtAcessoExterno.Text,
                PortaApiCrm = Convert.ToInt32(txtPortaAPI.Text),
                PastaInstalacaoCRM = txtPastaInstalacaoCRM.Text,
                ConnectionStringMSSQL = txtConnectionString.Text,
                ConnectionStringFB = txtConnectionStringFB.Text,
                ServidorFirebird = txtServidorFirebird.Text,
                BancoFirebird = txtBancoFirebird.Text,
                UsuarioFirebird = txtUsuarioFirebird.Text,
                SenhaFirebird = txtSenhaFirebird.Text,
                PortaFirebird = Convert.ToInt32(txtPortaFirebird.Text),
                ServidorSqlServer = txtServidorSqlServer.Text,
                BancoSqlServer = txtBancoSqlServer.Text,
                UsuarioSqlServer = txtUsuarioSqlServer.Text,
                SenhaSqlServer = txtSenhaSqlServer.Text
            };
            return obj;
        }

        private async void btnSalvarConfiguracoes_Click(object sender, EventArgs e)
        {
            // Validações de entrada
            if (!ValidarEntradas())
                return;


            if (!conexaoFIREBIRD)
            {
                ConectarBancoFirebird(txtConnectionStringFB.Text);
            }

            if (!conexaoSQLSERVER)
            {
                conectarBancoSql(txtConnectionString.Text);
                PreencherDadosAPartirDoBancoDeDados(txtConnectionString.Text);
                button1.Visible = PRECISA_ATUALIZAR_BASE;
            }

            if (!conexaoFIREBIRD)
            {
                MessageBox.Show("NÃO É POSSIVEL CONECTAR NO BANCO DE DADOS FIREBIRD");
                return;
            }

            if (!conexaoSQLSERVER)
            {
                MessageBox.Show("NÃO É POSSIVEL CONECTAR NO BANCO DE DADOS SQL SERVER");
                return;
            }

            if (PRECISA_ATUALIZAR_BASE)
            {
                MessageBox.Show("A base de dados precisa ser ATUALIZADA");
                return;
            }

            try
            {
                var obj = TextBoxToClass();

                await this.SalvarConfiguracaoPastaInstaladorAsync(obj, txtPastaInstalacaoCRM.Text, pathInstalador);
                this.DesinstalarTodosServicosCrm(nomeclienteAntigo, txtNomeCliente.Text);
                this.InstalarServicosSelecionados(obj);
                var cfgParametros = new ConfigParametros();
                cfgParametros.caminhoHTML = "";
                cfgParametros.caminhoPDF = txtPastaInstalacaoCRM.Text + "\\Website\\PDF";
                cfgParametros.caminhoHTML = txtPastaInstalacaoCRM.Text + "\\Website\\HTML";
                this.SalvarConfiguracaoParametros(txtConnectionString.Text);
                this.SalvarConfiguracaoServicos(txtConnectionString.Text, txtConnectionStringFB.Text);
                this.SalvarProxyReverso(txtPastaInstalacaoCRM.Text + @"\website\apicrm\web.config", txtPortaAPI.Text);
                this.GerarArquivosAppSettings(obj);
                this.Log("Configurações salvas com sucesso!");
            }
            catch (Exception ex)
            {
                Log("ERRO: " + ex.Message);
            }

        }

        private void Log(string mensagem)
        {
            mensagem = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + mensagem;
            txtLog.Items.Insert(0, mensagem);
        }

        private bool ValidarEntradas()
        {
            // Validação do nome do cliente
            if (string.IsNullOrWhiteSpace(txtNomeCliente.Text))
            {
                MessageBox.Show("O NOME DO CLIENTE NÃO ESTÁ PREENCHIDO", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNomeCliente.Focus();
                return false;
            }

            // Validação da pasta de instalação
            if (string.IsNullOrWhiteSpace(txtPastaInstalacaoCRM.Text))
            {
                MessageBox.Show("PREENCHA A PASTA DE INSTALAÇÃO DO CRM!", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPastaInstalacaoCRM.Focus();
                return false;
            }

            if (!Directory.Exists(txtPastaInstalacaoCRM.Text))
            {
                MessageBox.Show("A pasta de instalação do CRM não existe", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPastaInstalacaoCRM.Focus();
                return false;
            }

            // Validação da porta da API
            if (!int.TryParse(txtPortaAPI.Text, out int porta) || porta <= 0 || porta > 65535)
            {
                MessageBox.Show("Porta deve ser um número válido entre 1 e 65535", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPortaAPI.Focus();
                return false;
            }

            // Validação das connection strings
            if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
            {
                MessageBox.Show("A connection string do SQL Server é obrigatória", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtConnectionString.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtConnectionStringFB.Text))
            {
                MessageBox.Show("A connection string do Firebird é obrigatória", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtConnectionStringFB.Focus();
                return false;
            }

            return true;
        }

        private async Task SalvarConfiguracaoPastaInstaladorAsync(ConfigCrmFile obj, string pathInstalacao, string pathInstalador)
        {
            var arq = pathInstalacao + @"\Instalador\configcrm.json";
            var arqInstalador = pathInstalador + @"\configcrm.json";
            var arquivojson = new ConfigCrmFile();
            arquivojson.NomeCliente = obj.NomeCliente;
            arquivojson.AcessoExterno = obj.AcessoExterno;
            arquivojson.PortaApiCrm = obj.PortaApiCrm;
            arquivojson.PastaInstalacaoCRM = obj.PastaInstalacaoCRM;
            arquivojson.ConnectionStringMSSQL = obj.ConnectionStringMSSQL;
            arquivojson.ConnectionStringFB = obj.ConnectionStringFB;
            arquivojson.ServidorFirebird = obj.ServidorFirebird;
            arquivojson.BancoFirebird = obj.BancoFirebird;
            arquivojson.UsuarioFirebird = obj.UsuarioFirebird;
            arquivojson.SenhaFirebird = obj.SenhaFirebird;
            arquivojson.PortaFirebird = obj.PortaFirebird;
            arquivojson.ServidorSqlServer = obj.ServidorSqlServer;
            arquivojson.BancoSqlServer = obj.BancoSqlServer;
            arquivojson.UsuarioSqlServer = obj.UsuarioSqlServer;
            arquivojson.SenhaSqlServer = obj.SenhaSqlServer;

            if (!Directory.Exists(pathInstalacao + @"\Instalador"))
            {
                Directory.CreateDirectory(pathInstalacao + @"\Instalador");
            }
            await SalvarParaArquivoAsync<ConfigCrmFile>(arquivojson, arq);
            if (pathInstalador != pathInstalacao) await SalvarParaArquivoAsync<ConfigCrmFile>(arquivojson, arqInstalador);
            Log($"Arquivo {arq} gerado com sucesso!");
        }

        public async Task SalvarParaArquivoAsync<T>(T obj, string caminhoArquivo)
        {
            try
            {
                var opcoes = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                using var stream = File.Create(caminhoArquivo);
                await System.Text.Json.JsonSerializer.SerializeAsync(stream, obj, opcoes);

            }
            catch (Exception ex)
            {
                var erro = ex.ToString();
                throw;
            }
        }

        private Boolean PreencherDadosAPartirDoBancoDeDados(string connectionString)
        {
            txtAcessoExterno.Text = "";
            txtPastaInstalacaoCRM.Text = "";
            txtPortaAPI.Text = "";

            LabelVersaoBanco(connectionString);

            if (!ExisteCamposInstalacaoParametros(connectionString)) return false;

            try
            {
                using var connection = new SqlConnection(connectionString);
                var query = $@"SELECT top 1
                            DS_IpExterno as AcessoExterno,
                            PortaApiCrm,
                            PastaInstalacaoCRM
                            FROM OPE_PARAMETRO
                            where CD_BancoDados=1 and CD_Mandante=1";

                var obj = connection.QueryFirst<ConfigCrmFile>(query);

                if (obj != null)
                {
                    txtAcessoExterno.Text = obj.AcessoExterno;
                    txtPastaInstalacaoCRM.Text = obj.PastaInstalacaoCRM;
                    txtPortaAPI.Text = obj.PortaApiCrm.ToString();
                    obj.ConnectionStringFB = txtConnectionStringFB.Text;
                    obj.ConnectionStringMSSQL = txtConnectionString.Text;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log($"ERRO: ERRO AO CARREGAR PARÂMETROS: {ex.Message}");
                return false;
            }
        }



        private Boolean ExisteCamposInstalacaoParametros(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                var query = $@"SELECT 1
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_SCHEMA = 'dbo'
                              AND TABLE_NAME = 'OPE_PARAMETRO'
                              AND COLUMN_NAME = 'PastaInstalacaoCRM'";

                var obj = connection.Query<ConfigCrmFile>(query);
                return obj.Count() > 0;
            }
            catch (Exception ex)
            {
                Log($"ERRO: ERRO AO CARREGAR PARÂMETROS: {ex.Message}");
                return false;
            }
        }

        private async void SalvarDadosInstalador(ConfigCrmFile obj, string CaminhoArquivoJson)
        {
            await SalvarParaArquivoAsync<ConfigCrmFile>(obj, CaminhoArquivoJson);
        }

        private async Task<ConfigCrmFile> CarregarDadosInstalador(string CaminhoArquivoJson)
        {
            var obj = new ConfigCrmFile
            {
                NomeCliente = "",
                AcessoExterno = "",
                PortaApiCrm = 5000,
                PastaInstalacaoCRM = ""
            };

            if (!File.Exists(CaminhoArquivoJson))
            {
                Log($"Arquivo {CaminhoArquivoJson} não encontrado.");
                this.SalvarDadosInstalador(obj, CaminhoArquivoJson);
            }
            else
            {
                obj = await CarregarDoArquivoAsync<ConfigCrmFile>(CaminhoArquivoJson);
            }

            return obj;
        }

        // M�todo auxiliar para carregar e desserializar o arquivo JSON
        private async Task<T> CarregarDoArquivoAsync<T>(string caminhoArquivo) where T : class
        {
            try
            {
                using (var streamReader = new StreamReader(caminhoArquivo))
                {
                    var jsonContent = await streamReader.ReadToEndAsync();
                    var options = new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<T>(jsonContent, options);
                }
            }
            catch (Exception ex)
            {
                Log($"Erro ao ler arquivo {caminhoArquivo}: {ex.Message}");
                throw;
            }
        }

        private void SalvarProxyReverso(string caminhoArquivo, string portaApi)
        {
            var xmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                              "<configuration>\n" +
                              "  <system.webServer>\n" +
                              "    <rewrite>\n" +
                              "      <rules>\n" +
                              "        <rule name=\"ReverseProxy\" stopProcessing=\"true\">\n" +
                              "          <match url=\"(.*)\" />\n" +
                              "          <action type=\"Rewrite\" url=\"http://localhost:" + portaApi + "/{R:1}\" />\n" +
                              "        </rule>\n" +
                              "      </rules>\n" +
                              "    </rewrite>\n" +
                              "  </system.webServer>\n" +
                              "</configuration>";
            File.WriteAllText(caminhoArquivo, xmlContent);
            Log($"Arquivo de configuração do proxy reverso salvo em {caminhoArquivo}");
        }

        private void SalvarConfiguracaoServicos(string connectionString, string connectionStringFB = "")
        {

            var opcoes = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            // configuração API
            var obj = new ApiCrmJsonFile();
            obj.conexao = connectionString;
            obj.conexaoFirebird = connectionStringFB;
            obj.AllowedHosts = "*";
            obj.Urls = $"http://localhost:{txtPortaAPI.Text}";
            obj.Logging = new LoggingConfiguration
            {
                LogLevel = new LogLevelConfiguration
                {
                    Default = "Information",
                    MicrosoftAspNetCore = "Warning"
                }
            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllTextAsync(txtPastaInstalacaoCRM.Text + @"\servicos\apicrm\appsettings.json", jsonString);
            Log("configuração API CRM");

            // Configuracao EMAIL
            var objEmail = new EmailJsonFile();
            objEmail.Logging = new LoggingConfiguration
            {
                LogLevel = new LogLevelConfiguration
                {
                    Default = "Information",
                    MicrosoftAspNetCore = "Warning"
                }
            };
            objEmail.conexao = connectionString;
            string jsonStringEmail = System.Text.Json.JsonSerializer.Serialize(objEmail, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllTextAsync(txtPastaInstalacaoCRM.Text + @"\servicos\email\appsettings.json", jsonStringEmail);
            Log("configuração EMAIL");

            // Configuracao INTEGRACAO
            var objIntegracao = new IntegracaoJsonFile();
            objIntegracao.Logging = new LoggingConfiguration
            {
                LogLevel = new LogLevelConfiguration
                {
                    Default = "Information",
                    MicrosoftAspNetCore = "Warning"
                }
            };
            objIntegracao.conexao = connectionString;
            objIntegracao.conexaoFirebird = connectionStringFB;
            objIntegracao.conexaodb = $"Data Source=logIntegracao{txtNomeCliente.Text}.db;Cache=Shared;";
            objIntegracao.cliente = txtNomeCliente.Text;
            objIntegracao.mostrarsomenteerros = true;
            string jsonStringIntegracao = System.Text.Json.JsonSerializer.Serialize(objIntegracao, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllTextAsync(txtPastaInstalacaoCRM.Text + @"\servicos\integracao\appsettings.json", jsonStringIntegracao);
            Log("configuração INTEGRAÇÃO");

            // Configuracao MONITOR
            var objMonitor = new MonitorJsonFile();
            objMonitor.Logging = new LoggingConfiguration
            {
                LogLevel = new LogLevelConfiguration
                {
                    Default = "Information",
                    MicrosoftAspNetCore = "Warning"
                }
            };
            objMonitor.conexao = connectionString;
            objMonitor.conexaoFirebird = connectionStringFB;
            objMonitor.conexaodb = $"Data Source=logMonitor{txtNomeCliente.Text}.db;Cache=Shared;";
            string jsonStringMonitor = System.Text.Json.JsonSerializer.Serialize(objMonitor, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllTextAsync(txtPastaInstalacaoCRM.Text + @"\servicos\monitor\appsettings.json", jsonStringMonitor);
            Log("configuração MONITORAMENTO");

            // Configuracao SLA
            var objSla = new SlaJsonFile();
            objSla.Logging = new LoggingConfiguration
            {
                LogLevel = new LogLevelConfiguration
                {
                    Default = "Information",
                    MicrosoftAspNetCore = "Warning"
                }
            };
            objSla.conexao = connectionString;
            objSla.conexaoFirebird = connectionStringFB;
            objSla.conexaodb = $"Data Source=logSla{txtNomeCliente.Text}.db;Cache=Shared;";
            string jsonStringSla = System.Text.Json.JsonSerializer.Serialize(objSla, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllTextAsync(txtPastaInstalacaoCRM.Text + @"\servicos\sla\appsettings.json", jsonStringSla);
            Log("configuração SLA");

        }

        private void DesinstalarTodosServicosCrm(string nomeclienteAntigo, string nomeclienteAtual)
        {
            ServicosUtil.ExcluirServicos(nomeclienteAntigo);
            ServicosUtil.ExcluirServicos(nomeclienteAtual);
        }

        private void InstalarServicosSelecionados(ConfigCrmFile obj)
        {
            var versaoInstalador = ConfigurarCRM.VersaoDoInstalador();
            var retorno = "";
            var SERVICO = "";

            SERVICO = NOME_SERVICO_API + "_" + obj.NomeCliente;
            retorno = ServicosUtil.InstalarServico(SERVICO,
                                            obj.PastaInstalacaoCRM + "\\Servicos\\apicrm\\poliview.crm.api.exe",
                                            $"Poliview CRM - {obj.NomeCliente} - API - {versaoInstalador}",
                                            "Serviço de API do CRM",
                                            "API");
            if (!string.IsNullOrEmpty(retorno))
                Log(SERVICO + ": " + retorno);
            else
                Log(SERVICO + ": instalado");

            SERVICO = NOME_SERVICO_SERVICO + "_" + obj.NomeCliente;
            retorno = ServicosUtil.InstalarServico(SERVICO,
                                            obj.PastaInstalacaoCRM + "\\Servicos\\servicos\\Poliview.crm.servicos.exe",
                                            $"Poliview CRM - {obj.NomeCliente} - SERVIÇO - {versaoInstalador}",
                                            "Poliview CRM Serviço",
                                            "SERVICO");
            if (!string.IsNullOrEmpty(retorno))
                Log(SERVICO + ": " + retorno);
            else
                Log(SERVICO + ": instalado");

            SERVICO = NOME_SERVICO_SLA + "_" + obj.NomeCliente;
            retorno = ServicosUtil.InstalarServico(SERVICO,
                                            obj.PastaInstalacaoCRM + "\\Servicos\\sla\\poliview.crm.sla.exe",
                                            $"Poliview CRM - {obj.NomeCliente} - SLA - {versaoInstalador}",
                                            "Poliview CRM Controle de SLA",
                                            "SLA");
            if (!string.IsNullOrEmpty(retorno))
                Log(SERVICO + ": " + retorno);
            else
                Log(SERVICO + ": instalado");

            SERVICO = NOME_SERVICO_MONITOR + "_" + obj.NomeCliente;
            retorno = ServicosUtil.InstalarServico(SERVICO,
                                            obj.PastaInstalacaoCRM + "\\Servicos\\monitor\\Poliview.crm.monitor.service.exe",
                                            $"Poliview CRM - {obj.NomeCliente} - MONITOR - {versaoInstalador}",
                                            "Poliview CRM Monitoramento Serviços",
                                            "MONITOR");
            if (!string.IsNullOrEmpty(retorno))
                Log(SERVICO + ": " + retorno);
            else
                Log(SERVICO + ": instalado");
        }

        private void btnConectarSqlServer_Click(object sender, EventArgs e)
        {
            //conectarBancoSql();
            //PreencherDadosAPartirDoBancoDeDados();
        }

        private void conectarBancoSql(string connectionString)
        {

            var connection = new SqlConnection(connectionString);

            this.Cursor = Cursors.WaitCursor;

            try
            {
                connection.Open();
                TextBoxToClass();
                Log("conexão SQL SERVER - ok!");
                PRECISA_ATUALIZAR_BASE = ConfigurarCRM.PrecisaAtualizarBaseDados(txtConnectionString.Text);
                conexaoSQLSERVER = true;
                LblConexao(true, lblconexaoSQL);
            }
            catch (Exception ex)
            {
                Log($"Erro conexão SQL SERVER - {ex.Message}");
                conexaoSQLSERVER = false;
                LblConexao(false, lblconexaoSQL);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ConectarBancoFirebird(string connectionStringFB)
        {
            var connection = new FbConnection(connectionStringFB);
            this.Cursor = Cursors.WaitCursor;

            try
            {
                connection.Open();
                TextBoxToClass();
                Log("conexão FIREBIRD - ok!");
                conexaoFIREBIRD = true;
                LblConexao(true, lblConexaoFB);
            }
            catch (Exception ex)
            {
                Log($"Erro conexão FIREBIRD - {ex.Message}");
                conexaoFIREBIRD = false;
                LblConexao(false, lblConexaoFB);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var connectionStringSql = txtConnectionString.Text;
                var versaoBanco = ConfigurarCRM.RetornaVersaoBancoDados(connectionStringSql);
                var versaoInstalador = ConfigurarCRM.VersaoDoInstalador();
                var erro = ConfigurarCRM.AtualizarVersaoBase(txtPastaInstalacaoCRM.Text, versaoBanco, versaoInstalador, txtLog, connectionStringSql);

                if (!erro)
                {
                    Log($"Base atualizada da versão {versaoBanco} para a versão {versaoInstalador} executada com sucesso!");
                    LabelVersaoBanco(connectionStringSql);
                }
                else
                {
                    Log($"ERRO AO ATUALIZAR A BASE");
                }
            }
            catch (Exception ex)
            {
                Log($"ERRO: ERRO AO ATUALIZAR A BASE: {ex.Message}");
            }
        }

        private void SalvarConfiguracaoParametros(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                var query = @"UPDATE OPE_PARAMETRO SET
                                        DS_IpExterno = @ipExterno,
                                        PortaApiCrm = @porta,
                                        PastaInstalacaoCRM = @pasta,
                                        caminhoPDF = @caminhoPdf";

                connection.Execute(query, new
                {
                    ipExterno = txtAcessoExterno.Text,
                    porta = Convert.ToInt32(txtPortaAPI.Text),
                    pasta = txtPastaInstalacaoCRM.Text,
                    caminhoPdf = txtPastaInstalacaoCRM.Text + "/website/pdf"
                });
            }
            catch (Exception ex)
            {
                Log($"ERRO: ERRO AO SALVAR PARÂMETROS: {ex.Message}");
            }
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void LblConexao(Boolean conectado, Label lbl)
        {
            if (conectado)
            {
                lbl.Text = "Conectado";
                lbl.BackColor = Color.Green;
                lbl.ForeColor = Color.White;
            }
            else
            {
                lbl.Text = "Desconectado";
                lbl.ForeColor = Color.White;
                lbl.BackColor = Color.Red;
            }
        }

        private void LabelVersaoBanco(string connectionString, string erro = "")
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                var versao = ConfigurarCRM.RetornaVersaoBancoDados(connectionString);
                lblVersaoSQL.Text = versao;
                var precisaAtualizar = ConfigurarCRM.PrecisaAtualizarBaseDados(connectionString);
                PRECISA_ATUALIZAR_BASE = precisaAtualizar;

                if (erro != "")
                {
                    lblVersaoSQL.Text = erro;
                    lblVersaoSQL.ForeColor = Color.White;
                    lblVersaoSQL.BackColor = Color.Red;
                    return;
                }

                if (precisaAtualizar)
                {
                    lblVersaoSQL.ForeColor = Color.White;
                    lblVersaoSQL.BackColor = Color.Red;
                    lblVersaoSQL.Text += " - ATUALIZAR";
                }
                else
                {
                    lblVersaoSQL.ForeColor = Color.White;
                    lblVersaoSQL.BackColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblVersaoSQL.Text = ex.Message;
                lblVersaoSQL.ForeColor = Color.White;
                lblVersaoSQL.BackColor = Color.Red;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtConnectionString.Text = MontarConnectionStringSqlServer();
            txtConnectionStringFB.Text = MontarConnectionStringFirebird();

            conectarBancoSql(txtConnectionString.Text);
            PreencherDadosAPartirDoBancoDeDados(txtConnectionString.Text);
            button1.Visible = PRECISA_ATUALIZAR_BASE;
        }

        private string MontarConnectionStringSqlServer()
        {
            // Server = G15\SQLEXPRESS; Database = ELEMENTO; User Id = sa; Password = master; Connect Timeout = 600; TrustServerCertificate = True;
            return $"Server={txtServidorSqlServer.Text};Database={txtBancoSqlServer.Text};User Id={txtUsuarioSqlServer.Text};Password={txtSenhaSqlServer.Text};Connect Timeout=600;TrustServerCertificate=True";
        }
        private string MontarConnectionStringFirebird()
        {
            // User=SYSDBA;Password=masterkey;Database=C:\Poliview\BancoDeDados\ELEMENTO.PLV;DataSource=localhost;Port=3050;Dialect=3;PacketSize=4096;Pooling=false;
            return $"User={txtUsuarioFirebird.Text};Password={txtSenhaFirebird.Text};Database={txtBancoFirebird.Text};DataSource={txtServidorFirebird.Text};Port={txtPortaFirebird.Text};Dialect=3;Charset=UTF8;Pooling=false;";
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            ConectarBancoFirebird(txtConnectionStringFB.Text);
        }

        private void GerarArquivosAppSettings(ConfigCrmFile obj)
        {
            if (!AppSettingsXmlGenerator.ValidarParametros(obj.ConnectionStringMSSQL, obj.NomeCliente))
            {
                Log("ERRO: Parâmetros inválidos para gerar arquivos appSettings.xml");
                return;
            }
            try
            {
                AppSettingsXmlGenerator.GerarAppSettingsServico(obj);
                Log($"Arquivos appSettings.xml do serviço gerado com sucesso!");
            }
            catch (Exception ex)
            {
                Log($"ERRO ao gerar arquivo Serviço appSettings.xml: {ex.Message}");
            }

            try
            {
                AppSettingsXmlGenerator.GerarAppSettingsWebSite(obj);
                Log($"Arquivos appSettings.xml do WebSite gerado com sucesso!");
            }
            catch (Exception ex)
            {
                Log($"ERRO ao gerar arquivo WebSite appSettings.xml: {ex.Message}");
            }
        }

        private void txtServidorSqlServer_Leave(object sender, EventArgs e)
        {
            txtConnectionString.Text = MontarConnectionStringSqlServer();
        }

        private void txtServidorFirebird_Leave(object sender, EventArgs e)
        {
            txtConnectionStringFB.Text = MontarConnectionStringFirebird();
        }

        private void txtLog_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}