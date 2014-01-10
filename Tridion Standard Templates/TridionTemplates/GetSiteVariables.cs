using System;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class GetSiteVariables : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            RepositoryLocalObject context;
            if (package.GetByName(Package.ComponentName) != null)
            {
                context = (RepositoryLocalObject)engine.GetObject(package.GetByName(Package.ComponentName));
            }
            else if (package.GetByName(Package.PageName) != null)
            {
                context = (RepositoryLocalObject)engine.GetObject(package.GetByName(Package.PageName));
            }
            else
            {
                throw new Exception("Could not determine context from package. Did not find page or component in package");
            }
            Repository contextPublication = context.ContextRepository;
            if (contextPublication.Metadata == null) return;
            ItemFields metadata = new ItemFields(contextPublication.Metadata, contextPublication.MetadataSchema);
            ComponentLinkField configuration = (ComponentLinkField)metadata["SiteConfiguration"];
            foreach (Component c in configuration.Values)
            {
                ItemFields content = new ItemFields(c.Content, c.Schema);
                foreach (ItemField field in content)
                {
                    var textField = field as TextField;
                    if (textField != null)
                    {
                        package.PushItem(textField.Name, package.CreateStringItem(ContentType.Text, textField.Value));
                    }
                }
            }
        }
    }
}
