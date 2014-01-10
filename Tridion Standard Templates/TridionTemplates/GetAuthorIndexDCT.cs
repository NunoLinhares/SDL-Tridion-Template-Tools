using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Get Author Index Component Template")]
    public class GetAuthorIndexDct : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            const string componentTemplateWebdavUrl = "/System/Component%20Templates/Promo%20Content.tctcmp";
            if (package.GetByName(Package.PageName) == null) return;
            Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));

            ComponentTemplate promoCt = (ComponentTemplate)engine.GetObject(page.ContextRepository.RootFolder.WebDavUrl + componentTemplateWebdavUrl);
            package.PushItem("promoCtId", package.CreateStringItem(ContentType.Text, promoCt.Id));

        }
    }
}
