using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.SharePoint.Common.Logging;
using Microsoft.SharePoint.Administration;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.Configuration;

namespace Tellago.SP.Logging
{
    class LoggingInstallerHelper
    {
        public static void AddCustomAreaAndCategories()
        {
            DiagnosticsArea customArea = new DiagnosticsArea(Areas.TellagoDevLabs.ToString());
            var categories = Enum.GetNames(typeof(LogCategories));
            foreach (var category in categories)
            {
                customArea.DiagnosticsCategories.Add(new DiagnosticsCategory(category, EventSeverity.Verbose, TraceSeverity.Verbose));
            }
            AddArea(customArea);
        }

        public static void RemoveCustomAreaAndCategories()
        {
            RemoveArea(Areas.TellagoDevLabs.ToString());
        }

        #region Private Methods

        private static void AddArea(DiagnosticsArea newArea)
        {
            DiagnosticsAreaCollection areas = GetCurrentAreas();
            var existingArea = areas.FirstOrDefault(area => area.Name == newArea.Name);
            if (existingArea == null)
            {
                areas.Add(newArea);
            }
            else
            {
                int index = areas.IndexOf(existingArea);
                foreach (DiagnosticsCategory item in newArea.DiagnosticsCategories)
                {
                    var existingCateg = areas[index].DiagnosticsCategories.FirstOrDefault(categ => categ.Name == item.Name);
                    if (existingCateg == null)
                    {
                        areas[index].DiagnosticsCategories.Add(item);
                    }
                }
            }
            areas.SaveConfiguration();
        }
        
        private static DiagnosticsAreaCollection GetCurrentAreas()
        {
            IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
            IConfigManager config = serviceLocator.GetInstance<IConfigManager>();
            DiagnosticsAreaCollection areaCollection = new DiagnosticsAreaCollection(config);
            return areaCollection;
        }

        private static void RemoveArea(string areaName)
        {
            DiagnosticsAreaCollection areas = GetCurrentAreas();
            if (ExistArea(areas, areaName))
            {
                while (areas[areaName].DiagnosticsCategories.Count != 0)
                {
                    areas[areaName].DiagnosticsCategories.Clear();
                }
                areas.RemoveAt(areas.IndexOf(areas[areaName]));
                areas.SaveConfiguration();
            }
        }

        private static bool ExistArea(DiagnosticsAreaCollection collection, string areaName)
        {
            foreach (DiagnosticsArea item in collection)
            {
                if (item.Name.Trim().ToUpper() == areaName.Trim().ToUpper())
                    return true;
            }
            return false;
        }

        #endregion

    }
}
