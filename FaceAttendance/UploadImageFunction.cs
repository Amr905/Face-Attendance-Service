using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager  
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount  
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types  
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.IO;
using FaceAttendance.FaceApi;
using FaceAttendance.Mapping;
using FaceAttendance.Storage;
using Newtonsoft.Json.Serialization;
using FaceAttendance.Model;

namespace FaceAttendance
{
    public static class UploadImageFunction
    {
        [FunctionName("UploadImage")]
        public async static Task<string> UploadImageAzureFunction([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var Content = req.Content;
                string JsonContent = await Content.ReadAsStringAsync();
                FaceIdentificationContext FaceAppContext = JsonConvert.DeserializeObject<FaceIdentificationContext>(JsonContent);
                StorageService.uploadImageAsync(FaceAppContext.ImageUrl, ImageType.image64, FaceAppContext.AppID, FaceAppContext.Context.Id);
                return StorageService.GetImageUrl(FaceAppContext.AppID, FaceAppContext.Context.Id);
            }
            catch(ErrorMsg ErrorMsgException)
            {
                return  JsonConvert.SerializeObject(ErrorMsgException.error);

            }
        }
    }
}
