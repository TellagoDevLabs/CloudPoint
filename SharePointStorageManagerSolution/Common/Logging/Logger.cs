﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.SharePoint.Common.Logging;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.SharePoint.Administration;

namespace SPSM.Common.Logging
{
    public class Logger
    {
        private ILogger logger = SharePointServiceLocator.GetCurrent().GetInstance<ILogger>();

        public void LogToOperations(SPSMCategories category, EventSeverity severity, string message, params object[] args)
        {
            try
            {
                logger.LogToOperations(String.Format(message, args), GetEventId(category),
                     severity, category.ToLoggerString());
            }
            catch { }
        }

        public void LogToOperations(Exception ex, SPSMCategories category, EventSeverity severity, string message, params object[] args)
        {
            try
            {
                logger.LogToOperations(ex, String.Format(message, args), GetEventId(category),
                     severity, category.ToLoggerString());
            }
            catch { }
        }

        private int GetEventId(SPSMCategories category)
        {
            return 5000 + (int)category;
        }
    }
}
