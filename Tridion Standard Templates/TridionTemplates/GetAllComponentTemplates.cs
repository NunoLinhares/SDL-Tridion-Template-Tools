using System;
using System.Diagnostics;
using System.Xml;
using Tridion;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class GetAllComponentTemplates : ITemplate
    {
        private readonly TemplatingLogger _log = TemplatingLogger.GetLogger(typeof(GetAllComponentTemplates));

        public void Transform(Engine engine, Package package)
        {
            _log.Debug("GetAllComponentTemplates: start Transform");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Session session = engine.GetSession();
            ICache cache = session.Cache;

            String componentTemplateName = String.Empty;
            if (package.GetValue("ComponentTemplateName") != null)
            {
                componentTemplateName = package.GetValue("ComponentTemplateName");
            }

            if (!(String.IsNullOrEmpty(componentTemplateName)) && (package.GetByName(componentTemplateName) != null))
            {
                // Component Template found in Package
                return;
            }

            RepositoryLocalObject context;
            if (package.GetByName(Package.ComponentName) != null)
                context = engine.GetObject(package.GetByName(Package.ComponentName)) as RepositoryLocalObject;
            else
                context = engine.GetObject(package.GetByName(Package.PageName)) as RepositoryLocalObject;

            if (context != null)
            {
                Repository contextPublication = context.ContextRepository;

                RepositoryItemsFilter filter = new RepositoryItemsFilter(session)
                    {
                        ItemTypes = new[] { ItemType.ComponentTemplate },
                        Recursive = true,
                        BaseColumns = ListBaseColumns.IdAndTitle
                    };

                XmlNamespaceManager nm = new XmlNamespaceManager(new NameTable());
                nm.AddNamespace(Constants.TcmPrefix, Constants.TcmNamespace);
                XmlNodeList allComponentTemplates;

                if (cache.Get("ComponentTemplate", "listcts") != null)
                {
                    allComponentTemplates = (XmlNodeList)cache.Get("ComponentTemplate", "listcts");
                    _log.Debug("GetAllComponentTemplates: list already in cache");
                }
                else
                {
                    allComponentTemplates = contextPublication.GetListItems(filter).SelectNodes("/tcm:ListItems/tcm:Item", nm);
                    cache.Add("ComponentTemplate", "listcts", allComponentTemplates);
                    _log.Debug("GetAllComponentTemplates: list created in cache");
                }

                if (allComponentTemplates != null)
                    foreach (XmlNode ct in allComponentTemplates)
                    {
                        if (ct.Attributes != null)
                        {
                            String ctName = ct.Attributes["Title"].Value.Replace(" ", "").Replace("-", "");
                            // Avoid duplicates in Package
                            // Possible performance impact, but could be needed if ComponentTemplateName is set to an empty String
                            if (package.GetByName(ctName) == null)
                                package.PushItem(ctName, package.CreateStringItem(ContentType.Text, ct.Attributes["ID"].Value));
                        }
                    }
            }
            watch.Stop();
            _log.Debug("Template finished in " + watch.ElapsedMilliseconds + " milliseconds.");
        }

    }
}
