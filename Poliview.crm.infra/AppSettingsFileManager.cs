using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace Poliview.crm.infra
{
    public class AppSettingsFileManager
    {
        private readonly string _filePath;

        public AppSettingsFileManager(string filePath)
        {
            _filePath = filePath;
        }

        public void UpdateSetting(string key, string newValue)
        {
            JObject appSettings = JObject.Parse(File.ReadAllText(_filePath));

            string jsonString = File.ReadAllText(_filePath);
            var jsonNode = JsonNode.Parse(jsonString);
            if (jsonNode == null) return;

            JsonObject jsonObject = jsonNode.AsObject();
            if (jsonObject.ContainsKey(key))
            {
                jsonObject[key] = newValue;
            }

            //JObject appSettingsSection = (JObject)appSettings[key];
            //appSettingsSection[key] = newValue;

            // File.WriteAllText(_filePath, appSettings.ToString());
            File.WriteAllText(_filePath, jsonObject.ToString());
        }

    }
}
