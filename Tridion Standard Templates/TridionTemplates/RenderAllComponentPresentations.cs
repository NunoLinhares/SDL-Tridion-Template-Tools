using System.Text;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using CP = Tridion.ContentManager.CommunicationManagement.ComponentPresentation;
namespace TridionTemplates
{
    [TcmTemplateTitle("Render All Component Presentations")]
    public class RenderAllComponentPresentations : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));
            StringBuilder output = new StringBuilder();
            foreach (CP cp in page.ComponentPresentations)
            {
                output.Append(engine.RenderComponentPresentation(cp.Component.Id, cp.ComponentTemplate.Id));
            }
            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Html, output.ToString()));
        }
    }
}
