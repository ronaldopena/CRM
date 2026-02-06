using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Poliview.crm.infra
{
    public class XmlConfigFileManager
    {
        private readonly string _filePath;

        public XmlConfigFileManager(string filePath)
        {
            _filePath = filePath;
        }

        public void UpdateSetting(string key, string newValue)
        {
            XDocument doc = XDocument.Load(_filePath);
            if (doc.Root == null) return;

            XElement? settingElement = doc.Root.Descendants("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key);

            if (settingElement != null)
            {
                settingElement.SetAttributeValue("value", newValue);
                doc.Save(_filePath);
            }
        }
    }
}
