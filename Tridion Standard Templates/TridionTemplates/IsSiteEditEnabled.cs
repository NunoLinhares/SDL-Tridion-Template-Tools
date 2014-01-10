using System.Globalization;
using System.Text;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Is SiteEdit Enabled")]
    public class IsSiteEditEnabled : ITemplate
    {
        public void SetValue(Package package, bool isEnabled)
        {
            Item item = package.CreateStringItem(ContentType.Text, isEnabled.ToString(CultureInfo.InvariantCulture));
            package.PushItem("IsSiteEditEnabled", item);
        }
        public void Transform(Engine engine, Package package)
        {
            if (engine.PublishingContext.PublicationTarget == null)
            {
                // Template Builder
                SetValue(package, false);
                return;
            }
            PublicationTarget target = engine.PublishingContext.PublicationTarget;
            if (target.Id == TcmUri.UriNull)
            {
                // Content Manager Preview, we might need markup since this may be UI's Update Preview
                SetValue(package, true);
                return;
            }
            if (target.LoadApplicationData("SiteEdit") == null)
            {
                // No App Data for this target, no SiteEdit
                SetValue(package, false);
                return;
            }

            ApplicationData appData = target.LoadApplicationData("SiteEdit");
            string data = Encoding.UTF8.GetString(appData.Data);

            XmlDocument seConfig = new XmlDocument();
            seConfig.LoadXml(data);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("se", "http://www.sdltridion.com/2011/SiteEdit");
            XmlNode seNode = seConfig.SelectSingleNode("/se:configuration/se:PublicationTarget/se:EnableSiteEdit", nsManager);
            if (seNode != null && seNode.InnerText.Equals("true"))
            {
                SetValue(package, true);
                return;
            }
            SetValue(package, false);
        }
    }
}
