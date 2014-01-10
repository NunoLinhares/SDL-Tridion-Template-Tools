using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Render Code")]
    public class RenderCode : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            Component component = (Component)engine.GetObject(package.GetByName(Package.ComponentName));
            ItemFields fields = new ItemFields(component.Content, component.Schema);
            TextField code = (TextField)fields["Code"];
            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Html, code.Value));
        }
    }
}
