using System.IO;
using System.Text;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Get Site Navigation Xml (Keywords)")]
    public class KeywordBasedNavigation : ITemplate
    {
        private const string NavigationCategoryWebDavUrl = "/Site%20Navigation";
        private TemplatingLogger _log;

        public void Transform(Engine engine, Package package)
        {
            _log = TemplatingLogger.GetLogger(GetType());

            RepositoryLocalObject context =
                engine.GetObject(package.GetByName(Package.PageName)) as RepositoryLocalObject;
            if (context == null)
            {
                _log.Error("Could not retrieve page from package. Exiting.");
                return;
            }
            string categoryUrl = context.ContextRepository.WebDavUrl + NavigationCategoryWebDavUrl;
            Category navigation = (Category)engine.GetObject(categoryUrl);

            using (MemoryStream ms = new MemoryStream())
            {
                XmlTextWriter w = new XmlTextWriter(ms, new UTF8Encoding(false))
                {
                    Indentation = 4,
                    Formatting = Formatting.Indented
                };

                w.WriteStartDocument();
                w.WriteStartElement(Navigation.RootNodeName);
                KeywordsFilter filter = new KeywordsFilter(engine.GetSession()) { IsRoot = true };
                foreach (XmlNode rootChildren in navigation.GetListKeywords(filter))
                {
                    Keyword rootKeyword = (Keyword)engine.GetObject(rootChildren.Attributes["ID"].Value);
                    w.WriteStartElement(Navigation.NodeName);
                    NavigationNode n = new NavigationNode(rootKeyword);

                }
            }

        }
    }

    internal class NavigationNode
    {
        internal Keyword Keyword;
        private readonly XmlDocument _keywordMeta;
        private TemplatingLogger _log;
        private readonly XmlNamespaceManager _nm;
        private const string XpathBase = "/meta:Metadata/meta:Navigation/meta:";

        internal NavigationNode(Keyword keyword)
        {
            Keyword = keyword;
            _keywordMeta = new XmlDocument();
            _keywordMeta.LoadXml(keyword.Metadata.OuterXml);
            _nm = new XmlNamespaceManager(new NameTable());
            _nm.AddNamespace("meta", keyword.MetadataSchema.NamespaceUri);
            _log = TemplatingLogger.GetLogger(GetType());
        }


        internal string Title
        {
            get { return Keyword.Title; }
        }
        internal TcmUri Id { get { return Keyword.Id; } }
        internal string Description { get { return Keyword.Description; } }
        internal string Key { get { return Keyword.Key; } }

        internal bool IncludeInNavigation
        {
            get
            {
                const string xpath = XpathBase + "IncludeInNavigation";
                XmlNode node = _keywordMeta.SelectSingleNode(xpath, _nm);
                if (node == null) return false;
                if (node.InnerText == "Yes") return true;
                return false;
            }
        }

        internal string AlternateFriendlyNavigationTitle
        {
            get { return GetNodeValue("AlternateFriendlyNavigationTitle"); }
        }

        internal string NavigationMobileAlternateTitle
        {
            get { return GetNodeValue("NavigationMobileAlternateTitle"); }
        }

        private string GetNodeValue(string fieldName)
        {
            string xpath = XpathBase + fieldName;
            XmlNode node = _keywordMeta.SelectSingleNode(xpath, _nm);
            string result = node == null ? string.Empty : node.InnerText;
            return result;
        }
    }

}
