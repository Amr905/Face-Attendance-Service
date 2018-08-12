using FaceAttendance.Mapping;
using FaceAttendance.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Azure;
namespace FaceAttendance.FaceApi
{
    public static class FaceService
    {
        const string baseURL = "https://eastus.api.cognitive.microsoft.com/face/v1.0";

        static HttpClient InitializeClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "7c6ca37b2b094e5bacdd96dbb45b76fe");//CloudConfigurationManager.GetSetting("SupscriptionKey")
            return client;
        }

        public static async Task<string> CreatePerson(string GroupID, string Name)
        {
            HttpClient client = InitializeClient();
            byte[] byteData = Encoding.UTF8.GetBytes("{\"name\": \"" + Name + "\",}");
            string personId;
            // var content = new ByteArrayContent(byteData);
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(@baseURL + "/persongroups/" + GroupID + "/persons", content);
                //Console.WriteLine(response.Content);
                var responseString = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(responseString);
                JObject responseJson = JObject.Parse(responseString.ToString());
                personId = responseJson["personId"].ToString();
            }
            return personId;
        }

        public static async Task AddFaceToPerson(string PersonGroupId, string PersonID, string ImageUrl, Location targetFace)
        {
            HttpClient client = InitializeClient();
            byte[] byteData = Encoding.UTF8.GetBytes("{\"url\": \"" + ImageUrl + "\",}");
            // var content = new ByteArrayContent(byteData);
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //optional requests //[?userData][&targetFace]
                var response = await client.PostAsync(@baseURL + "/persongroups/" + PersonGroupId + "/persons/" + PersonID + "/persistedFaces?targetFace=" + targetFace.Left + "," + targetFace.Top + "," + targetFace.Width + "," + targetFace.Height, content);

                //Console.WriteLine(response.Content);
                var responseString = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(responseString);
                if (!responseString.Contains("persistedFaceId"))
                {
                    throw JsonConvert.DeserializeObject<ErrorMsg>(responseString);
                }
            }

        }

        public static async Task CreatePersonGroup(string groupID, string GroupName)
        {
            HttpClient client = InitializeClient();
            byte[] byteData = Encoding.UTF8.GetBytes("{\"name\": \"" + GroupName + "\",}");
            // var content = new ByteArrayContent(byteData);
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PutAsync(@baseURL + "/persongroups/" + groupID, content);
                //Console.WriteLine(response.Content);
                var responseString = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(responseString + "  Done");
            }

        }

        public static async Task<string> GetPersonGroup(String groupID)
        {

            HttpClient client = InitializeClient();


            var response = await client.GetAsync(@baseURL + "/persongroups/" + groupID);
            //Console.WriteLine(response.Content);

            var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseString);

            return responseString;

        }

        public static async Task GetPerson(int groupID, string personID)
        {
            HttpClient client = InitializeClient();

            var response = await client.GetAsync(@baseURL + "/persongroups/" + groupID + "/persons/" + personID);
            //Console.WriteLine(response.Content);

            var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseString);

        }
        public static async Task TrainPersonGroup(string groupID)
        {
            HttpClient client = InitializeClient();

            var response = await client.PostAsync(@baseURL + "/persongroups/" + groupID + "/train", null);
            //Console.WriteLine(response.Content);

            var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(response.IsSuccessStatusCode);
        }

        public static async Task GetTrainingStatus(string groupID)
        {
            HttpClient client = InitializeClient();

            var response = await client.GetAsync(@baseURL + "/persongroups/" + groupID + "/training");
            //Console.WriteLine(response.Content);

            var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(response.IsSuccessStatusCode);
        }
        public static async Task<List<Face>> FaceDetection(string ImageUrl)
        {

            HttpClient client = InitializeClient();
            byte[] byteData = Encoding.UTF8.GetBytes("{\"url\": \"" + ImageUrl + "\",}");
            var responseString = "";
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(@baseURL + "/detect", content);
                //Console.WriteLine(response.Content);

                responseString = await response.Content.ReadAsStringAsync();
                if (responseString.Contains("error"))
                {
                    
                    throw JsonConvert.DeserializeObject<ErrorMsg>(responseString);
                }
                 
             
            }
            return JsonConvert.DeserializeObject<List<Face>>(responseString);
        }

        public static async Task<Tuple<List<Face>, List<User>>> Identify(string personGroupId, List<Face> faces, FaceIdentificationContext appContext)
        {
            var iReturn = new Tuple<List<Face>, List<User>>(faces, new List<User>());

            HttpClient client = InitializeClient();
            string facesIds = string.Join("\",\"", faces.Select(f => f.FaceID));

            List<identifyResponse> iResponse = new List<identifyResponse>();

            byte[] byteData = Encoding.UTF8.GetBytes("{\"personGroupId\":\"" + personGroupId + "\",\"faceIds\":[\"" + facesIds + "\"]}");


            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(@baseURL + "/identify?maxNumOfCandidatesReturned=1", content);

                var responseString = await response.Content.ReadAsStringAsync();

                iResponse = JsonConvert.DeserializeObject<List<identifyResponse>>(responseString);
            }
            for (int i = 0; i < iResponse.Count; i++)
            {
                identifyResponse item = iResponse[i];
                if (item.Candidates.Count > 0)
                {
                    string personId = item.Candidates[0].PersonId;
                    string userId = MappingService.GetFromMapping(appContext.AppID + "-" + appContext.Context.Id, personId);
                    User user = new User(userId, "");//TODO:Change to retrive user name appContext.users[i].name);
                    iReturn.Item2.Add(user);//appid-context, personid
                    iReturn.Item1.RemoveAt(i);
                }
            }

            return iReturn;
        }

    }
}
