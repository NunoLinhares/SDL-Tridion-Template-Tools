using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class CompareHtmlvsTextItem : ITemplate
    {
        public void Transform(Engine engine, Package p)
        {
            Component component = (Component)engine.GetObject(p.GetByName(Package.ComponentName));
            ItemFields fields = new ItemFields(component.Content, component.Schema);
            SingleLineTextField field = (SingleLineTextField) fields["ArticleTitle"];

            p.PushItem("AsHtml", p.CreateHtmlItem(System.Security.SecurityElement.Escape(field.Value)));
            p.PushItem("AsText", p.CreateStringItem(ContentType.Text, System.Security.SecurityElement.Escape(field.Value)));

        }
    }
}
