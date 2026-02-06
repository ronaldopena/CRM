
namespace Poliview.crm.domain
{
    public class Jwt
    {
        public string? key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? Subject { get; set; }
    }

    public class UserJwt
    {
        public string? id { get; set; }
        public string? nome { get; set; }
        public string? email { get; set; }
        public string? cpfcnpj { get; set; }
    }
}
