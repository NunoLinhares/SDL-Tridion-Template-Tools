using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Get Component Template Id")]
    public class GetComponentTemplateId : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            TcmUri templateId = engine.PublishingContext.ResolvedItem.Template.Id;
            if (templateId.ItemType != ItemType.ComponentTemplate) return;
            Item item = package.CreateStringItem(ContentType.Text, templateId);
            package.PushItem("ComponentTemplateId", item);
            package.PushItem("RenderMode", package.CreateStringItem(ContentType.Text, engine.RenderMode.ToString()));
        }
    }
}
