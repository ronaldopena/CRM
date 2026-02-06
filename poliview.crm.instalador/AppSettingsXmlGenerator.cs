using System.Xml;

namespace poliview.crm.instalador
{
    public class AppSettingsXmlGenerator
    {
        public static void GerarAppSettingsServico(ConfigCrmFile obj)
        {
            try
            {
                // Criar o documento XML
                var xmlDoc = new XmlDocument();

                // Adicionar declaração XML
                var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                // Criar elemento raiz appSettings
                var appSettingsElement = xmlDoc.CreateElement("appSettings");
                xmlDoc.AppendChild(appSettingsElement);

                // Criar elemento add para conexão
                var conexaoElement = xmlDoc.CreateElement("add");
                conexaoElement.SetAttribute("key", "conexao");
                conexaoElement.SetAttribute("value", obj.ConnectionStringMSSQL);
                appSettingsElement.AppendChild(conexaoElement);

                // Criar elemento add para cliente
                var clienteElement = xmlDoc.CreateElement("add");
                clienteElement.SetAttribute("key", "cliente");
                clienteElement.SetAttribute("value", obj.NomeCliente);
                appSettingsElement.AppendChild(clienteElement);

                // Criar diretório se não existir
                var arquivo = obj.PastaInstalacaoCRM + @"\servicos\servicos\AppSettings.config";
    
                // Configurar formatação para XML bem formatado
                var xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineOnAttributes = false,
                    Encoding = System.Text.Encoding.UTF8
                };

                // Salvar o arquivo XML
                using var xmlWriter = XmlWriter.Create(arquivo, xmlWriterSettings);
                xmlDoc.Save(xmlWriter);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar arquivo appSettings.xml do Serviço: {ex.Message}", ex);
            }
        }

        public static void GerarAppSettingsWebSite(ConfigCrmFile obj)
        {
            // Cria o documento XML
            XmlDocument xmlDoc = new XmlDocument();

            // Adiciona a declaração XML
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", null, null);
            xmlDoc.AppendChild(xmlDeclaration);

            // Cria o elemento raiz
            XmlElement appSettings = xmlDoc.CreateElement("appSettings");
            xmlDoc.AppendChild(appSettings);

            // Adiciona os elementos de configuração
            AdicionarConfiguracao(xmlDoc, appSettings, "NomeSistema", "SIECONSP7");
            AdicionarConfiguracao(xmlDoc, appSettings, "NomeEmpresa", "Poliview");
            AdicionarConfiguracao(xmlDoc, appSettings, "EmpresaCopyrightAnos", "Poliview Tecnologia S.A | 1986 - ");
            AdicionarConfiguracao(xmlDoc, appSettings, "EmpresaSiteRodape", "www.poliview.com.br");
            AdicionarConfiguracao(xmlDoc, appSettings, "IntervaloProcessamentoSMTP", "5");
            AdicionarConfiguracao(xmlDoc, appSettings, "IntervaloProcessamentoEmailsSLA", "1");
            AdicionarConfiguracao(xmlDoc, appSettings, "QtdEmailsPorIntervaloProcessamentoSMTP", "5");
            AdicionarConfiguracao(xmlDoc, appSettings, "NumMaximoTentativasEnvioEmails", "5");
            AdicionarConfiguracao(xmlDoc, appSettings, "PastaInstalacao", @$"{obj.PastaInstalacaoCRM}");

            // Criar diretório se não existir
            var caminhoCompleto = obj.PastaInstalacaoCRM + @"\WebSite\AppSettings.config";

            // Salva o arquivo
            xmlDoc.Save(caminhoCompleto);
        }

        private static void AdicionarConfiguracao(XmlDocument xmlDoc, XmlElement parentElement, string key, string value)
        {
            XmlElement addElement = xmlDoc.CreateElement("add");
            addElement.SetAttribute("key", key);
            addElement.SetAttribute("value", value);
            parentElement.AppendChild(addElement);
        }

        private static void GerarAppSettingsWebSiteAntigo(ConfigCrmFile obj)
        {
            try
            {
                // Criar o documento XML
                var xmlDoc = new XmlDocument();

                // Adicionar declaração XML
                var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                // Criar elemento raiz appSettings
                var appSettingsElement = xmlDoc.CreateElement("appSettings");
                xmlDoc.AppendChild(appSettingsElement);

                var conexaoElement = xmlDoc.CreateElement("add");
                conexaoElement.SetAttribute("key", "NomeSistema");
                conexaoElement.SetAttribute("value", "SIECONSP7");
                appSettingsElement.AppendChild(conexaoElement);

                var clienteElement = xmlDoc.CreateElement("add");
                clienteElement.SetAttribute("key", "NomeEmpresa");
                clienteElement.SetAttribute("value", "Poliview");
                appSettingsElement.AppendChild(clienteElement);

                var EmpresaCopyrightAnosElement = xmlDoc.CreateElement("add");
                EmpresaCopyrightAnosElement.SetAttribute("key", "EmpresaCopyrightAnos");
                EmpresaCopyrightAnosElement.SetAttribute("value", "Poliview Tecnologia S.A | 1986 - ");
                appSettingsElement.AppendChild(EmpresaCopyrightAnosElement);

                var EmpresaSiteRodapeElement = xmlDoc.CreateElement("add");
                EmpresaSiteRodapeElement.SetAttribute("key", "EmpresaSiteRodape");
                EmpresaSiteRodapeElement.SetAttribute("value", "www.poliview.com.br");
                appSettingsElement.AppendChild(EmpresaSiteRodapeElement);

                var IntervaloProcessamentoSMTPElement = xmlDoc.CreateElement("add");
                IntervaloProcessamentoSMTPElement.SetAttribute("key", "IntervaloProcessamentoSMTP");
                IntervaloProcessamentoSMTPElement.SetAttribute("value", "5");
                IntervaloProcessamentoSMTPElement.AppendChild(IntervaloProcessamentoSMTPElement);

                var IntervaloProcessamentoEmailsSLA = xmlDoc.CreateElement("add");
                IntervaloProcessamentoEmailsSLA.SetAttribute("key", "IntervaloProcessamentoEmailsSLA");
                IntervaloProcessamentoEmailsSLA.SetAttribute("value", "1");
                IntervaloProcessamentoEmailsSLA.AppendChild(IntervaloProcessamentoEmailsSLA);

                var NumMaximoTentativasEnvioEmails = xmlDoc.CreateElement("add");
                NumMaximoTentativasEnvioEmails.SetAttribute("key", "NumMaximoTentativasEnvioEmails");
                NumMaximoTentativasEnvioEmails.SetAttribute("value", "5");
                NumMaximoTentativasEnvioEmails.AppendChild(NumMaximoTentativasEnvioEmails);

                var PastaInstalacaoElement = xmlDoc.CreateElement("add");
                PastaInstalacaoElement.SetAttribute("key", "PastaInstalacao");
                PastaInstalacaoElement.SetAttribute("value", obj.PastaInstalacaoCRM);
                appSettingsElement.AppendChild(PastaInstalacaoElement);

                // Criar diretório se não existir
                var arquivo = obj.PastaInstalacaoCRM + @"\WebSite\AppSettings.config";

                // Configurar formatação para XML bem formatado
                var xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineOnAttributes = false,
                    Encoding = System.Text.Encoding.UTF8
                };

                // Salvar o arquivo XML
                using var xmlWriter = XmlWriter.Create(arquivo, xmlWriterSettings);
                xmlDoc.Save(xmlWriter);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar arquivo appSettings.xml do Serviço: {ex.Message}", ex);
            }
        }
        public static bool ValidarParametros(string connectionString, string nomeCliente)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            if (string.IsNullOrWhiteSpace(nomeCliente))
                return false;

            return true;
        }
    }
}