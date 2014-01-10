using System;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class ResolvePageUrisForLinks : ITemplate
    {
        private const string Pattern = "origin=\"tcm:0-0-0\"";
        public void Transform(Engine engine, Package package)
        {
            Item output = package.GetByName(Package.OutputName);
            string content = output.GetAsString();
            if (content.IndexOf(Pattern, StringComparison.Ordinal) <= 1) return;

            Page page = engine.PublishingContext.RenderContext.ContextItem as Page;
            if (page == null) return;

            string newOrigin = "origin=\"" + page.Id + "\"";
            content = content.Replace(Pattern, newOrigin);
            output.SetAsString(content);
            package.PushItem(Package.OutputName, output);
        }
    }
}
