using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Security;
using Tridion.ContentManager.Templating;

namespace TridionTemplates
{

    /// <summary>
    /// Util function class. Contains a collection of static functions.
    /// </summary>
    public class TemplateUtils
    {

        public const string ImagesFolderName = "assets";
        protected static TemplatingLogger log = TemplatingLogger.GetLogger(typeof(TemplateUtils));

        private TemplateUtils() { }


        
        /// <summary>
        /// Get 'current' PublishTransaction. It tries to identify a PublishTransaction from the publish queue that is on the 
        /// given TcmUri, Publication, User, etc.
        /// </summary>
        /// <param name="engine">Engine object</param>
        /// <param name="tcmUri">String representing the tcmuri of the item to check</param>
        /// <returns>PublishTransaction if found; or null, otherwise</returns>
        public static PublishTransaction GetPublishTransaction(Engine engine, String tcmUri)
        {
            String binaryPath = engine.PublishingContext.PublishInstruction.RenderInstruction.BinaryStoragePath;
            Regex tcmRegex = new Regex(@"tcm_\d+-\d+-66560");
            Match match = tcmRegex.Match(binaryPath);

            if (match.Success)
            {
                String transactionId = match.Value.Replace('_', ':');
                TcmUri transactionUri = new TcmUri(transactionId);
                return new PublishTransaction(transactionUri, engine.GetSession());
            }
            else
            {
                return FindPublishTransaction(engine, tcmUri);
            }
        }

        /// <summary>
        /// Get 'current' PublishTransaction. It tries to identify a PublishTransaction from the publish queue that is on the 
        /// given TcmUri, Publication, User, etc.
        /// </summary>
        /// <param name="engine">Engine object</param>
        /// <param name="tcmUri">String representing the tcmuri of the item to check</param>
        /// <returns>PublishTransaction if found; or null, otherwise</returns>
        private static PublishTransaction FindPublishTransaction(Engine engine, String tcmUri)
        {
            log.Debug(String.Format("Find PublishTransaction for item '{0}'", tcmUri));

            PublishTransaction result = null;
            Session session = engine.GetSession();
            PublishTransactionsFilter filter = new PublishTransactionsFilter(session);

            filter.PublishTransactionState = PublishTransactionState.Resolving;
            RepositoryLocalObject item = engine.GetObject(tcmUri) as RepositoryLocalObject;
            filter.ForRepository = item.ContextRepository;

            PublicationTarget publicationTarget = engine.PublishingContext.PublicationTarget;
            if (publicationTarget != null)
            {
                filter.PublicationTarget = publicationTarget;
            }

            XmlElement element = PublishEngine.GetListPublishTransactions(filter);


            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("tcm", "http://www.tridion.com/ContentManager/5.0");

            String xPath = String.Format("tcm:ListPublishTransactions/tcm:Item[@ItemID='{0}']", tcmUri);
            XmlNodeList nodeList = element.SelectNodes(xPath, namespaceManager);

            String transactionId = null;
            if (nodeList.Count == 1)
            {
                transactionId = nodeList[0].Attributes["ID"].Value;
                TcmUri transactionUri = new TcmUri(transactionId);
                result = new PublishTransaction(transactionUri, session);
            }
            else
            {
                foreach (XmlNode node in element.ChildNodes)
                {
                    transactionId = node.Attributes["ID"].Value;
                    TcmUri transactionUri = new TcmUri(transactionId);
                    result = new PublishTransaction(transactionUri, session);
                    if (IsPublishTransactionForTcmUri(result, tcmUri))
                    {
                        break;
                    }
                    result = null;
                }
            }

            log.Debug("Returning PublishTransaction " + result);
            return result;
        }

        /// <summary>
        /// Try to identify if the given publish result contains the TcmUri to check
        /// </summary>
        /// <param name="result">PublishTransaction to use</param>
        /// <param name="tcmUri">String Tcm Uri to check</param>
        /// <returns>true if found; false, otherwise</returns>
        private static bool IsPublishTransactionForTcmUri(PublishTransaction transaction, String tcmUri)
        {
            IList<IdentifiableObject> items = transaction.Items;
            foreach (IdentifiableObject item in items)
            {
                if (item.Id.ToString().Equals(tcmUri))
                {
                    return true;
                }
            }

            foreach (PublishContext context in transaction.PublishContexts)
            {
                foreach (ProcessedItem processedItem in transaction.GetListProcessedItems(context))
                {
                    IdentifiableObject item = processedItem.ResolvedItem.Item;
                    if (item.Id.ToString().Equals(tcmUri))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check the Publishing queue and determine whether the given TcmUri is already present in the queue.
        /// </summary>
        /// <param name="engine">Engine object</param>
        /// <param name="tcmUri">String representing the tcmuri of the item to check</param>
        /// <param name="state">PublishTransactionState the publish state to filter on</param>
        public static bool IsInPublishingQueue(Engine engine, String tcmUri, PublishTransactionState state)
        {
            log.Debug(String.Format("Check Publishing queue for item '{0}'", tcmUri));

            Session session = engine.GetSession();
            PublishTransactionsFilter filter = new PublishTransactionsFilter(session);

            filter.PublishTransactionState = state;
            RepositoryLocalObject item = engine.GetObject(tcmUri) as RepositoryLocalObject;
            filter.ForRepository = item.ContextRepository;

            PublicationTarget publicationTarget = engine.PublishingContext.PublicationTarget;
            if (publicationTarget != null)
            {
                filter.PublicationTarget = publicationTarget;
            }

            XmlElement element = PublishEngine.GetListPublishTransactions(filter);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("tcm", "http://www.tridion.com/ContentManager/5.0");

            String xPath = String.Format("tcm:ListPublishTransactions/tcm:Item[@ItemID='{0}']", tcmUri);
            XmlNodeList nodeList = element.SelectNodes(xPath, namespaceManager);

            return nodeList.Count > 0;
        }
    }
}
