using System.Collections.Generic;

namespace Sdl.Tridion.Community.Regions
{
    public static class PageExtensions
    {
        public static Dictionary<string, Region> GetRegions(this Page page)
        {
            PageTemplate pageTemplate = page.PageTemplate;
            string cacheKey = pageTemplate.Id + "regions";
            if (page.Session.Cache.Get("", cacheKey) != null)
                return (Dictionary<string, Region>)page.Session.Cache.Get("", cacheKey);

            Dictionary<string, Region> regions = new Dictionary<string, Region>();

            Dictionary<string, Keyword> themeKeywords = new Dictionary<string, Keyword>();

            if (pageTemplate.Metadata != null)
            {
                ItemFields pageTemplateMeta = new ItemFields(pageTemplate.Metadata, pageTemplate.MetadataSchema);
                if (pageTemplateMeta.Contains("theme"))
                {
                    KeywordField k = (KeywordField)pageTemplateMeta["theme"];
                    Keyword theme = k.Value;
                    ChildKeywordsFilter f = new ChildKeywordsFilter(page.Session);

                    foreach (Keyword keyword in theme.GetChildKeywords(f))
                    {
                        string title = keyword.Title;
                        string themepattern = theme.Title + " - ";
                        string overridenregionname = title.Replace(themepattern, "");
                        themeKeywords.Add(overridenregionname, keyword);
                    }
                }
            }

            foreach (ComponentPresentation cp in page.ComponentPresentations)
            {

                ComponentTemplate ct = cp.ComponentTemplate;
                if (ct.Metadata != null)
                {
                    ItemFields metadata = new ItemFields(ct.Metadata, ct.MetadataSchema);
                    if (metadata.Contains("region"))
                    {
                        KeywordField keywordField = (KeywordField)metadata["region"];
                        if (keywordField.Values.Count > 0)
                        {
                            Keyword regionKeyword = keywordField.Value;
                            string regionName = regionKeyword.Title;

                            if (regions.ContainsKey(regionName))
                            {
                                regions[regionName].ComponentPresentations.Add(cp);
                            }
                            else
                            {
                                Region region = new Region
                                {
                                    Name = regionName,
                                    ComponentPresentations = new List<ComponentPresentation> { cp }
                                };
                                if (themeKeywords.ContainsKey(regionName))
                                    region.RegionDefinitionsKeyword = themeKeywords[regionName];
                                else
                                    region.RegionDefinitionsKeyword = regionKeyword;

                                regions.Add(regionName, region);
                            }
                        }
                    }
                }
            }
            page.Session.Cache.Add("", cacheKey, regions);
            return regions;
        }
    }
}
