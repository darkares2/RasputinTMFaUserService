using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using SendGrid.Helpers.Mail;

namespace Rasputin.TM
{
    public static class HandleUserMessage
    {
        [FunctionName("HandleUserMessage")]
        public static async Task Run([QueueTrigger("userMessageQueue")] string userMessageQueueItem,
                                     [Table("tblUsers")] CloudTable tblUsers,
                                     [SendGrid(ApiKey = "SendGridApiKey")] IAsyncCollector<SendGridMessage> messageCollector,
                                     ILogger log)
        {
            log.LogInformation("HandleUserMessage called");
            
            UserMessage userMessage = (UserMessage)JsonConvert.DeserializeObject(userMessageQueueItem, typeof(UserMessage));
            User user = await new UserService().FindUser(log, tblUsers, userMessage.UserID);
            if (user == null)
                throw new Exception("User not found");
            log.LogInformation($"Sending mail to user: {user.Name}, {user.Email}");
            if (user.Email == null)
                throw new Exception("User does not have an email");
            var message = new SendGridMessage();
            message.AddContent("text/plain", userMessage.message);
            message.SetSubject("Mail from RasputinTM");
            message.SetFrom("thanbol@pm.me");
            message.AddTo(user.Email);

            await messageCollector.AddAsync(message);
        }
    }
}
