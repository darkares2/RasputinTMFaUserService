using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Rasputin.TM {
    public class UserService {
        public async Task<User> InsertUser(ILogger log, CloudTable tblUser, string name, string password, User.UserTypes type, string email)
        {
            User user = new User(name, password, type, email);
            TableOperation operation = TableOperation.Insert(user);
            await tblUser.ExecuteAsync(operation);
            return user;
        }

        public async Task<User[]> FindAll(ILogger log, CloudTable tblUser)
        {
            log.LogInformation($"FindAll");
            List<User> result = new List<User>();
            TableQuery<User> query = new TableQuery<User>();
            TableContinuationToken continuationToken = null;
            try {
                do {
                var page = await tblUser.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = page.ContinuationToken;
                result.AddRange(page.Results);
                } while(continuationToken != null);
                return result.ToArray();
            } catch(Exception ex) {
                log.LogWarning(ex, "All");
                return null;
            }
        }

        public async Task<User> FindUser(ILogger log, CloudTable tblUser, Guid userID)
        {
            string pk = "p1";
            string rk = userID.ToString();
            log.LogInformation($"FindUser: {pk},{rk}");
            TableOperation operation = TableOperation.Retrieve(pk, rk);
            try {
                var tableResult = await tblUser.ExecuteAsync(operation);
                return tableResult.Result as User != null ? tableResult.Result as User : (User)tableResult;
            } catch(Exception ex) {
                log.LogWarning(ex, "FindUser", userID);
                return null;
            }
        }
    }
}