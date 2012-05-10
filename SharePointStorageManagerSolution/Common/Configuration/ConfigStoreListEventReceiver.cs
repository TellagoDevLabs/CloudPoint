using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Caching;

using Microsoft.SharePoint;
using System.IO;
using System.Diagnostics;
using SPSM.Common.Logging;
using Microsoft.SharePoint.Administration;

namespace SPSM.Common.Configuration
{
    /// <summary>
    /// This list event receiver manages the memory cache used by the Config Store. In the current implementation 
    /// it is essential that the Config Store list is configured with this event receiver, otherwise stale 
    /// values will be retrieved.
    /// </summary>
    public class ConfigStoreListEventReceiver : SPItemEventReceiver
    {
        private Logger logger = new Logger();

        public override void ItemAdded(SPItemEventProperties properties)
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStoreListEventReceiver-ItemAdded(): Entered - item will be added to cache.");

            // here there is no need to invalidate the existing items in the cache, but we can proactively add this item 
            // prior to it's first request..
            addItemToCache(properties);

            base.ItemAdded(properties);
        }

        public override void ItemDeleted(SPItemEventProperties properties)
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStoreListEventReceiver-ItemDeleted(): Entered - item will be deleted and cache flushed.");

            // here we need to flush the cache across all app pools..
            flushCache();
            
            base.ItemDeleted(properties);
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStoreListEventReceiver-ItemUpdated(): Entered - cache will be flushed and item added to cache.");

            // here we need to flush the cache across all app pools..
            flushCache();

            // .. but whilst we're here let's also add the updated item to the current app pool's cache..
            addItemToCache(properties);

            base.ItemUpdated(properties);
        }

        private static void addItemToCache(SPItemEventProperties properties)
        {
            string sCategory = properties.ListItem[ConfigStore.CategoryField] as string;
            string sKey = properties.ListItem[ConfigStore.KeyField] as string;
            string sValue = properties.ListItem[ConfigStore.ValueField] as string;

            string sCacheKey = ConfigStore.GetCacheKey(sCategory, sKey);

            ConfigStore.CacheConfigStoreItem(sValue, sCacheKey);
        }

        private void flushCache()
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    File.WriteAllText(ConfigStore.CacheDependencyFilePath, DateTime.Now.ToString());
                });
                logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStoreListEventReceiver-flushCache(): Successfully flushed Config Store cache - wrote " +
                    "to cache dependency file at '{0}'.", ConfigStore.CacheDependencyFilePath);
            }
            catch (Exception e)
            {
                // many IO exception types can occur here, so we catch general exception..
                logger.LogToOperations(e,SPSMCategories.ConfigStore, EventSeverity.Warning, "ConfigStoreListEventReceiver-flushCache(): Failed to flush Config Store cache - unable " +
                    "to write to cache dependency file at '{0}'. Config Store may return stale values!", ConfigStore.CacheDependencyFilePath);
            }
        }
    }
}
