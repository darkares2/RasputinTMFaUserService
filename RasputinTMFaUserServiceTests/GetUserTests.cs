using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Rasputin.TM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RasputinTMFaUserServiceTests
{
    public class GetUserTests
    {
        [Fact]
        public async Task GetUserByUserID()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            Guid userID = Guid.NewGuid();
            var qs = new Dictionary<string, StringValues>
            {
                { "userID", userID.ToString() }
            };
            request.Query = new QueryCollection(qs);

            var iLoggerMock = new Mock<ILogger>();
            var tblUserMock = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            User user1 = new User() { RowKey = userID.ToString(), Name = "Test", Type = "Patient", Email = "test@test.com" };
            TableResult tableResult = new TableResult();
            tableResult.Result = user1;
            tableResult.HttpStatusCode = 200;
            tblUserMock.Setup(_ => _.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(tableResult);

            // Act
            OkObjectResult result = (OkObjectResult)await GetUser.Run(request, tblUserMock.Object, iLoggerMock.Object);

            // Assert
            Assert.Equal(200, result.StatusCode);
            User userResult = (User)JsonConvert.DeserializeObject((string)result.Value, typeof(User));
            Assert.Equal(user1.UserID, userResult.UserID);
        }
        [Fact]
        public async Task GetUserByUserIDNotFound()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            Guid userID = Guid.NewGuid();
            var qs = new Dictionary<string, StringValues>
            {
                { "userID", userID.ToString() }
            };
            request.Query = new QueryCollection(qs);

            var iLoggerMock = new Mock<ILogger>();
            var tblUserMock = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            TableResult tableResult = new TableResult();
            tableResult.HttpStatusCode = 404;
            tblUserMock.Setup(_ => _.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(tableResult);

            // Act
            NotFoundResult result = (NotFoundResult)await GetUser.Run(request, tblUserMock.Object, iLoggerMock.Object);

            // Assert
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task TestGetAll()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;

            var iLoggerMock = new Mock<ILogger>();
            var tblUserMock = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            User user1 = new User() { RowKey = Guid.NewGuid().ToString(), Name = "Test1", Type = "Doctor", Password = "Secret", Email = "Test1@test.com" };
            User user2 = new User() { RowKey = Guid.NewGuid().ToString(), Name = "Test2", Type = "Patient", Password = "Secret", Email = "Test2@test.com" };
            List<User> users = new List<User>() { user1, user2 };
            var resultMock = new Mock<TableQuerySegment<User>>(users);
            tblUserMock.Setup(_ => _.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<User>>(), It.IsAny<TableContinuationToken>())).ReturnsAsync(resultMock.Object);

            // Act
            OkObjectResult result = (OkObjectResult)await GetUser.Run(request, tblUserMock.Object, iLoggerMock.Object);

            // Assert
            Assert.Equal(200, result.StatusCode);
            User[] userResult = (User[])JsonConvert.DeserializeObject((string)result.Value, typeof(User[]));
            Assert.Equal(2, userResult.Length);
            Assert.Equal(user1.UserID, userResult[0].UserID);
            Assert.Equal(User.UserTypes.Doctor, userResult[0].TypeId);
            Assert.Equal(user2.UserID, userResult[1].UserID);
            Assert.Equal(User.UserTypes.Patient, userResult[1].TypeId);
        }

    }
}
