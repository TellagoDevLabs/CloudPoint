using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.SharePoint;

namespace Tellago.SP.Media.AssetStorage
{
    public class SPLibraryAssetStorageManager : IAssetStorageManager
    {
        private string libraryName;
        private string webUrl;
       
        public SPLibraryAssetStorageManager(string webUrl, string libraryName)
        {
            this.webUrl = webUrl;
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
            using (SPSite site = new SPSite(webUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPDocumentLibrary library = web.Lists[libraryName] as SPDocumentLibrary;
                    SPFile uploadedFile = library.RootFolder.Files.Add(fileName, fileStream, true);
                    return String.Concat(web.Url, "/", uploadedFile.Url); 

                }
            }
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
