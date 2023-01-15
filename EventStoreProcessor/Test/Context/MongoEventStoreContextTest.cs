using Moq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Integrations.Messaging.Context;
using EGD.Command.Models;
using EGD.Command.Models.Enums;

namespace Integrations.Messaging.Test.Context
{
    public class MongoEventStoreContextTests
    {
        private readonly Mock<ILogger<MongoEventStoreContext>> _loggerMock;
        private readonly Mock<IMongoCollection<BsonDocument>> _mongoCollectionMock;
        private readonly IEventStoreContext _mongoEventStoreContext;
        private CommandEventStore commandEventStore;
        public MongoEventStoreContextTests()
        {
            _loggerMock = new Mock<ILogger<MongoEventStoreContext>>();
            _mongoCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            _mongoEventStoreContext = new MongoEventStoreContext(_loggerMock.Object, _mongoCollectionMock.Object);
        }

        private async Task SetupForSuccessCondition()
        {
            commandEventStore = new CommandEventStore()
            {
                Action = "test.action",
                Operation = CommandOperation.Process,
                CorrelationId = Guid.NewGuid(),
                Status = CommandStatus.New,
                Timestamp = DateTimeOffset.UtcNow,
                RecordId = Guid.NewGuid(),
                ProcessData = new { TestProperty1 = "Value", TestProperty2 = new { TestProperty3 = "Value", TestProperty4 = 1 } }
            };

            _mongoCollectionMock.Setup(s => s.InsertOneAsync(It.IsAny<BsonDocument>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task InsertOneAsync_CommandEvent_Null_Throws_ArgumentNullException()
        {

            ArgumentNullException agNullEx = null;
            try
            {
                await _mongoEventStoreContext.InsertOneAsync(null);
            }
            catch(ArgumentNullException ex)
            {
                agNullEx = ex;
            }

            Assert.NotNull(agNullEx);
        }

        [Fact]
        public async Task InsertOneAsync_CommandEvent_NotNull_Returns_Success()
        {
            Exception ex = null;
            await SetupForSuccessCondition();
            try
            {
                await _mongoEventStoreContext.InsertOneAsync(commandEventStore);
            }
            catch (Exception ex2)
            {
                ex = ex2;
            }

            Assert.Null(ex);
        }
    }
}