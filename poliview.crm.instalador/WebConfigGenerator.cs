using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poliview.crm.instalador
{
    using System.Xml;
    using System.Xml.Serialization;

    // Classe principal para representar o web.config
    [XmlRoot("configuration")]
    public class WebConfiguration
    {
        [XmlElement("system.webServer")]
        public SystemWebServer SystemWebServer { get; set; } = new();
    }

    public class SystemWebServer
    {
        [XmlElement("rewrite")]
        public Rewrite Rewrite { get; set; } = new();
    }

    public class Rewrite
    {
        [XmlElement("rules")]
        public Rules Rules { get; set; } = new();
    }

    public class Rules
    {
        [XmlElement("rule")]
        public Rule[] Rule { get; set; } = Array.Empty<Rule>();
    }

    public class Rule
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute("stopProcessing")]
        public string StopProcessing { get; set; } = "true";

        [XmlElement("match")]
        public Match Match { get; set; } = new();

        [XmlElement("action")]
        public Action Action { get; set; } = new();
    }

    public class Match
    {
        [XmlAttribute("url")]
        public string Url { get; set; } = string.Empty;
    }

    public class Action
    {
        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;

        [XmlAttribute("url")]
        public string Url { get; set; } = string.Empty;
    }

    // Classe utilitária para gerar o XML
    public static class WebConfigGenerator
    {
        public static WebConfiguration CriarConfiguracaoReverseProxy(
            string actionUrl = "http://localhost:6001/{R:1}",
            string ruleName = "ReverseProxy",
            string matchUrl = "(.*)",
            bool stopProcessing = true)
        {
            return new WebConfiguration
            {
                SystemWebServer = new SystemWebServer
                {
                    Rewrite = new Rewrite
                    {
                        Rules = new Rules
                        {
                            Rule = new[]
                            {
                            new Rule
                            {
                                Name = ruleName,
                                StopProcessing = stopProcessing.ToString().ToLower(),
                                Match = new Match
                                {
                                    Url = matchUrl
                                },
                                Action = new Action
                                {
                                    Type = "Rewrite",
                                    Url = actionUrl
                                }
                            }
                        }
                        }
                    }
                }
            };
        }

        public static string GerarXml(WebConfiguration config = null)
        {
            config ??= CriarConfiguracaoReverseProxy();

            var xmlSerializer = new XmlSerializer(typeof(WebConfiguration));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            xmlSerializer.Serialize(xmlWriter, config);
            return stringWriter.ToString();
        }

        public static void SalvarArquivo(string caminho = "web.config", WebConfiguration config = null)
        {
            config ??= CriarConfiguracaoReverseProxy();

            var xmlSerializer = new XmlSerializer(typeof(WebConfiguration));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var fileStream = new FileStream(caminho, FileMode.Create);
            using var xmlWriter = XmlWriter.Create(fileStream, settings);

            xmlSerializer.Serialize(xmlWriter, config);
        }

        public static WebConfiguration CarregarDeXml(string xml)
        {
            var xmlSerializer = new XmlSerializer(typeof(WebConfiguration));
            using var stringReader = new StringReader(xml);
            return (WebConfiguration)xmlSerializer.Deserialize(stringReader);
        }

        public static WebConfiguration CarregarDeArquivo(string caminho)
        {
            var xmlSerializer = new XmlSerializer(typeof(WebConfiguration));
            using var fileStream = new FileStream(caminho, FileMode.Open);
            return (WebConfiguration)xmlSerializer.Deserialize(fileStream);
        }

        // Método para adicionar múltiplas regras
        public static WebConfiguration CriarConfiguracaoComMultiplasRegras(params (string name, string matchUrl, string actionUrl)[] regras)
        {
            var rules = regras.Select(r => new Rule
            {
                Name = r.name,
                StopProcessing = "true",
                Match = new Match { Url = r.matchUrl },
                Action = new Action { Type = "Rewrite", Url = r.actionUrl }
            }).ToArray();

            return new WebConfiguration
            {
                SystemWebServer = new SystemWebServer
                {
                    Rewrite = new Rewrite
                    {
                        Rules = new Rules { Rule = rules }
                    }
                }
            };
        }
    }
   
}
