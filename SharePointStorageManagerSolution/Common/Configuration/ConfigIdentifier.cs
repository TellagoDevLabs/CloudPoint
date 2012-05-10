using System;
using System.Collections.Generic;
using System.Text;

namespace SPSM.Common.Configuration
{
    /// <summary>
    /// Represents a config item in the config store list.
    /// </summary>
    public class ConfigIdentifier
    {
        private string m_sCategory = null;
        private string m_sKey = null;

        /// <summary>
        /// Category of the config item.
        /// </summary>
        public string Category
        {
            get { return m_sCategory; }
            set { m_sCategory = value; }
        }
        
        /// <summary>
        /// Key (name) of the config item.
        /// </summary>
        public string Key
        {
            get { return m_sKey; }
            set { m_sKey = value; }
        }

        public ConfigIdentifier(string Category, string Key)
        {
            m_sCategory = Category;
            m_sKey = Key;
        }
    }
}
