using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Set SiteEdit Blueprint Context")]
    public class SetSiteEditBlueprintContext : ITemplate
    {
        public const string SiteEditComponentContext = "BluePrintingComponentContext";
        public const string SiteEditPageContext = "BluePrintingPageContext";
        public const string SiteEditPublishContext = "BluePrintingPublishContext";
        public void Transform(Engine engine, Package package)
        {
            TemplatingLogger log = TemplatingLogger.GetLogger(GetType());
            RepositoryLocalObject context;
            if (package.GetByName(Package.PageName) != null)
            {

                context = (RepositoryLocalObject)engine.GetObject(package.GetByName(Package.PageName));
                log.Debug("Setting context to page with ID " + context.Id);
            }
            else
            {
                log.Info("This template building block should only run on a page. Exiting.");
                return;
            }

            if (!(context is Page)) return;

            Page page = (Page)context;

            package.PushItem(SiteEditPageContext, package.CreateStringItem(ContentType.Text, page.OwningRepository.Id));
            package.PushItem(SiteEditPublishContext, package.CreateStringItem(ContentType.Text, page.ContextRepository.Id));

            if (page.ComponentPresentations.Count <= 0) return;
            Component component = page.ComponentPresentations[0].Component;
            package.PushItem(SiteEditComponentContext, package.CreateStringItem(ContentType.Text, component.OwningRepository.Id));
        }
    }
}
