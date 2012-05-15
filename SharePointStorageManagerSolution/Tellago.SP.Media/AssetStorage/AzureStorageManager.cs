using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;

namespace Tellago.SP.Media.AssetStorage
{
    class AzureStorageManager : IAssetStorageManager
    {
        private string accountName;
        private string accountKey;
        private string blobEndpoint;
        private string containerAddress;

        public AzureStorageManager(string accountName,string accountKey,string blobEndpoint,string container)
        {
            this.accountName = accountName;
            this.accountKey = accountKey;
            this.blobEndpoint = blobEndpoint;
            this.containerAddress = container;
        }
        public void Delete(string fileUrl)
        {
            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = new CloudBlobClient(blobEndpoint, new StorageCredentialsAccountAndKey(accountName, accountKey));

            //Get a reference to a container, which may or may not exist.
            CloudBlobContainer container = blobClient.GetContainerReference(containerAddress);

            //Get a reference to a blob, which may or may not exist.
            CloudBlob blob = container.GetBlobReference(fileUrl);

            //Upload content to the blob, which will create the blob if it does not already exist.
            blob.DeleteIfExists();
        }

        public string Save(System.IO.FileInfo file)
        {
            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = new CloudBlobClient(blobEndpoint, new StorageCredentialsAccountAndKey(accountName, accountKey));

            //Get a reference to a container, which may or may not exist.
            CloudBlobContainer container = blobClient.GetContainerReference(containerAddress);

            //Create a new container, if it does not exist
            container.CreateIfNotExist();

            //Get a reference to a blob, which may or may not exist.
            CloudBlob blob = container.GetBlobReference(file.Name);

            //Upload content to the blob, which will create the blob if it does not already exist.
            blob.UploadFile(file.FullName);

            return blob.Uri.ToString();
        }

        public string Save(string fileName, System.IO.Stream fileStream)
        {
            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = new CloudBlobClient(blobEndpoint, new StorageCredentialsAccountAndKey(accountName, accountKey));

            //Get a reference to a container, which may or may not exist.
            CloudBlobContainer container = blobClient.GetContainerReference(containerAddress);

            //Create a new container, if it does not exist
            container.CreateIfNotExist();

            //Get a reference to a blob, which may or may not exist.
            CloudBlob blob = container.GetBlobReference(fileName);

            //Upload content to the blob, which will create the blob if it does not already exist.
            blob.UploadFromStream(fileStream);

            return blob.Uri.ToString();
        }
    }
}
