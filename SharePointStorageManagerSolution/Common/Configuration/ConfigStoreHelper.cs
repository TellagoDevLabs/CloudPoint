using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using SPSM.Common.Logging;

namespace SPSM.Common.Configuration
{
    /// <summary>
    /// Provides helper methods related to use of Config Store.
    /// </summary>
    public class ConfigStoreHelper
    {
        #region -- Constructor (private) --

        private ConfigStoreHelper() 
        {
        }

        #endregion

        /// <summary>
        /// Reads a value from a dictionary of config values retrieved by ConfigStore.GetValues(). This method ensures a meaningful 
        /// exception is thrown is a config value is not present so the missing value can be easily identified. 
        /// </summary>
        /// <param name="ConfigItems">Dictionary of config values.</param>
        /// <param name="ConfigID">ConfigIdentifier for the value to retrieve from dictionary.</param>
        /// <param name="MissingValueMessage">Optional - the message to include in exception thrown when no config value is retrieved, 
        /// e.g. 'This config key should specify the ....'.</param>
        /// <returns>String config value.</returns>
        public static string ReadDictionaryValue(Dictionary<ConfigIdentifier, string> ConfigItems, ConfigIdentifier ConfigID)
        {
            string sValue = null;
            try
            {
                sValue = ConfigItems[ConfigID];
            }
            catch (KeyNotFoundException)
            {
                // throw more meaningful exception and trace..
                string sError = string.Format("No value found in Config Store for key {0}.{1}. {2}",
                    ConfigID.Category, ConfigID.Key);
                throw new ArgumentException(sError,"ConfigID");
            }

            return sValue;
        }

    }
}