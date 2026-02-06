using apimobuss.Dominio;

namespace apimobuss.Entidades
{
    public class IntegracaoResposta
    {
        public IEnumerable<Empreendimentos> empreendimentos { get; set; } = new List<Empreendimentos>();
        public IEnumerable<Blocos> blocos { get; set; } = new List<Blocos>();
        public IEnumerable<Unidades> unidades { get; set; } = new List<Unidades>();
        public IEnumerable<Clientes> clientes { get; set; } = new List<Clientes>();
    }
}
