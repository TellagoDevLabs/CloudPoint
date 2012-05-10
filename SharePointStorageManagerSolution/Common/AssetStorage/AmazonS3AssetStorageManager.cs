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
using SPSM.Common.Logging;

namespace SPSM.Common.AssetStorage
{
    public class AmazonS3AssetStorageManager : IAssetStorageManager
    {

        private const string AMAZON_SECURITY_EXTENSION = "_amzsec";
        private const string AMAZON_S3_URL = "https://s3.amazonaws.com";
        private const string AMAZON_S3 = "s3.amazonaws.com";
        private string bucketName = string.Empty;
        private string keyName = string.Empty;
        private string accessKeyID = string.Empty;
        private string secretAccessKeyID = string.Empty;
        private Boolean makePublic = false;
        private int expireMinutes = 15;

        AmazonS3 client;
        private Logger logger;
        private string webUrl;

        public AmazonS3AssetStorageManager(string _bucketName, string _keyName, string _accessKeyID, string _secretAccessKeyID, string _makePublic, string _expireMinutes, string _webUrl)
        {
            webUrl = _webUrl;
            bucketName = _bucketName;
            keyName = _keyName;
            accessKeyID = _accessKeyID;
            secretAccessKeyID = _secretAccessKeyID;
            Boolean.TryParse(_makePublic, out makePublic);
            int.TryParse(_expireMinutes, out expireMinutes);

            //check for 0 minutes, if so always default to 15
            if (expireMinutes == 0) expireMinutes = 15;

            logger = new Logger();
        }

        public void Delete(string fileName)
        {
            string uniqueKeyItemName = string.Format("{0}-{1}", keyName, fileName);
            DeleteObjectRequest deleteObjectRequest =
         new DeleteObjectRequest()
         .WithBucketName(bucketName)
         .WithKey(uniqueKeyItemName);

            using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKeyID, secretAccessKeyID))
            {
                try
                {
                    client.DeleteObject(deleteObjectRequest);
                    logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object KeyID: {0} deleted successfully", uniqueKeyItemName);

                    //now check to see if poster and thumbnail exists
                    var resource_WithoutExt = uniqueKeyItemName.Substring(0, uniqueKeyItemName.Length - 4);
                    var resourceExt = uniqueKeyItemName.Substring(uniqueKeyItemName.Length - 4);

                    var posterResource = resource_WithoutExt + "_poster" + resourceExt;
                    var thumbResource = resource_WithoutExt + "_thumb" + resourceExt;

                    GetObjectRequest getPosterRequest = new GetObjectRequest().WithBucketName(bucketName)
         .WithKey(posterResource);
                    GetObjectRequest getThumbRequest = new GetObjectRequest().WithBucketName(bucketName)
         .WithKey(thumbResource);

                    try
                    {
                        //see if it exists
                        var posterObj = client.GetObject(getPosterRequest);
                        if (posterObj.ContentLength > 0)
                        {
                            logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object Poster KeyID: {0} found", posterResource );
                            // then delete
                            DeleteObjectRequest delPoster = new DeleteObjectRequest().WithBucketName(bucketName)
         .WithKey(posterResource);
                            client.DeleteObject(delPoster);
                            logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object Poster KeyID: {0} deleted successfully", posterResource);
                        }
                    }
                    catch (Exception )
                    {
                        // we expect Poster to not be found in some scenarios
                        logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object Poster KeyID: {0} not found", posterResource);
                    }

                    try
                    {
                        //see if it exists
                        var thumbObj = client.GetObject(getThumbRequest);
                        if (thumbObj.ContentLength > 0)
                        {
                            logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object Thumbnail KeyID: {0} found", thumbResource);
                            // then delete
                            DeleteObjectRequest thumbPoster = new DeleteObjectRequest().WithBucketName(bucketName)
         .WithKey(thumbResource);
                            client.DeleteObject(thumbPoster);
                            logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object Thumbnail KeyID: {0} deleted successfully", thumbResource);
                        }
                    }
                    catch (Exception)
                    {
                        // we expect thumbnail to not be found in some scenarios
                        logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object Thumbnail KeyID: {0} not found", thumbResource);
                    }
                }
                catch (AmazonS3Exception s3Exception)
                {
                    // Console.WriteLine(s3Exception.Message,s3Exception.InnerException);
                    logger.LogToOperations(s3Exception, SPSMCategories.Media, EventSeverity.ErrorCritical,
                                           "Error Occurred in Delete operation for ObjectKeyID: {0}", uniqueKeyItemName);
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
                ServiceURL = AMAZON_S3,
                CommunicationProtocol = Amazon.S3.Model.Protocol.HTTP,
            };

            using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKeyID, secretAccessKeyID, S3Config))
            {
                return UploadToAmazon(fileName, fileStream);
            }

        }

        string UploadToAmazon(string fileName, Stream fileStream)
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
                
                // Check for Thumbnail or Poster images...
                // if it is then make the url public
                makePublic = CheckForThumbOrPoster(fileName);

                //Check for if we need to make public
                if (makePublic)
                {
                    S3CannedACL anonPolicy = S3CannedACL.PublicRead;
                    request.WithCannedACL(anonPolicy);
                }
                S3Response response = client.PutObject(request);

                //Set the Public response
                var urlResponse = string.Format(AMAZON_S3_URL + "/{0}/{1}", bucketName, uniqueKeyItemName);

                //if not public then get Signed URL
                if (!makePublic)
                {
                    GetPreSignedUrlRequest privateUrlRequest =
                        new GetPreSignedUrlRequest().WithBucketName(bucketName).WithKey(uniqueKeyItemName).WithExpires(
                            DateTime.Now.AddMinutes(expireMinutes));
                    //i.e. https://currentdevbucket.s3.amazonaws.com/{resource}?AWSAccessKeyId=AKIAIYNCH5NSNY3HKP3Q&Expires=1331068519&Signature=LIzXrLt5p4m5%2FiMGhCoACd4TO%2Fs%3D
                    string amazonSignedUrlPrefix = string.Format("https://{0}.{1}", bucketName, AMAZON_S3);
                    //Check for url port number if so strip off
                    
                    var replacedHost = client.GetPreSignedURL(privateUrlRequest).Replace(amazonSignedUrlPrefix, webUrl);

                    var uniqueKey = uniqueKeyItemName.Substring(0, uniqueKeyItemName.Length - 4);
                    var uniqueExtension = uniqueKeyItemName.Substring(uniqueKeyItemName.Length - 4);
                    urlResponse = replacedHost.Replace(uniqueKeyItemName, uniqueKey + AMAZON_SECURITY_EXTENSION + uniqueExtension );


                }

                response.Dispose();
                return urlResponse;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {

                    logger.LogToOperations(amazonS3Exception, SPSMCategories.Media, EventSeverity.ErrorCritical,
                                           "Error - Invalid Credentials - please check the provided AWS Credentials");
                    return null;
                }
                else
                {
                    logger.LogToOperations(amazonS3Exception, SPSMCategories.Media, EventSeverity.ErrorCritical,
                                                   "Error occured when uploading media: {0}", amazonS3Exception.Message);
                    return null;
                }
            }
        }

        private bool CheckForThumbOrPoster(string fileName)
        {
            var result = (fileName.Contains("_thumb") || fileName.Contains("_poster"));
            return result;
        }


        public string RetrieveNewSignedUrl(string resourceName)
        {
            string uniqueKeyItemName = resourceName; //string.Format("{0}-{1}", keyName, fileName);
            string urlResponse = "";
            using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKeyID, secretAccessKeyID))
            {
                try
                {

                    GetPreSignedUrlRequest privateUrlRequest =
                       new GetPreSignedUrlRequest().WithBucketName(bucketName).WithKey(uniqueKeyItemName).WithExpires(
                           DateTime.Now.AddMinutes(expireMinutes));
                    urlResponse = client.GetPreSignedURL(privateUrlRequest);
                    logger.LogToOperations(SPSMCategories.Media, EventSeverity.Information, "Amazon Object KeyID: {0} retrieved, with Signed URL: {1}", uniqueKeyItemName, urlResponse);

                }
                catch (AmazonS3Exception s3Exception)
                {
                    logger.LogToOperations(s3Exception, SPSMCategories.Media, EventSeverity.ErrorCritical,
                                           "Error Occurred in RetreiveNewSignedUrl operation for ObjectKeyID: {0}", uniqueKeyItemName);
                }
            }
            return urlResponse;

        }
    }

}
