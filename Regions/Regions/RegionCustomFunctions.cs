using System.Collections.Generic;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Expression;

namespace Sdl.Tridion.Community.Regions
{
    public class RegionCustomFunctions : IFunctionSource
    {

        private Engine _Engine;
        private Package _Package;
        private TemplatingLogger _Log;

        [TemplateCallable]
        public string RenderRegion(string regionName)
        {
            _Log.Debug("Render region: " + regionName + " called");
            
            string output = string.Empty;
            Page page = (Page)_Engine.GetObject(_Package.GetByName(Package.PageName));

            var regions = page.GetRegions();

            if (regions.ContainsKey(regionName))            
                output = GenerateRegionOutput(regionName, output, regions);
            
            return output;
        }

        private string GenerateRegionOutput(string regionName, string output, Dictionary<string, Region> regions)
        {
            int repeatIndex = 1;

            // Added by Nuno, June 26 2013
            if(_Engine.PublishingContext.PublicationTarget.IsSiteEditEnabled())
                output += regions[regionName].ToJson();

            foreach (var cp in regions[regionName].ComponentPresentations)
            {
                //SetVariable("Constraints", regions[regionName].RegionConstraints);
                SetVariable("isFirst", isFirst(repeatIndex));
                SetVariable("isLast", isLast(repeatIndex, regions[regionName]));
                SetVariable("index", repeatIndex);
                SetVariable("totalCount", regions[regionName].ComponentPresentations.Count);
                output += _Engine.RenderComponentPresentation(cp.Component.Id, cp.ComponentTemplate.Id);

                repeatIndex++;
                //TODO: Clean variables
            }
            return output;
        }

        private bool isFirst(int repeatIndex)
        {
            if (repeatIndex == 1)
                return true;
            return false;
        }

        private bool isLast(int repeatIndex, Region region)
        {
            if (region.ComponentPresentations.Count == repeatIndex)
                return true;
            return false;
        }

        public void Initialize(Engine engine, Package package)
        {
            _Engine = engine;
            _Package = package;
            _Log = TemplatingLogger.GetLogger(GetType());

        }

        private void SetVariable(string variableName, object value)
        {
            _Engine.PublishingContext.RenderContext.ContextVariables.Remove(variableName);
            _Engine.PublishingContext.RenderContext.ContextVariables.Add(variableName, value);


        }


        public object GetVariable(string variableName)
        {
            //Get the varialbe
            return _Engine.PublishingContext.RenderContext.ContextVariables[variableName];
        }


    }
}
