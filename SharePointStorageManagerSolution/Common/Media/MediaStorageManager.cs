using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Utilities;

namespace SPSM.Common.Media
{
    public class MediaStorageManager
    {
        private string serverRelativeLibraryUrl;
        private string webUrl;

        public MediaStorageManager(SPWeb web, string mediaLibraryName)
        {
            webUrl = web.Url;
            serverRelativeLibraryUrl = String.Concat(web.ServerRelativeUrl,mediaLibraryName);
        }


        public string Upload(FileInfo file)
        {
            string destinationUrl = String.Concat(serverRelativeLibraryUrl,"/", file.Name);
            ClientContext clientContext = new ClientContext(webUrl);
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext, destinationUrl, fileStream, true);
            }

            return destinationUrl;
        }

        public string Upload(string fileName, Stream fileStream)
        {
            string destinationUrl = String.Concat(serverRelativeLibraryUrl, "/", fileName);
            ClientContext clientContext = new ClientContext(webUrl);
            {
                Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext, destinationUrl, fileStream, true);
            }

            return destinationUrl;
        }




        public void DeleteFromFileSystem(string filePath)
        {
            if (!String.IsNullOrEmpty(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                DeleteFromFileSystem(fi);
            }
        }

        public void DeleteFromFileSystem(FileInfo fi)
        {
            if (fi.Exists)
            {
                fi.Delete();
            }
        }

        public void DeleteFromSPLibrary(string fileUrl)
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
