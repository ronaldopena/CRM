using Microsoft.Extensions.Configuration;

namespace Poliview.crm.infra
{
    public class AppSettingsModifier
    {
        private readonly IConfiguration _configuration;

        public AppSettingsModifier(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void UpdateSetting(string key, string newValue)
        {
            IConfigurationSection section = _configuration.GetSection("AppSettings");

            section[key] = newValue; // Modifica a configuração
        }
    }
}
