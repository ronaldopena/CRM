using Poliview.crm.domain;
using Poliview.crm.models;


namespace Poliview.crm.services
{
    public interface IConfigCrmService
    {
        public ConfigCrm getConfigCrm();
        public MenusPermissao getMenus(int idempreendimento);
    }
}
