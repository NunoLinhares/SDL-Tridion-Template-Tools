using System;
using System.Collections.Generic;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using comm = Tridion.ContentManager.CommunicationManagement;

namespace Tridion.Extensions.ContentManager.Templating
{
    /// <summary>
    /// Group the component presentations in the package by template
    /// This can be useful on pages where the components are grouped
    /// into different display areas based on their template
    /// A Component List package item is created for each template type used
    /// on the page, based on a token (default is CT title with spaces removed) and the 
    /// postfix 'Components'. So all items using the 'Article Summary' CT
    /// would be put in a list named 'ArticleSummaryComponents'
    /// It is possible to specify a comma separated list of tokens in 
    /// the ComponentTemplateTitleMatchTokens parameter, to enable more control
    /// over the grouping. If a template title contains a token, components
    /// using that template are put in that group. For example if you specify a 
    /// value of 'Left,Right' and have templates 'Article Summary', 'Teaser Right',
    /// 'Banner Right', 'Teaser Left', you will get 3 groups:
    /// LeftComponents (containing Teaser Left components)
    /// RightComponents (containing Teaser Right and Banner Right components)
    /// Components (all other components)
    /// </summary>
    [TcmTemplateTitle("Group Components By Template Type")]
    public class GroupComponentsByTemplateType : ITemplate
    {
        private readonly List<string> _matchTokens = new List<string>();

        public void Transform(Engine engine, Package package)
        {
            if (package.GetByName(Package.PageName) == null) return;
            comm.Page page = (comm.Page)engine.GetObject(package.GetByName(Package.PageName));
            Dictionary<string, List<ComponentPresentation>> lists = new Dictionary<string, List<ComponentPresentation>>();
            string matchTokens = package.GetValue("ComponentTemplateTitleMatchTokens");
            if (!String.IsNullOrEmpty(matchTokens))
            {
                foreach (string token in matchTokens.Split(','))
                { 
                    _matchTokens.Add(token.Trim());
                }
            }
            
            foreach (comm.ComponentPresentation cp in page.ComponentPresentations)
            {
                string ct = GetPresentationType(cp.ComponentTemplate.Title);
                if (!lists.ContainsKey(ct))
                    lists.Add(ct, new List<ComponentPresentation>());
                lists[ct].Add(new ComponentPresentation(cp.Component.Id, cp.ComponentTemplate.Id));
            }

            foreach (string token in lists.Keys)
            {
                Item item = package.CreateStringItem(ContentType.ComponentArray, ComponentPresentationList.ToXml(lists[token]));
                package.PushItem(token + "Components", item);
            }
        }

        private string GetPresentationType(string templateTitle)
        {
            //check if we are grouping using tokens
            if (_matchTokens.Count>0)
            {
                foreach (string token in _matchTokens)
                {
                    if (templateTitle.Contains(token))
                        return token;
                }
                //Default is the standard Components array
                return "";
            }
            //if no tokens are specified, we group by the template titles themselves
            return templateTitle.Replace(" ", "") ;
        }

    }
}
