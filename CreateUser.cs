using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;

namespace Rasputin.TM
{
    public static class CreateUser
    {
        [FunctionName("CreateUser")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                                                    [Table("tblUsers")] CloudTable tblUser,
                                                    ILogger log)
        {
            log.LogInformation("CreateUser called");

            //string name = req.Query["name"];
            //User.UserTypes type = (User.UserTypes)Int32.Parse(req.Query["type"]);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data?.name;
            User.UserTypes type = data?.type;

            User user = await new UserService().InsertUser(log, tblUser, name, type);

            string responseMessage = JsonConvert.SerializeObject(user);
            return new OkObjectResult(responseMessage);
        }
    }
}
