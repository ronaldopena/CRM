namespace Poliview.crm.models
{
    public class TrocarSenhaRequisicao
    {        
        public int idusuario { get; set; }
        public string? senhaatual { get; set; }
        public string? novasenha { get; set; }
        public string? repetirnovasenha { get; set; }
    }
}
