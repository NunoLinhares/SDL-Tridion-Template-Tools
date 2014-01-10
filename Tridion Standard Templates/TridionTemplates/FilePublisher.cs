using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.ContentManagement;

namespace TridionTemplates
{
    [TcmTemplateTitle("File Publisher")]
    public class FilePublisher : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            Component component = (Component)engine.GetObject(package.GetByName(Package.ComponentName));
            package.PushItem(package.CreateMultimediaItem(component.Id));
            if (package.GetByName(Package.OutputName) == null)
            {
                Item output = package.CreateStringItem(ContentType.Text, string.Empty);
                package.PushItem(Package.OutputName, output);
            }
            
        }

    }
}
