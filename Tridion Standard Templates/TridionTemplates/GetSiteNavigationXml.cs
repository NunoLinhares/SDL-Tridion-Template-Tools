using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    [TcmTemplateTitle("Get Site Navigation Xml")]
    public class StructureGroupNavigation : ITemplate
    {
        private int _countStructureGroups;
        private int _countPages;
        private int _countFields;
        private const string RegexPattern = @"^[\d]* ";
        private readonly Regex _regex = new Regex(RegexPattern, RegexOptions.None);
        private static TemplatingLogger _log;
        private Engine _engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureGroupNavigation"/> class.
        /// </summary>
        public StructureGroupNavigation()
        {
            _countStructureGroups = 0;
            _countPages = 0;
            _countFields = 0;
        }

        public void Transform(Engine engine, Package package)
        {
            _engine = engine;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            // Initialize internal variables
            _log = TemplatingLogger.GetLogger(GetType());
            _log.Debug("Initialized");
            // Check if template should run
            if (package.GetByName(Package.PageName) == null)
            {
                _log.Error("This templating building block must be executed in the context of a page.");
                return;
            }

            // Get the current publishing context
            Page context = (Page)engine.GetObject(package.GetByName(Package.PageName));
            // Get the current publication
            Publication contextPublication = (Publication)context.ContextRepository;
            // Get the Root Structure Group (starting point for the navigation)
            StructureGroup root = contextPublication.RootStructureGroup;

            // Prepare memory stream with Xml document
            using (MemoryStream stream = new MemoryStream())
            {
                // Use XmlTextWriter to write our navigation
                XmlTextWriter writer = new XmlTextWriter(stream, new UTF8Encoding(false)) { Indentation = 4, Formatting = Formatting.Indented };

                writer.WriteStartDocument();
                writer.WriteStartElement(Navigation.RootNodeName);

                // Get Root Structure Group Attributes
                string rootPath = root.PublishLocationUrl;
                string rootTitle = root.Title;
                string rootUri = root.Id;

                if (Navigation.IncludePathAttribute) writer.WriteAttributeString(Navigation.PathAttributeName, rootPath);
                writer.WriteAttributeString(Navigation.TitleAttributeName, rootTitle);
                if (Navigation.IncludeUriAttribute) writer.WriteAttributeString(Navigation.UriAttributeName, rootUri);
                writer.WriteAttributeString(Navigation.TypeAttributeName, root.Id.ItemType.ToString());

                if (Navigation.IncludePublicationMetadata)
                {
                    if (contextPublication.Metadata != null)
                    {
                        ItemFields publicationMetadata = new ItemFields(contextPublication.Metadata, contextPublication.MetadataSchema);
                        AddMetadataAttributes(publicationMetadata, writer);
                    }
                }
                // Add the Root Structure Group to the count
                _countStructureGroups++;
                foreach (RepositoryLocalObject item in root.GetItems())
                {
                    ProcessNavigation(writer, item);
                }

                writer.WriteEndElement();
                if (Navigation.IncludePerformanceMetrics)
                {
                    writer.WriteComment("Navigation rendered in " + watch.ElapsedMilliseconds + " milliseconds");
                    writer.WriteComment(string.Format("Items included: {0} structure groups, {1} pages and {2} Metadata Fields", _countStructureGroups, _countPages, _countFields));
                }

                writer.WriteEndDocument();

                writer.Flush();
                stream.Position = 0;
                package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Xml, Encoding.UTF8.GetString(stream.ToArray())));
                writer.Close();
            }
            watch.Stop();
            _log.Debug("Navigation created in " + watch.ElapsedMilliseconds + " milliseconds.");
            _log.Debug(string.Format("Navigation created for {0} structure groups, {1} pages and {2} Metadata Fields",
                                     _countStructureGroups, _countPages, _countFields));
        }

        /// <summary>
        /// Adds the metadata attributes from the page or structure group.
        /// </summary>
        /// <param name="fields">ItemFields collection containing the metadata fields & values.</param>
        /// <param name="writer">The XmlWriter.</param>
        /// <param name="prefix">The prefix to use - leave blank, it will be used when fields are embedded.</param>
        /// <remarks>This method will write values in the navigation XML output from the Page or StructureGroup metadata. Note that it will only include these values if Navigation.IncludePageMetadata or Navigation.IncludeStructureGroupMetadata are set to true. </remarks>
        private void AddMetadataAttributes(IEnumerable<ItemField> fields, XmlWriter writer, string prefix = null)
        {

            foreach (ItemField itemField in fields)
            {
                if (itemField is TextField || itemField is KeywordField)
                {
                    bool includeField = false;
                    string attributeName = null;
                    if (Navigation.FieldsToInclude.Count == 0) //  Include all fields
                    {
                        includeField = true;
                        attributeName = itemField.Name.ToLower();
                    }
                    else if (Navigation.FieldsToInclude.ContainsKey(itemField.Name))
                    {
                        includeField = true;
                        attributeName = Navigation.FieldsToInclude[itemField.Name];
                    }
                    else
                    {
                        _log.Debug("Not including field " + itemField.Name + " because it is not in the list of Navigation.FieldsToInclude");
                    }
                    if (includeField)
                    {
                        if (prefix != null)
                            attributeName = prefix + attributeName;
                        if (itemField is TextField)
                        {
                            TextField field = (TextField)itemField;
                            if (field.Values.Count > 0)
                                writer.WriteAttributeString(attributeName, field.Value);
                        }
                        if (itemField is KeywordField)
                        {
                            KeywordField field = (KeywordField)itemField;
                            if (field.Values.Count > 0)
                                writer.WriteAttributeString(attributeName, field.Value.Title);
                        }
                        _countFields++;
                    }
                }
                else if (itemField is EmbeddedSchemaField)
                {
                    EmbeddedSchemaField embedded = (EmbeddedSchemaField)itemField;
                    int count = 0;
                    foreach (ItemFields embeddedFields in embedded.Values)
                    {
                        AddMetadataAttributes(embeddedFields, writer, embedded.Name + count);
                        count++;
                    }
                }
                else
                {
                    _log.Debug("Not adding metadata for field " + itemField.Name + " since it is not a Text Field.");
                }
            }
        }

        /// <summary>
        /// Gets the approved version of the item being rendered for the current target.
        /// </summary>
        /// <param name="item">The item in question.</param>
        /// <param name="target">The target you're publishing to.</param>
        /// <returns>Either the same version or a later version if in workflow and its approval status is high enough for this target</returns>
        private VersionedItem GetApprovedVersionForTarget(VersionedItem item, PublicationTarget target)
        {
            bool isPreview = false;
            if (!item.LockType.HasFlag(LockType.InWorkflow))
                return item;
            if (target == null)
            {
                isPreview = true;
            }
            if (!isPreview && target.Id == TcmUri.UriNull)
            {
                isPreview = true;
            }
            if (item is Page)
            {
                Page pageInWorkflow =
                    (Page)_engine.GetObject(new TcmUri(item.Id.ItemId, item.Id.ItemType, item.Id.PublicationId, 0));
                if (isPreview) return pageInWorkflow;
                int targetApprovalStatus;
                if (target.MinApprovalStatus == null)
                    targetApprovalStatus = 0;
                else
                    targetApprovalStatus = target.MinApprovalStatus.Position;

                return pageInWorkflow.ApprovalStatus.Position >= targetApprovalStatus
                               ? pageInWorkflow
                               : item;
            }
            if (item is Component)
            {
                Component componentInWorkflow =
                   (Component)_engine.GetObject(new TcmUri(item.Id.ItemId, item.Id.ItemType, item.Id.PublicationId, 0));
                if (isPreview) return componentInWorkflow;
                return componentInWorkflow.ApprovalStatus.Position >= target.MinApprovalStatus.Position
                           ? componentInWorkflow
                           : item;
            }
            return item;
        }

        /// <summary>
        /// Processes (recursively) the navigation.
        /// </summary>
        /// <param name="writer">The XmlTextWriter to use.</param>
        /// <param name="item">The item.</param>
        private void ProcessNavigation(XmlTextWriter writer, RepositoryLocalObject item)
        {

            Match match = _regex.Match(item.Title);
            bool isStructureGroup = (item is StructureGroup);

            // Exit if the current item has no numbers in its title.
            if (Navigation.OnlyIncludeItemsWithNumbersInTitle && !match.Success) return;

            if (!isStructureGroup)
                item = GetApprovedVersionForTarget((Page)item, _engine.PublishingContext.PublicationTarget);

            // Get the correct item title, without numbers
            string itemTitle = Regex.Replace(item.Title, RegexPattern, string.Empty);

            // Get the correct item path
            string itemPath = isStructureGroup ? ((StructureGroup)item).PublishLocationUrl : ((Page)item).PublishLocationUrl;

            // Add / where it needs to be added
            if ((!itemPath.EndsWith("/")) && (isStructureGroup)) itemPath += "/";
            if (!itemPath.StartsWith("/")) itemPath = "/" + itemPath;

            writer.WriteStartElement(Navigation.NodeName);
            writer.WriteAttributeString(Navigation.TypeAttributeName, item.Id.ItemType.ToString());
            if (Navigation.IncludePathAttribute) writer.WriteAttributeString(Navigation.PathAttributeName, itemPath);
            writer.WriteAttributeString(Navigation.TitleAttributeName, itemTitle);
            if (Navigation.IncludeUriAttribute) writer.WriteAttributeString(Navigation.UriAttributeName, item.Id);

            if (isStructureGroup)
            {
                _countStructureGroups++;
                if (Navigation.IncludeStructureGroupMedata)
                {
                    if (item.Metadata != null)
                    {
                        ItemFields itemMetadata = new ItemFields(item.Metadata, item.MetadataSchema);
                        AddMetadataAttributes(itemMetadata, writer);
                    }
                }
                foreach (RepositoryLocalObject child in ((StructureGroup)item).GetItems())
                {
                    ProcessNavigation(writer, child);
                }
            }
            else
            {
                _countPages++;
                if (Navigation.IncludePageMetadata)
                {
                    if (item.Metadata != null)
                    {
                        ItemFields itemMetadata = new ItemFields(item.Metadata, item.MetadataSchema);
                        AddMetadataAttributes(itemMetadata, writer);
                    }
                }
            }
            writer.WriteEndElement();
        }
    }

    internal static class Navigation
    {
        // The following 6 strings control the XML Element and attribute names to use
        internal static string RootNodeName = "navigation";
        internal static string NodeName = "link";
        internal static string PathAttributeName = "path";
        internal static string TitleAttributeName = "title";
        internal static string TypeAttributeName = "type";
        internal static string UriAttributeName = "uri";

        // Modify the following booleans to include or exclude a given attribute
        // Name & Type are ALWAYS included
        internal static bool IncludePathAttribute = true;
        internal static bool IncludeUriAttribute = true;

        // Modify this value to include pages and structure groups without numbers in the navigation
        internal static bool OnlyIncludeItemsWithNumbersInTitle;

        // Modify this value to include metadata for the different Tridion objects
        internal static bool IncludePublicationMetadata = true;
        internal static bool IncludeStructureGroupMedata = true;
        internal static bool IncludePageMetadata = true;

        // Modify this value to log performance metrics
        internal static bool IncludePerformanceMetrics = true;

        private static Dictionary<string, string> _fieldsToInclude = new Dictionary<string, string>();

        // Text fields will be output as fieldname="fieldvalue" attributes on the specified node
        internal static Dictionary<string, string> FieldsToInclude
        {
            get
            {
                if (_fieldsToInclude.Count == 0)
                {
                    _fieldsToInclude = new Dictionary<string, string>();
                    // Add fields by adding them as follows:
                    // _fieldsToInclude.Add("fieldName", "outputAttributeName");
                    // Leave outputAttributeName as null or string.Empty to keep the same name
                    // Leave this dictionary empty to include all fields.
                }
                return _fieldsToInclude;
            }
        }
    }
}
