namespace Poliview.crm.espacocliente
{
    public static class Configuration
    {
        public static string UrlApicrm { get; private set; }
        public static string HttpClientName = "crmHttpClient";

        static Configuration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            UrlApicrm = config["urlapi"];
        }
    }
}