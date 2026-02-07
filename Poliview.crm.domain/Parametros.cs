using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Parametros
    {
        public string? caminhoSiecon { get; set; }
        public string? caminhoPdf { get; set; }
        public string? caminhoHTML { get; set; }
        public string? cssHTML { get; set; }
        public string? urlExternaHTML { get; set; }
        public string? versao { get; set; }
        public string? urlExterna { get; set; }
        public string? avisoHtml { get; set; }
        public string? avisoArquivo { get; set; }
        public Boolean avisoMostrar { get; set; }
        public int senhaComprimento { get; set; }
        public int senhaMinimoMaiusculo { get; set; }
        public int senhaMinimoMinusculo { get; set; }
        public int senhaMinimoNumerico { get; set; }
        public int senhaMinimoAlfanumerico { get; set; }
        public int senhaTentativasLogin { get; set; }
        public int senhaCoincidir { get; set; }
        public int TipoAutenticacaoEmail { get; set; }
        public int intervaloRecebimentoEmailMinutos { get; set; }
        public int intervaloEnvioEmailMinutos { get; set; }
        public string emailDestinatarioSuporte { get; set; }
        public int tipoAcessoSiecon { get; set; }
        public string? usuarioApiSiecon { get; set; }
        public string? senhaApiSiecon { get; set; }
        public string? urlApiSiecon { get; set; }
        public int qtdeEmailsEnvio { get; set; }
        public int tamanhoMaximoAnexos { get; set; }
        public string emailErrosAdmin { get; set; }
        public int habilitarEspacoCliente { get; set; }
        public int empreendimentoTesteEspacoCliente { get; set; }
        public string nomeremetente { get; set; }
        public string emailremetente { get; set; }
        /// <summary>Integração SieconSP7: Servidor</summary>
        public string? NM_ServidorInteg { get; set; }
        /// <summary>Integração SieconSP7: Usuário</summary>
        public string? NM_UsuarioInteg { get; set; }
        /// <summary>Integração SieconSP7: Senha</summary>
        public string? DS_SenhaUserInteg { get; set; }
        /// <summary>Integração SieconSP7: Caminho do servidor (path DB)</summary>
        public string? DS_PathDbInteg { get; set; }
        /// <summary>Integração SieconSP7: Porta do servidor</summary>
        public string? DS_portaServidorInteg { get; set; }
        /// <summary>Jornada padrão SLA</summary>
        public int? ID_JornadaSLA { get; set; }
        /// <summary>Jornada padrão Recurso</summary>
        public int? ID_JornadaRecurso { get; set; }
    }

    public class ConfigEspacoCliente
    {
        public bool habilitado { get; set; }
        public bool leituraobrigatoria { get; set; }
    }

    public class BotaoLogin
    {
        public int habilitabotaologin { get; set; }
        public string? urliconebotaologin { get; set; }
        public string? textoiconebotaologin { get; set; }
        public int alturaiconebotaologin { get; set; }
        public int larguraiconebotaologin { get; set; }
        public string? urlexternabotaologin { get; set; }

    }
}
