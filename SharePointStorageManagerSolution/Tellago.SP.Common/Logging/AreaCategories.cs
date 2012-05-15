using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellago.SP.Logging
{    
    public enum Areas
    {
        /// <summary>
        /// Your project logging Area. Change the name accordingly.
        /// </summary>
        TellagoDevLabs
    }

    /// <summary>
    /// Your project logging categories. Change and extend as required.
    /// </summary>
    public enum LogCategories
    {
        Default,
        Media,
        Configuration
    }

    public static class CustomCategoriesExtensions
    {
        public static string ToLoggerString(this LogCategories category)
        {
            return String.Format("{0}/{1}", Areas.TellagoDevLabs.ToString(), category.ToString());
        }
    }
    
}
