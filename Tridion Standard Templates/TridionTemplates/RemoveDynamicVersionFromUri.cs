using System.Text.RegularExpressions;
using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Remove Dynamic version ('-v0') from Tcm Uris in output")]
    public class RemoveDynamicVersionFromUri : ITemplate
    {
        private static readonly Regex TcmUriRegEx = new Regex("tcm:(?<pubId>[0-9]+)-(?<itemId>[0-9]+)(-(?<itemType>[0-9]+))?(-v(?<version>[0-9]+))?", RegexOptions.Compiled);

        public void Transform(Engine engine, Package package)
        {
            TemplatingLogger log = TemplatingLogger.GetLogger(GetType());
            if (package.GetByName(Package.OutputName) == null)
            {
                log.Error("Could not find \"Output\" item in Package. This template building block should be the last TBB in your template.");
                return;
            }
            Item output = package.GetByName(Package.OutputName);

            string outputText = output.GetAsString();

            bool outputchanged = false;
            foreach (Match m in TcmUriRegEx.Matches(outputText))
            {
                log.Debug("Found " + m.Value);
                TcmUri uri = new TcmUri(m.Value);
                if(uri.GetVersionlessUri().ToString().Equals(m.Value)) continue;
                log.Debug("Found version information on uri " + m.Value + ". Removing.");
                outputText = outputText.Replace(m.Value, uri.GetVersionlessUri().ToString());
                outputchanged = true;
            }
            if (outputchanged)
            {
                output.SetAsString(outputText);
                package.Remove(output);
                package.PushItem(Package.OutputName, output);
            }
        }
    }
}
