
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class GetArticleTopics : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            Component c = (Component)engine.GetObject(package.GetByName(Package.ComponentName));
            ItemFields content = new ItemFields(c.Content, c.Schema);
            KeywordField topic = (KeywordField)content["ContentCategory"];
            string topics = string.Empty;
            int count = 0;
            if (topic.Values.Count > 0)
            {
                topics += "(";
                foreach (Keyword key in topic.Values)
                {
                    if (count > 0) topics += ", ";
                    topics += key.Title;
                    count++;
                }
                topics += ")";
            }
            package.PushItem("Topics", package.CreateStringItem(ContentType.Text, topics));
        }
    }
}
