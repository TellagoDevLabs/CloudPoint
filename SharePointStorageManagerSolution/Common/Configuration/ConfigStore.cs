using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Caching;

using Microsoft.SharePoint;
using SPSM.Common.Logging;
using Microsoft.SharePoint.Administration;

namespace SPSM.Common.Configuration
{
    /// <summary>
    /// Utility class for retrieving configuration values within a SharePoint site. Where multiple values need to 
    /// be retrieved, the GetMultipleValues() method should be used where possible.
    /// 
    /// Created by Chris O'Brien (www.sharepointnutsandbolts.com).
    /// </summary>
    public class ConfigStore
    {
        #region -- Private fields --

        private static AppSettingsReader m_Reader = new AppSettingsReader();

        private static string m_GlobalConfigSiteAppSettingsKey = "ConfigSiteUrl";
        private static string m_GlobalConfigWebAppSettingsKey = "ConfigWebName";
        private static string m_GlobalConfigListAppSettingsKey = "ConfigListName";
        private static string m_CacheFilePathKey = "ConfigStoreCacheDependencyFile";

        private static readonly string m_DefaultListName = "SPSM-ConfigStore";

        private static Logger logger = new Logger();

        #endregion

        #region -- Constructor (private)

        private ConfigStore()
        {
        }

        #endregion

        #region -- Public fields --

        public static readonly string CategoryField = "ConfigCategory";
        public static readonly string KeyField = "Title";
        public static readonly string ValueField = "ConfigValue";
     //   public static string CacheDependencyFilePath = (string)m_Reader.GetValue(m_CacheFilePathKey, typeof(string));

        public enum ConfigStoreType
        {
            Local,
            Global
        }

        #endregion

        /// <summary>
        /// Retrieves a single value from the config store list.
        /// </summary>
        /// <param name="category">Category of the item to retrieve.</param>
        /// <param name="key">Key (item name) of the item to retrieve.</param>
        /// <returns>The config item's value.</returns>
        public static string GetValue(string category, string key)
        {
            return GetValue(category, key, true);
        }

        public static bool TryGetValue(string category, string key, bool useCache, out string value)
        {
            if (string.IsNullOrEmpty(category))
                throw new ArgumentException("invalid category");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("invalid key");

            try
            {
                value = GetValue(category, key, useCache);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Retrieves a single value from the config store list.
        /// </summary>
        /// <param name="category">Category of the item to retrieve.</param>
        /// <param name="key">Key (item name) of the item to retrieve.</param>
        /// <returns>The config item's value.</returns>
        public static string GetValue(string category, string key, bool useCache)
        {
            // first let's trim the supplied values..
            category = category.Trim();
            key = key.Trim();

            bool bListFoundFromSPContext = true;
            string sValue = null;

            // attempt retrieval from cache..
            HttpContext httpCtxt = HttpContext.Current;
            SPContext spCtxt = SPContext.Current;

            string sCachedValue = null;
            string sCacheKey = GetCacheKey(category, key);

            if (httpCtxt != null && useCache)
            {
                // first check memory cache for current site's Config Store..
                sCachedValue = httpCtxt.Cache[sCacheKey] as string;

                if (!string.IsNullOrEmpty(sCachedValue))
                {
                    return sCachedValue;
                }
                else
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): Value not found in memory cache, query will be executed.");
                }
            }

            // no value found, proceed with query..
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPQuery query = getSingleItemQuery(category, key);

                SPList configStoreList = null;
                try
                {
                    configStoreList = attemptGetLocalConfigStoreListFromContext();
                    if (configStoreList != null)
                    {
                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): Current site collection has local Config Store, will " +
                            "first attempt query against this list - '{0}'.", configStoreList.DefaultViewUrl);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): No local config store list found in current site collection.");
                }

                if (configStoreList == null)
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): Current site collection does not have local Config Store, falling " +
                        "back to global Config Store.");
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): Attempting to get config store from config.");
                    bListFoundFromSPContext = false;
                    configStoreList = TryGetGlobalConfigStoreListFromConfig();
                }

                // will have list or have thrown exception by now..               
                try
                {
                    sValue = executeSingleItemQuery(category, key, configStoreList, query, false);
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): Retrieved value '{0}' from Config Store '{1}' for " +
                        "category '{2}' and key '{3}'.", sValue, configStoreList.DefaultViewUrl, category, key);
                }
                finally
                {
                    if (!bListFoundFromSPContext)
                    {
                        // disposals are required.. 
                        configStoreList.ParentWeb.Site.Dispose();
                        configStoreList.ParentWeb.Dispose();
                    }
                    else
                    {
                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): No disposals required, list found from SPContext.");
                    }
                }

                if (string.IsNullOrEmpty(sValue))
                {
                    // now try query against global Config Store..
                    if (bListFoundFromSPContext && !IsGlobalConfigStore(configStoreList))
                    {
                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Warning, "ConfigStore-GetValue(): No value found in local Config Store, now looking " +
                            "up config item for category '{0}' and key '{1}' in global Config Store.",
                            category, key);

                        configStoreList = TryGetGlobalConfigStoreListFromConfig();
                        bListFoundFromSPContext = false;
                        sValue = executeSingleItemQuery(category, key, configStoreList, query, true);
                    }
                }

                // finally if we found a value let's cache it..
                if (!string.IsNullOrEmpty(sValue))
                {
                    // add to cache..
                    sCacheKey = GetCacheKey(category, key);
                    ConfigStore.CacheConfigStoreItem(sValue, sCacheKey);
                }
                else
                {
                    string sMessage = string.Format("No config item was found for category '{0}' and key '{1}'.", category, key);
                    throw new InvalidConfigurationException(sMessage);
                }
            });

            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): Returning '{0}'.", sValue);

            return sValue;
        }

        private static string getSiteCacheKey(SPContext context)
        {
            string sSiteCacheKey = null;
            if (context != null)
            {
                sSiteCacheKey = context.Site.Url;
            }
            return sSiteCacheKey;
        }

        private static SPQuery getSingleItemQuery(string Category, string Key)
        {
            SPQuery query = new SPQuery();
            query.Query = string.Format("<Where><And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"Text\">{1}</Value></Eq><Eq><FieldRef Name=\"{2}\" /><Value Type=\"Text\">{3}</Value></Eq></And></Where>",
                 ConfigStore.CategoryField, Category, ConfigStore.KeyField, Key);
            query.ViewFields = string.Format("<FieldRef Name=\"{0}\" />", ConfigStore.ValueField);
            return query;
        }

        private static string executeSingleItemQuery(string Category, string Key, SPList configStoreList, SPQuery query, bool bThrowOnNoResults)
        {
            string sValue = null;
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-executeSingleItemQuery(): Executing query '{0}'.", query.Query);
            SPListItemCollection items = configStoreList.GetItems(query);

            if (items.Count == 1)
            {
                sValue = items[0][0].ToString();
                logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-executeSingleItemQuery(): Found '{0}' as config value for Category '{1}' " +
                   "and Key '{2}'.", sValue, Category, Key);
            }
            else if (items.Count > 1)
            {
                string sMessage =
                    string.Format("Multiple config items were found for the requested item. Please check " +
                                  "config store settings list for category '{0}' and key '{1}'.", Category, Key);
                throw new InvalidConfigurationException(sMessage);
            }
            else if (items.Count == 0)
            {
                string sMessage = string.Format("No config item was found for category '{0}' and key '{1}'.", Category, Key);
                if (bThrowOnNoResults)
                {
                    throw new InvalidConfigurationException(sMessage);
                }
                else
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Warning, "ConfigStore-executeSingleItemQuery(): No config value found for category '{0}' " +
                        "and key '{1}'. Returning null, but not throwing exception since caller specified not to.",
                        Category, Key);
                }
            }

            return sValue;
        }

        #region -- GetMultipleValues() --

        /// <summary>
        /// Retrieves multiple config values with a single query. 
        /// </summary>
        /// <param name="ConfigIdentifiers">List of ConfigIdentifier objects to retrieve.</param>
        /// <returns>A Dictionary object containing the requested config values. Items are keyed by ConfigIdentifier.</returns>
        public static Dictionary<ConfigIdentifier, string> GetMultipleValues(List<ConfigIdentifier> ConfigIdentifiers)
        {
            // first let's trim the supplied values..
            trimDictionaryEntries(ConfigIdentifiers);

            Dictionary<ConfigIdentifier, string> configDictionary = new Dictionary<ConfigIdentifier, string>();

            // attempt retrieval from cache..
            HttpContext httpCtxt = HttpContext.Current;
            SPContext spCtxt = SPContext.Current;

            if (httpCtxt != null)
            {
                logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Have HttpContext, checking memory cache for config values.");
                string sCacheKey = null;
                string sCachedValue = null;
                bool bFoundAllValuesInCache = true;

                foreach (ConfigIdentifier configID in ConfigIdentifiers)
                {
                    sCacheKey = GetCacheKey(configID.Category, configID.Key);
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Checking in memory for config value with Category '{0}' and " +
                        "Key '{1}'. Cache key '{2}' will be used.", configID.Category, configID.Key, sCacheKey);
                    sCachedValue = httpCtxt.Cache[sCacheKey] as string;

                    if (sCachedValue != null)
                    {
                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Found value '{0}' in memory for cache key '{1}'. " +
                            "Adding to dictionary.", sCachedValue, sCacheKey);
                        configDictionary.Add(configID, sCachedValue);
                    }
                    else
                    {
                        //        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Did not find value for cache key '{0}' in memory " +
                        //            "Query will be executed, not checking memory for any further values.", sCacheKey);
                        bFoundAllValuesInCache = false;
                        break;
                    }
                }

                if (bFoundAllValuesInCache)
                {
                    //        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Found all values in memory, returning dictionary.");
                    return configDictionary;
                }
                else
                {
                    //      logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Clearing dictionary prior to query.");
                    // clear the dictionary since we'll add fresh config items to it from our query..
                    configDictionary.Clear();
                }
            }

            // no value found, proceed with query..
            if (ConfigIdentifiers.Count < 2)
            {
                string sMessage = "Invalid use of config store - the GetMultipleValues() method " +
                                  "must only be used to retrieve multiple config values.";
                throw new InvalidConfigurationException(sMessage);
            }

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                bool bListFoundFromSPContext = true;
                SPList configStoreList = null;
                try
                {
                    configStoreList = attemptGetLocalConfigStoreListFromContext();
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    logger.LogToOperations(ex, SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Failed to get config store list from current context.");
                }

                if (configStoreList == null)
                {
                    //  logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Attempting to get config store from config.");
                    bListFoundFromSPContext = false;
                    configStoreList = TryGetGlobalConfigStoreListFromConfig();
                }

                if (configStoreList != null)
                {
                    try
                    {
                        SPListItemCollection items = null;
                        Int32 configIdCounter = 0;
                        StringBuilder sbQuery = new StringBuilder();
                        StringBuilder sbQueryStart = new StringBuilder();
                        sbQuery.Append("<Where>");

                        if (ConfigIdentifiers.Count > 1)
                        {
                            for (int iOrCount = 0; iOrCount < ConfigIdentifiers.Count - 1; iOrCount++)
                            {
                                sbQuery.Append("<Or>");
                            }
                        }

                        foreach (ConfigIdentifier configID in ConfigIdentifiers)
                        {
                            sbQuery.AppendFormat("<And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"Text\">{1}</Value></Eq>" +
                                "<Eq><FieldRef Name=\"{2}\" /><Value Type=\"Text\">{3}</Value></Eq></And>",
                                 ConfigStore.CategoryField, configID.Category, ConfigStore.KeyField, configID.Key);

                            if (ConfigIdentifiers.Count > 1 && configIdCounter > 0)
                            {
                                sbQuery.Append("</Or>");
                            }

                            configIdCounter++;
                        }

                        sbQuery.Append("</Where>");

                        SPQuery query = new SPQuery();
                        query.Query = sbQuery.ToString();
                        query.ViewFields = string.Format("<FieldRef Name=\"{0}\" /><FieldRef Name=\"{1}\" /><FieldRef Name=\"{2}\" />",
                            ConfigStore.CategoryField, ConfigStore.KeyField, ConfigStore.ValueField);

                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Executing query '{0}'.", query.Query);
                        items = configStoreList.GetItems(query);
                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Query returned '{0}' items.",
                            items.Count);

                        string sCategory = string.Empty;
                        string sKey = string.Empty;
                        string sConfigValue = string.Empty;
                        string sCacheKey = string.Empty;

                        foreach (SPListItem item in items)
                        {
                            foreach (ConfigIdentifier configID in ConfigIdentifiers)
                            {
                                if ((item[ConfigStore.CategoryField].ToString() == configID.Category) && (item[ConfigStore.KeyField].ToString() == configID.Key))
                                {
                                    sCategory = item[ConfigStore.CategoryField].ToString();
                                    sKey = item[ConfigStore.KeyField].ToString();
                                    sConfigValue = item[ConfigStore.ValueField].ToString();

                                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Retrieved config value '{0}' for " +
                                        "Category '{1}' and Key '{2}'.", sConfigValue, sCategory, sKey);

                                    if (!configDictionary.ContainsKey(configID))
                                    {
                                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Adding to dictionary.");
                                        configDictionary.Add(configID, sConfigValue);

                                        // also add to cache..
                                        if (httpCtxt != null)
                                        {
                                            sCacheKey = GetCacheKey(configID.Category, configID.Key);
                                            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetMultipleValues(): Have HttpContext " +
                                                "adding config value '{0}' to cache with key '{1}'.", sConfigValue, sCacheKey);
                                            httpCtxt.Cache.Insert(sCacheKey, sConfigValue, null, DateTime.MaxValue, Cache.NoSlidingExpiration);
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        string sMessage =
                                            string.Format(
                                                "Multiple config items were found for the requested item. Please check " +
                                                "config store settings list for category '{0}' and key '{1}'.",
                                                configID.Category, configID.Key);
                                        throw new InvalidConfigurationException(sMessage);
                                    }
                                }
                            }
                        }
                    }

                    finally
                    {
                        if (!bListFoundFromSPContext)
                        {
                            // disposals are required.. 
                            configStoreList.ParentWeb.Site.Dispose();
                            configStoreList.ParentWeb.Dispose();
                        }
                        else
                        {
                        }
                    }
                }
            });
            return configDictionary;
        }

        #endregion

        public static string GetCacheKey(string Category, string Key)
        {
            return string.Format("{0}|{1}", Category, Key);
        }

        public static bool IsGlobalConfigStore(SPList ConfigStoreList)
        {
            string sGlobalConfigStoreSite = (string)m_Reader.GetValue(m_GlobalConfigSiteAppSettingsKey, typeof(string));
            string sGlobalConfigStoreWeb = (string)m_Reader.GetValue(m_GlobalConfigWebAppSettingsKey, typeof(string));
            string sGlobalConfigStoreListName = (string)m_Reader.GetValue(m_GlobalConfigListAppSettingsKey, typeof(string));

            if (string.Compare(ConfigStoreList.ParentWeb.Site.Url, sGlobalConfigStoreSite, true) == 0 &&
                string.Compare(ConfigStoreList.ParentWeb.Name, sGlobalConfigStoreWeb, true) == 0 &&
                string.Compare(ConfigStoreList.Title, sGlobalConfigStoreListName, true) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region -- Private helper methods --

        private static void trimDictionaryEntries(List<ConfigIdentifier> ConfigIdentifiers)
        {
            foreach (ConfigIdentifier configId in ConfigIdentifiers)
            {
                configId.Category = configId.Category.Trim();
                configId.Key = configId.Key.Trim();
            }
        }

        private static SPList attemptGetLocalConfigStoreListFromContext()
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreListFromContext(): Entered.");

            SPList configList = null;
            SPContext currentContext = SPContext.Current;

            if (currentContext != null)
            {
                logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreListFromContext(): We have SPContext, " +
                     "will first search for Config Store in root web of current site '{0}'.", currentContext.Site.Url);

                SPSite currentSite = currentContext.Site;
                configList = TryGetConfigStoreList(currentSite.RootWeb, ConfigStoreType.Local, false);

                if (configList != null)
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreListFromContext(): Successfully found config list " +
                        "with name '{0}', returning.", configList.Title);
                }
                else
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Warning, "ConfigStore-attemptGetConfigStoreListFromContext(): No config list found, " +
                        "returning null.");
                }
            }

            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreListFromContext(): Leaving.");
            return configList;
        }

        private static SPList TryGetGlobalConfigStoreListFromConfig()
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreListFromConfig(): Entered.");

            SPList configStoreList = null;

            // ensure we throw exceptions if the *global* config store cannot be found since this is the last resort..
            bool bThrowOnNotFound = true;

            using (SPSite configSite = GetGlobalConfigSiteFromConfiguredUrl())
            {
                using (SPWeb configStoreWeb = attemptGetConfigStoreWeb(configSite, ConfigStoreType.Global, bThrowOnNotFound))
                {
                    configStoreList = TryGetConfigStoreList(configStoreWeb, ConfigStoreType.Global, bThrowOnNotFound);

                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreListFromConfig(): Leaving.");

                    return configStoreList;
                }
            }
        }

        private static SPList TryGetConfigStoreList(SPWeb configStoreWeb, ConfigStoreType configStoreType, bool bThrowOnNotFound)
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreList(): Entered with SPWeb named '{0}', " +
                "configStoreType '{1}' and 'throw exception on not found' param of '{2}'.", configStoreWeb.Title, configStoreType, bThrowOnNotFound);

            SPList configStoreList = null;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string sListName = null;

                if (configStoreType == ConfigStoreType.Global)
                {
                    string sOverrideList = getAppSettingsValue(m_GlobalConfigListAppSettingsKey);
                    if (string.IsNullOrEmpty(sOverrideList))
                    {
                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreList(): Found override list name '{0}' " +
                            "specified in config, will attempt to find Config Store list with this name.", sOverrideList);
                        sListName = sOverrideList;
                    }
                    else
                    {
                        logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreList(): No override list name found in config, " +
                            "will attempt to find Config Store list with default list name '{0}'.", m_DefaultListName);
                        sListName = m_DefaultListName;
                    }
                }
                else
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreList(): Will attempt to find local Config " +
                        "Store list with default list name '{0}'.", m_DefaultListName);
                    sListName = m_DefaultListName;
                }

                try
                {
                    configStoreList = configStoreWeb.Lists[sListName];
                }
                catch (ArgumentException argExc)
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Error, "ConfigStore-attemptGetConfigStoreList(): Failed to find list named '{0}' " +
                        "in web '{1}'!", sListName, configStoreWeb.Title);
                    if (bThrowOnNotFound)
                    {
                        string sMessage = string.Format("Unable to find configuration list with name '{0}'.", sListName);
                        throw new InvalidConfigurationException(sMessage, argExc);
                    }
                }
            });


            return configStoreList;
        }

        private static SPWeb attemptGetConfigStoreWeb(SPSite configSite, ConfigStoreType configStoreType, bool bThrowOnNotFound)
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreWeb(): Entered with SPSite named '{0}' " +
                ", configStoreType '{1}' and 'throw exception on not found' param of '{2}'.", configSite, configStoreType, bThrowOnNotFound);

            // if we're looking for the global Config Store do we have an override web name specified in config? 

            bool bUseRootWeb = false;
            string sOverrideWeb = null;

            if (configStoreType == ConfigStoreType.Global)
            {
                sOverrideWeb = getAppSettingsValue(m_GlobalConfigWebAppSettingsKey);

                // if so, find web with this name. If not, default to root web..
                if (!string.IsNullOrEmpty(sOverrideWeb))
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreWeb(): Found override web name '{0}' " +
                            "specified in config, will attempt to find Config Store web with this name.", sOverrideWeb);
                }
                else
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreWeb(): No override web name found in config, " +
                        "will use root web of site '{0}'.", configSite.RootWeb.Url);
                    bUseRootWeb = true;
                }
            }
            else
            {
                logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreWeb(): Will attempt to find local Config " +
                    "Store list in root web of site '{0}'.", configSite.RootWeb.Url);
                bUseRootWeb = true;
            }

            SPWeb configStoreWeb = null;
            if (bUseRootWeb)
            {
                configStoreWeb = configSite.RootWeb;
            }
            else
            {
                try
                {
                    configStoreWeb = configSite.AllWebs[sOverrideWeb];
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreWeb(): Successfully found web with name '{0}'.",
                        configStoreWeb.Name);
                }
                catch (ArgumentException argExc)
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Warning, "ConfigStore-attemptGetConfigStoreWeb(): Failed to find web named '{0}' " +
                        "in site '{1}'!", sOverrideWeb, configSite.Url);
                    if (bThrowOnNotFound)
                    {
                        string sMessage =
                            string.Format(
                                "Unable to find configuration web in current site collection with name '{0}'.",
                                sOverrideWeb);
                        throw new InvalidConfigurationException(sMessage, argExc);
                    }
                }
            }

            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-attemptGetConfigStoreWeb(): Leaving.");
            return configStoreWeb;
        }

        private static SPSite GetGlobalConfigSiteFromConfiguredUrl()
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-getConfigSiteFromConfiguredUrl(): Entered.");

            SPSite configSite = null;
            string sOverrideConfigSiteUrl = getAppSettingsValue(m_GlobalConfigSiteAppSettingsKey);

            if (sOverrideConfigSiteUrl == null)
            {
                string sMessage =
                    string.Format("The Config Store is not properly configured in web.config. Alternatively, you are using the Config Store where no SPContext is present " +
                                  "and the host process does not have a '**.exe.config' file which contains Config Store configuration. The config file must contain " +
                                  "an appSettings key named '{0}' which contains the URL of the parent site collection for the Config Store list.",
                                  m_GlobalConfigSiteAppSettingsKey);
                throw new InvalidConfigurationException(sMessage);
            }
            else
            {
                if (sOverrideConfigSiteUrl.Length == 0)
                {
                    string sMessage = "An override URL for the config site was specified but it was invalid. " +
                                      "Please check your configuration.";
                    throw new InvalidConfigurationException(sMessage);
                }

                try
                {
                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-getConfigSiteFromConfiguredUrl(): Attempting to create SPSite " +
                        "with URL '{0}'.", sOverrideConfigSiteUrl);

                    configSite = new SPSite(sOverrideConfigSiteUrl);

                    logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-getConfigSiteFromConfiguredUrl(): Successfully created SPSite, returning.");
                }
                catch (FileNotFoundException e)
                {
                    string sMessage = string.Format("Unable to contact site '{0}' specified in appSettings/{1} as " +
                                                    "URL for Config Store site. Please check the URL.",
                        sOverrideConfigSiteUrl, m_GlobalConfigSiteAppSettingsKey);
                    throw new InvalidConfigurationException(sMessage, e);
                }
            }

            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-getConfigSiteFromConfiguredUrl(): Leaving.");

            return configSite;
        }

        private static string getAppSettingsValue(string sKey)
        {
            string sValue = null;

            try
            {
                sValue = m_Reader.GetValue(sKey, typeof(string)) as string;
            }
            catch (InvalidOperationException)
            {
            }

            return sValue;
        }

        #endregion

        internal static void CacheConfigStoreItem(string Value, string CacheKey)
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStore-GetValue(): Adding item to memory cache with key '{0}' " +
                        "and value '{1}'.", CacheKey, Value);
            CacheDependency fileDep = null;// new CacheDependency(CacheDependencyFilePath);
            HttpRuntime.Cache.Insert(CacheKey, Value, fileDep, DateTime.Now.AddMinutes(2), Cache.NoSlidingExpiration);
        }
    }
}
