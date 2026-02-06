namespace Poliview.crm.domain
{
    public class TipoUnidade
    {
        public TipoUnidade()
        {
            id = 0;
            descricao = "";
            espacocliente = 1;
        }

        public int id { get; set; }
        public string descricao { get; set; }
        public int espacocliente { get; set; }
    }
}
