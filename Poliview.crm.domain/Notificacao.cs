using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Notificacao
    {
        public string? id { get; set; }
        public string? mensagem { get; set; }
    }

    public class NotificacaoCliente
    {
        public int boletovencido { get; set; }
        public int boletoavencer { get; set; }
        public int aniversario { get; set; }
        public int aniversariocontrato { get; set; }
        public int residuo { get; set; }
        public int quitacao { get; set; }
    }

    
    public class JaLeuNotificacaoRequisicao
    {
        public string cpf { get; set; }
        public string ano { get; set; }
        public string mes { get; set; }
    }

    public class JaLeuNotificacao
    {
        public string aniversario { get; set; }
        public string aniversario_contrato { get; set; }
        public string boleto_vencer { get; set; }
        public string boleto_vencido { get; set; }
        public string quitacao { get; set; }
        public string residuo { get; set; }
    }

    public class JaLeuNotificacaoResposta : IRetorno
    {
        public JaLeuNotificacao objeto { get; set; }
    }

}
