using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Utilities;
using SPSM.Common.Logging;
using Microsoft.SharePoint.Administration;

namespace SPSM.Common.AssetStorage
{
    public class SPLibraryAssetStorageManager : SPSM.Common.AssetStorage.IAssetStorageManager
    {
        private string libraryName;
        //private string serverRelativeLibraryUrl;
        private string webUrl;
        private Logger logger = new Logger();

        public SPLibraryAssetStorageManager(string webUrl, string libraryName)
        {
            this.webUrl = webUrl;
            //string webRelativeUrl = new Uri(webUrl).PathAndQuery;
            //this.serverRelativeLibraryUrl = String.Concat(webRelativeUrl, mediaLibraryName);
            this.libraryName = libraryName;
        }

        public string Save(FileInfo file)
        {
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                return Save(file.Name, fileStream);
            }
        }

        public string Save(string fileName, Stream fileStream)
        {
            //Changed to use server OB. Client OB was failing with some library names & we are using this inside Sharepoint
            using (SPSite site = new SPSite(webUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPDocumentLibrary library = web.Lists[libraryName] as SPDocumentLibrary;
                    SPFile uploadedFile = library.RootFolder.Files.Add(fileName, fileStream, true);
                    return String.Concat(web.Url, "/", uploadedFile.Url); 

                }
            }
            //string destinationUrl = String.Concat(serverRelativeLibraryUrl, "/", fileName);
            //ClientContext clientContext = new ClientContext(webUrl);
            //{
            //    logger.LogToOperations(SPSMCategories.Default, EventSeverity.Information,
            //                           "Saving file: {0} to location: {1}", fileName, destinationUrl);

            //    Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext, destinationUrl, fileStream, true);
            //}

            //return destinationUrl;
        }

        public void Delete(string fileUrl)
        {
            using (SPSite site = new SPSite(webUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPFile file = web.GetFile(fileUrl);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
            }
        }
    }
}
