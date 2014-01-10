using System;
using System.Text.RegularExpressions;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Enable JSTL")]
    public class EnableJSTL : ITemplate
    {
        private static readonly Regex JstlRegex = new Regex(@"\$\[.*\]");
        public void Transform(Engine engine, Package package)
        {
            Item outputItem = package.GetByName(Package.OutputName);
            string outputText = outputItem.GetAsString();

            Match match = JstlRegex.Match(outputText);
            while (match.Success)
            {
                String replaceJstl = match.Value.Replace("[", "{");
                replaceJstl = replaceJstl.Replace("]", "}");
                outputText = outputText.Replace(match.Value, replaceJstl);
                match = match.NextMatch();
            }
            outputItem.SetAsString(outputText);
            package.Remove(outputItem);
            package.PushItem(Package.OutputName, outputItem);
        }
    }
}
