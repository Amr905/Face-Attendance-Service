using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount  
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types  
using System;
using System.IO;
using System.Net;

namespace FaceAttendance.Storage
{
    public enum ImageType { Url, image64 };
    class StorageService
    {
        public static CloudStorageAccount GetCloudStorageAccount()
        {
            return new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                "functionapp778fe8", "zf7JXe/C4NZMoyqsmiGeGKzeLBwl+vNo1GI8KqxUg2h7eDbJl/YdaQpGe/Y3EfIibXvmBsWGVF3RAOa10mvpQw=="), true);
        }
        public static async void UploadFileToStorage(string Url, string AppID, string ContextID)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount();// new CloudStorageAccount(storageCredentials, true);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get a reference to a container named "my-new-container."
            CloudBlobContainer container = blobClient.GetContainerReference("groupimages");


            // container.CreateIfNotExists();

            container.SetPermissions(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob

            });
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream inputStream = response.GetResponseStream();
            CloudBlockBlob BlockBlob = container.GetBlockBlobReference(AppID + "-" + ContextID + ".jpg");
            await BlockBlob.UploadFromStreamAsync(inputStream);

        }
        public static async void uploadImageAsync(string imageString, ImageType imageType, string AppID, string ContextID)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount();// new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Get a reference to a container named "my-new-container."
            CloudBlobContainer container = blobClient.GetContainerReference("groupimages");
            // container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob

            });
            Stream InputStream;
            if (imageType == ImageType.Url)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageString);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                InputStream = response.GetResponseStream();
            }
            else //if (imageType == ImageType.image64)
            {
                byte[] ImageByteArray = Convert.FromBase64String(imageString);
                InputStream = new MemoryStream(ImageByteArray);
            }
            CloudBlockBlob BlockBlob = container.GetBlockBlobReference(AppID + "-" + ContextID + ".jpg");
            await BlockBlob.UploadFromStreamAsync(InputStream);
        }
        public static string GetImageUrl(string AppID, string ContextID)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount();// new CloudStorageAccount(storageCredentials, true);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("groupimages");
            CloudBlockBlob BlockBlob = container.GetBlockBlobReference(AppID + "-" + ContextID + ".jpg");
            return BlockBlob.Uri.AbsoluteUri;

        }
    }
}
