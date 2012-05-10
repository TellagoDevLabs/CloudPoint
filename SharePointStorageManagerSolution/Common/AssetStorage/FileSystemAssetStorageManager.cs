using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SPSM.Common.AssetStorage
{
    class FileSystemAssetStorageManager : IAssetStorageManager
    {
        private DirectoryInfo storageFolder;
        private string storageBaseAdddress;

        public FileSystemAssetStorageManager(string storageFolderPath, string storageBaseAdddress)
        {
            storageFolder = new DirectoryInfo(storageFolderPath);
            this.storageBaseAdddress = storageBaseAdddress;
            if (!this.storageBaseAdddress.EndsWith("/"))
            {
                this.storageBaseAdddress = this.storageBaseAdddress + "/";
            }
        }

        public void Delete(string fileUrl)
        {
            Uri fileUri = new Uri(fileUrl);
            FileUtils.Delete(fileUri.LocalPath);            
        }

        public string Save(System.IO.FileInfo file)
        {
            string destinationFilePath = Path.Combine(storageFolder.FullName, file.Name);
            file.CopyTo(destinationFilePath);
            string destinationUrl = storageBaseAdddress + file.Name;
            return destinationUrl;
        }

        public string Save(string fileName, System.IO.Stream fileStream)
        {
            string destinationFilePath = Path.Combine(storageFolder.FullName, fileName);
            using (Stream file = File.OpenWrite(destinationFilePath))
            {
                FileUtils.CopyStream(fileStream, file);
            }
            string destinationUrl = storageBaseAdddress + fileName;
            return destinationUrl;
        }

    }
}
