using System.Text.RegularExpressions;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class GetPageBreadcrumb : ITemplate
    {
        private const string Separator = " &raquo; ";
        private const string RegexPattern = @"^[\d]* ";
        private const string IndexPagePattern = "index";

        public void Transform(Engine engine, Package package)
        {
            TemplatingLogger log = TemplatingLogger.GetLogger(GetType());
            if (package.GetByName(Package.PageName) == null)
            {
                log.Info("Do not use this template building block in Component Templates");
                return;
            }

            Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));

            string output;
            if (page.Title.ToLower().Contains("index"))
                output = StripNumbersFromTitle(page.OrganizationalItem.Title);
            else
            {
                output = GetLinkToSgIndexPage((StructureGroup)page.OrganizationalItem, engine.GetSession()) + Separator + StripNumbersFromTitle(page.Title);
            }

            foreach (OrganizationalItem parent in page.OrganizationalItem.GetAncestors())
            {
                output = GetLinkToSgIndexPage((StructureGroup)parent, engine.GetSession()) + Separator + output;
            }

            package.PushItem("breadcrumb", package.CreateStringItem(ContentType.Html, output));
        }

        private string StripNumbersFromTitle(string title)
        {
            return Regex.Replace(title, RegexPattern, string.Empty);
        }

        private string GetLinkToSgIndexPage(StructureGroup sg, Session session)
        {
            OrganizationalItemItemsFilter filter = new OrganizationalItemItemsFilter(session) { ItemTypes = new[] { ItemType.Page } };
            string title = StripNumbersFromTitle(sg.Title);
            const string pageLinkFormat = "<a tridion:href=\"{0}\">{1}</a>";
            string result = null;
            foreach (XmlElement page in sg.GetListItems(filter).ChildNodes)
            {
                if (!page.Attributes["Title"].Value.ToLower().Contains(IndexPagePattern)) continue;
                result = string.Format(pageLinkFormat, page.Attributes["ID"].Value, title);
                break;
            }
            if (string.IsNullOrEmpty(result))
            {
                result = title;
            }
            return result;
        }
    }
}
