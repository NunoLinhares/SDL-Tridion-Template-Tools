using System.Collections.Generic;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using CP = Tridion.ContentManager.CommunicationManagement.ComponentPresentation;

namespace TridionTemplates
{
    [TcmTemplateTitle("Get SEO Data")]
    public class GetSeoData : ITemplate
    {
        private const string SeoKeywordsName = "SEOKeywords";
        private const string SeoDescriptionName = "SEODescription";
        public void Transform(Engine engine, Package package)
        {
            // Find the best fit for SEO Keywords and Description for this page's content.
            // Leave if we're not rendering a page
            if (package.GetByName(Package.PageName) == null) return;

            List<string> seoKeywords = new List<string>();
            string seoDescription = string.Empty;

            Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));
            if (page.Metadata != null)
            {
                ItemFields metadata = new ItemFields(page.Metadata, page.MetadataSchema);
                foreach (ItemField field in metadata)
                {
                    if (field.Name.Equals("ContentSource"))
                        seoKeywords.Add(((KeywordField)field).Value.Title);
                    if (field.Name.Equals("Keywords"))
                    {
                        TextField mvKeywords = (TextField)field;
                        foreach (string keyword in mvKeywords.Values)
                        {
                            seoKeywords.Add(keyword);
                        }
                    }
                    if (field.Name.Equals("Description"))
                    {
                        TextField description = (TextField)field;
                        seoDescription = description.Value;
                    }
                }
            }
            // Check if we have data...
            if (string.IsNullOrEmpty(seoDescription))
            {
                // Get description from first component.
                // If it exists...
                // If page has no components, then it must be a dynamic index page, we should get the description to match the page title
                if (page.ComponentPresentations.Count.Equals(0))
                {
                    seoDescription = package.GetValue("ArticlesByText");
                    // and the first seoKeyword should be the Content Source field.
                    if (seoKeywords.Count >= 1)
                    {
                        seoDescription += " " + seoKeywords[0];
                    }
                }
                else
                {
                    // Let's look at the content
                    // First find the type of page.
                    if (page.PageTemplate.Title.Contains("Content") || page.PageTemplate.Title.Contains("Home"))
                    {
                        // This is either a one page article, or the home page. In both cases, the SEO Description is the ArticleTitle of the first component
                        CP cp = page.ComponentPresentations[0];
                        Component c = cp.Component;
                        if (c.Schema.Title.Equals("Article"))
                        {
                            ItemFields content = new ItemFields(c.Content, c.Schema);
                            TextField title = (TextField)content["ArticleTitle"];
                            seoDescription = title.Value;
                            // If we have no keywords and the it's a content page, let's add the author to the keywords
                            if (seoKeywords.Count == 0 && page.PageTemplate.Title.Contains("Content"))
                            {
                                KeywordField source = (KeywordField)content["Source"];
                                if (source.Values.Count > 0)
                                {
                                    seoKeywords.Add(source.Value.Title);
                                }
                            }
                        }
                    }
                }
            }
            if (!(string.IsNullOrEmpty(seoDescription)))
            {
                package.PushItem(SeoDescriptionName, package.CreateStringItem(ContentType.Text, seoDescription));
            }
            if (seoKeywords.Count <= 0) return;
            string keywords = string.Empty;
            for (int i = 0; i < seoKeywords.Count; i++)
            {
                if (i > 0) keywords += ", ";
                keywords += seoKeywords[i];
            }
            package.PushItem(SeoKeywordsName, package.CreateStringItem(ContentType.Text, keywords));
        }
    }
}
