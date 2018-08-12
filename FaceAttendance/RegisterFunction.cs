using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using FaceAttendance.FaceApi;
using FaceAttendance.Mapping;
using FaceAttendance.Storage;
namespace FaceAttendance.Model
{
    public static class RegisterFunction
    {
        [FunctionName("Register")]
        public static async Task<HttpResponseMessage> RegisterAzureFunction([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var Content = req.Content;
            string JsonContent = await Content.ReadAsStringAsync();
            FaceRegistrationContext AppContext = null;
            try
            {
                AppContext = JsonConvert.DeserializeObject<FaceRegistrationContext>(JsonContent);

            }
            catch
            {
                ErrorMsg msg = new ErrorMsg("WrongRequestFormat", "json format is invalid maybe some fileds are missing ");
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(msg));
            }
            try
            {
                await Register(AppContext);
                return req.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (ErrorMsg ErrorMsgException)
            {
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(ErrorMsgException.error));
            }
            

        }
        public async static Task Register(FaceRegistrationContext appContext) //input : group name, persons names
        {
            string GroupId;
            if (MappingService.IsPersonGroupExist(appContext.AppId, appContext.Context.Id))
            {
                GroupId = MappingService.GetFromMapping(appContext.AppId, appContext.Context.Id);
            }
            else
            {//creat group
                Guid g = Guid.NewGuid();
                GroupId = g.ToString();
                await FaceService.CreatePersonGroup(GroupId, appContext.Context.Name);
                await MappingService.InsertNewPersonGroup(appContext.AppId, appContext.Context.Id, GroupId);
            }
            //creat n persons
            string ImgURL = StorageService.GetImageUrl(appContext.AppId, appContext.Context.Id); //mapping get img using appid,contextid
            if (ImgURL == "")
            {
                throw new ErrorMsg("InvalidURL", "Failed to download from target server. Remote server error returned.");
            }
            foreach (User user in appContext.Users)
            {
                string PersonId;
                if (MappingService.GetFromMapping(appContext.AppId + "-" + appContext.Context.Id, user.Id) != "")
                {
                    PersonId = MappingService.GetFromMapping(appContext.AppId + "-" + appContext.Context.Id, user.Id);
                }
                else
                {
                    //create person
                    PersonId = await FaceService.CreatePerson(GroupId, user.Name); //temp names
                    if (PersonId == "")
                        throw new ErrorMsg("PersonCreationFailed", "Failed to Create person. maybe it reached the limit or connection error");
                    //Console.WriteLine("person " + user.Name + " id: " + PersonId);
                    //insert person
                    IList<TableResult> TempResult = await MappingService.InsertNewPerson(appContext.AppId, appContext.Context.Id, PersonId, user.Id);
                    foreach (TableResult Result in TempResult)
                    {
                        if (!Result.HttpStatusCode.Equals(HttpStatusCode.OK))
                            throw new ErrorMsg("PersonAdditionToStorageFailed", "Failed to Store person In Azure Table");
                    }
                    //add face to person 

                    await FaceService.AddFaceToPerson(GroupId, PersonId, ImgURL, user.Face.Location);//add last image

                }

                //train person group
                await FaceService.TrainPersonGroup(GroupId);

                //check training status
                await FaceService.GetTrainingStatus(GroupId);

            }
        }
    }
}
