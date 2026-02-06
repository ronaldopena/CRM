namespace Poliview.crm.models
{
    public class TokenReposta
    {
        public string? token_type { get; set; }
        public string? scope { get; set; }
        public int expire_in { get; set; }
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
    }
}