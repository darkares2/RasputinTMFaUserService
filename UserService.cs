using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Rasputin.TM {
    public class UserService {
        public async Task<User> InsertUser(ILogger log, CloudTable tblUser, string name, User.UserTypes type)
        {
            User user = new User(name, type);
            TableOperation operation = TableOperation.Insert(user);
            await tblUser.ExecuteAsync(operation);
            return user;
        }

        public async Task<User> FindUser(ILogger log, CloudTable tblUser, Guid userID)
        {
            string pk = "p1";
            string rk = userID.ToString();
            log.LogInformation($"FindUser: {pk},{rk}");
            TableOperation operation = TableOperation.Retrieve(pk, rk);
            return (User)await tblUser.ExecuteAsync(operation);
        }

    }
}