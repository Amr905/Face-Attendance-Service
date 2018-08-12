using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using FaceAttendance.FaceApi;
using FaceAttendance.Mapping;
using FaceAttendance.Storage;

namespace FaceAttendance.Model
{
    public static class IdentifyFunction
    {
        [FunctionName("Identify")]
        public static async Task<HttpResponseMessage> IdentifyAzureFunction([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var Content = req.Content;
            string JsonContent = await Content.ReadAsStringAsync();
            FaceIdentificationContext Appcontext = null;
            try
            {
                Appcontext = JsonConvert.DeserializeObject<FaceIdentificationContext>(JsonContent);
                if (Appcontext.AppID == "" || Appcontext.Context.Id == "")
                    throw new ErrorMsg("WrongRequestFormat", "json format is invalid maybe some fileds are missing ");

                String Result = JsonConvert.SerializeObject(await FaceIdentification(Appcontext));
                return req.CreateResponse(HttpStatusCode.OK, Result);
            }
            catch (ErrorMsg ErrorMsgException)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(ErrorMsgException.error));
            }
        }
        

        static async Task<Tuple<List<Face>, List<User>>> FaceIdentification(FaceIdentificationContext appContext)
        {

            var IReturn = new Tuple<List<Face>, List<User>>(new List<Face>(), new List<User>());
            if (appContext.ImageUrl == null)
                throw new ErrorMsg("imageDoesntExist", "image not found maybe link is not attached ");
            List<Face> Faces = await FaceService.FaceDetection(appContext.ImageUrl);
            string personGroupId = MappingService.GetFromMapping(appContext.AppID, appContext.Context.Id);//get from mapping

            if (MappingService.IsPersonGroupExist(appContext.AppID, appContext.Context.Id))//check if context has group ? get groupID : return detection in faces and emty users 
            {
                IReturn = await FaceService.Identify(personGroupId, Faces, appContext);
            }
            IReturn = Tuple.Create(Faces, new List<User>());
            StorageService.uploadImageAsync(appContext.ImageUrl, ImageType.Url, appContext.AppID, appContext.Context.Id);
            return IReturn;
        }
    }
}
