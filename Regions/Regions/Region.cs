using System;
using System.Collections.Generic;
using System.Text;
using Sdl.Ripple;

namespace Sdl.Tridion.Community.Regions
{
    public class Region
    {
        private ItemFields _regionMeta = null;
        private List<RegionConstraints> _regionConstraints = null;
        private int? _minOccurs = null;
        private int? _maxOccurs = null;

        public List<ComponentPresentation> ComponentPresentations;
        public string Name;
        public Keyword RegionDefinitionsKeyword;

        public List<RegionConstraints> RegionConstraints
        {
            get
            {
                if (_regionConstraints != null) return _regionConstraints;
                _regionConstraints = GetRegionConstraints();
                return _regionConstraints;
            }
        }
        public int MaxOccurs
        {
            get
            {
                if (_maxOccurs.HasValue) return _maxOccurs.Value;

                if (_regionMeta == null) _regionMeta = GetKeywordMetadata();
                NumberField f = (NumberField)_regionMeta["maxoccurs"];
                _maxOccurs = Convert.ToInt32(f.Value);
                return _maxOccurs.Value;
            }
        }
        public int MinOccurs
        {
            get
            {
                if (_minOccurs.HasValue) return _minOccurs.Value;
                if (_regionMeta == null) _regionMeta = GetKeywordMetadata();
                NumberField f = (NumberField)_regionMeta["minoccurs"];
                _minOccurs = Convert.ToInt32(f.Value);
                return _minOccurs.Value;
            }
        }

        public string ToJson()
        {
/*
    * <!-- Start Region: {
  title: "My Region",
  allowedComponentTypes: [
    {
      schema: "tcm:2-26-8",
      template: "tcm:2-32-32"
    },
    {
      schema: "tcm:2-27-8",
      template: "tcm:2-32-32"
    }
  ],
  minOccurs: 1,
  maxOccurs: 0
} -->
             */
            StringBuilder s = new StringBuilder();
            s.Append("<!-- Start Region: {title:\"").Append(Name).Append("\",");
            s.Append("allowedComponentTypes:[");
            int count = 0;
            foreach (RegionConstraints rc in RegionConstraints)
            {
                if (count > 0) s.Append(",");
                s.Append("{schema:\"").Append(rc.SchemaId).Append("\",");
                s.Append("template:\"").Append(rc.ComponentTemplateId).Append("\"}");
                count++;
            }
            s.Append("],");
            s.Append("minOccurs:").Append(MinOccurs).Append(",");
            s.Append("maxOccurs:").Append(MaxOccurs).Append("} -->");
            return s.ToString();
        }


        private List<RegionConstraints> GetRegionConstraints()
        {
            List<RegionConstraints> constraints = new List<RegionConstraints>();
            if (_regionMeta == null)
            {
                _regionMeta = GetKeywordMetadata();
            }
            EmbeddedSchemaField schematemplatepairs = (EmbeddedSchemaField)_regionMeta["schematemplatepairs"];
            foreach (ItemFields fields in schematemplatepairs.Values)
            {
                RegionConstraints rc = new RegionConstraints
                    {
                        SchemaId = ResolveUrl(fields["schema"]),
                        ComponentTemplateId = ResolveUrl(fields["template"])
                    };
                constraints.Add(rc);
            }
            return constraints;
        }

        private TcmUri ResolveUrl(ItemField field)
        {
            string itemUrl = RegionDefinitionsKeyword.ContextRepository.WebDavUrl;
            itemUrl += ((SingleLineTextField)field).Value;
            IdentifiableObject item;
            try
            {
                item = RegionDefinitionsKeyword.Session.GetObject(itemUrl);
            }
            catch (Exception)
            {
                throw new Exception(
                    string.Format(
                        "Error resolving URL for region constraint. Item {0} in keyword {1} could not be read.", itemUrl,
                        RegionDefinitionsKeyword.Id));
            }
            return item.Id;
        }

        private ItemFields GetKeywordMetadata()
        {
            if (RegionDefinitionsKeyword == null)
            {
                throw new Exception("Region was initialized without a keyword - cannot read metadata!");
            }
            if (RegionDefinitionsKeyword.Metadata == null)
            {
                throw new Exception("Region Definition Keyword " + RegionDefinitionsKeyword.Id +
                                    " does not have metadata!");
            }
            return new ItemFields(RegionDefinitionsKeyword.Metadata, RegionDefinitionsKeyword.MetadataSchema);
        }
    }


}
