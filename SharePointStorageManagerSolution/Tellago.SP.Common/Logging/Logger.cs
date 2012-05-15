using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.SharePoint.Common.Logging;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.SharePoint.Administration;

namespace Tellago.SP.Logging
{
    public class Logger
    {
        private ILogger logger = SharePointServiceLocator.GetCurrent().GetInstance<ILogger>();

        public void LogToOperations(LogCategories category, EventSeverity severity, string message, params object[] args)
        {
            try
            {
                logger.LogToOperations(String.Format(message, args), GetEventId(category),
                     severity, category.ToLoggerString());
            }
            catch 
            { 
                //don't want the app to fail because of failures in logging 
            }
        }

        public void LogToOperations(Exception ex, LogCategories category, EventSeverity severity, string message, params object[] args)
        {
            try
            {
                logger.LogToOperations(ex, String.Format(message, args), GetEventId(category),
                     severity, category.ToLoggerString());
            }
            catch 
            {
                //don't want the app to fail because of failures in logging 
            }
        }

        private int GetEventId(LogCategories category)
        {
            return 5000 + (int)category;
        }
    }
}
