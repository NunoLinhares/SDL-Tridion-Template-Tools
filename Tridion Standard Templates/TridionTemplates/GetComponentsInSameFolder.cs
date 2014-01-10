using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class GetComponentsInSameFolder : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            TemplatingLogger log = TemplatingLogger.GetLogger(GetType());
            if (package.GetByName(Package.ComponentName) == null)
            {
                log.Info("This template should only be used with Component Templates. Could not find component in package, exiting");
                return;
            }
            var c = (Component)engine.GetObject(package.GetByName(Package.ComponentName));
            var container = (Folder)c.OrganizationalItem;
            var filter = new OrganizationalItemItemsFilter(engine.GetSession()) { ItemTypes = new[] { ItemType.Component } };

            // Always faster to use GetListItems if we only need limited elements
            foreach (XmlNode node in container.GetListItems(filter))
            {
                string componentId = node.Attributes["ID"].Value;
                string componentTitle = node.Attributes["Title"].Value;
            }

            // If we need more info, use GetItems instead
            foreach (Component component in container.GetItems(filter))
            {
                // If your filter is messed up, GetItems will return objects that may
                // not be a Component, in which case the code will blow up with an
                // InvalidCastException. Be careful with filter.ItemTypes[]
                Schema componentSchema = component.Schema;
                SchemaPurpose purpose = componentSchema.Purpose;
                XmlElement content = component.Content;
            }


        }
    }
}
