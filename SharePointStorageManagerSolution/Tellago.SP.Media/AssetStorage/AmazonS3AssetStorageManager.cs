using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.SharePoint.Administration;

namespace Tellago.SP.Media.AssetStorage
{
    public class AmazonS3AssetStorageManager : IAssetStorageManager
    {

        private string bucketName = string.Empty;
        private string keyName = string.Empty;
        private string accessKeyID = string.Empty;
        private string secretAccessKeyID = string.Empty;

        AmazonS3 client;

        public AmazonS3AssetStorageManager(string _bucketName, string _keyName, string _accessKeyID, string _secretAccessKeyID)
        {
            bucketName = _bucketName;
            keyName = _keyName;
            accessKeyID = _accessKeyID;
            secretAccessKeyID = _secretAccessKeyID;
        }

        public void Delete(string fileName)
        {
            string uniqueKeyItemName = string.Format("{0}-{1}", keyName, fileName);
            DeleteObjectRequest deleteObjectRequest =
         new DeleteObjectRequest()
         .WithBucketName(bucketName)
         .WithKey(uniqueKeyItemName );

            using (client = new AmazonS3Client(accessKeyID, secretAccessKeyID))
            {
                try
                {
                    client.DeleteObject(deleteObjectRequest);
                }
                catch (AmazonS3Exception s3Exception)
                {
                    throw new Exception( String.Format("Error Occurred in Delete operation for ObjectKeyID: {0}", uniqueKeyItemName ),s3Exception);
                }
            }
           
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
            AmazonS3Config S3Config = new AmazonS3Config()
            {
                ServiceURL = "s3.amazonaws.com",
                CommunicationProtocol = Amazon.S3.Model.Protocol.HTTP,
            };

            using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKeyID, secretAccessKeyID, S3Config ))
                {
                    return UploadToAmazon(fileName, fileStream);
                }
          
        }

        string  UploadToAmazon(string fileName, Stream fileStream)
        {
            try
            {
                string uniqueKeyItemName = string.Format("{0}-{1}", keyName, fileName);
                PutObjectRequest request = new PutObjectRequest();
                request.WithInputStream(fileStream);
                request.WithBucketName(bucketName)
                    .WithKey(uniqueKeyItemName);
                request.WithMetaData("title", fileName);
                // Add a header to the request.
                //request.AddHeaders(AmazonS3Util.CreateHeaderEntry ("ContentType", contentType));

                S3CannedACL anonPolicy = S3CannedACL.PublicRead;
                request.WithCannedACL(anonPolicy);
                S3Response response = client.PutObject(request);
                
               //GetPreSignedUrlRequest publicUrlRequest = new GetPreSignedUrlRequest().WithBucketName(bucketName).WithKey( uniqueKeyItemName ).WithExpires(DateTime.Now.AddMonths(3) );
                //var urlResponse = client.GetPreSignedURL(publicUrlRequest);
                response.Dispose();
                var urlResponse = string.Format("https://s3.amazonaws.com/{0}/{1}", bucketName, uniqueKeyItemName );
                return urlResponse;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Error - Invalid Credentials - please check the provided AWS Credentials", amazonS3Exception);
                }
                else
                {
                    throw new Exception(String.Format("Error occured when uploading media: {0}",amazonS3Exception.Message),amazonS3Exception);
                }
            }
        }
    }
    
}
