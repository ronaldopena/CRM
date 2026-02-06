namespace Poliview.crm.models
{
    public class LoginRequisicao
    {
        public string? usuario { get; set; }
        public string? senha { get; set; }
        public int origem { get; set; }
        public int idempresa { get; set; }
    }
    // ORIGEM => 0 = PORTAL, 1 = CRM, 2 = APP, 3 = MOBUSS
}


