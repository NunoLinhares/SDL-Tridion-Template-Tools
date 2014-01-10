using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using CP = Tridion.ContentManager.CommunicationManagement.ComponentPresentation;

namespace TridionTemplates
{
    [TcmTemplateTitle("Add Header and Footer to Page")]
    public class AddHeaderAndFooterToPage : ITemplate
    {
        private const string HeaderComponentTemplateUrl = "/System/Component%20Templates/Header.tctcmp";
        private const string HeaderComponentUrl = "/Content/Header%20and%20Footer/Site%20Header.xml";
        private const string FooterComponentTemplateUrl = "/System/Component%20Templates/Footer.tctcmp";
        private const string FooterComponentUrl = "/Content/Header%20and%20Footer/Site%20Footer.xml";
        public void Transform(Engine engine, Package package)
        {
            if (package.GetByName(Package.PageName) == null) return;
            Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));
            bool hasHeader = false;
            bool hasFooter = false;

            foreach (CP cp in page.ComponentPresentations)
            {
                if (cp.ComponentTemplate.Title.ToLower().Contains("header")) hasHeader = true;
                if (cp.ComponentTemplate.Title.ToLower().Contains("footer")) hasFooter = true;
            }
            if (!hasHeader)
            {
                ComponentTemplate headerCt = (ComponentTemplate)engine.GetObject(page.ContextRepository.RootFolder.WebDavUrl + HeaderComponentTemplateUrl);
                Component header = (Component)engine.GetObject(page.ContextRepository.RootFolder.WebDavUrl + HeaderComponentUrl);
                package.PushItem("headerCP", package.CreateStringItem(ContentType.Html, string.Format("<tcdl:ComponentPresentation type=\"Dynamic\" componentURI=\"{0}\" templateURI=\"{1}\" />", header.Id, headerCt.Id)));
            }
            if (!hasFooter)
            {
                ComponentTemplate footerCt = (ComponentTemplate)engine.GetObject(page.ContextRepository.RootFolder.WebDavUrl + FooterComponentTemplateUrl);
                Component footer = (Component)engine.GetObject(page.ContextRepository.RootFolder.WebDavUrl + FooterComponentUrl);
                package.PushItem("footerCP", package.CreateStringItem(ContentType.Html, string.Format("<tcdl:ComponentPresentation type=\"Dynamic\" componentURI=\"{0}\" templateURI=\"{1}\" />", footer.Id, footerCt.Id)));

            }
        }
    }
}
