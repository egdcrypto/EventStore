using EGD.Command.Models;
using Integrations.Messaging;
using Integrations.Messaging.Context;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventStoreServiceBusProcessor.Test
{
    public class WorkerTest
    {
        private readonly Mock<ILogger<Worker>> _loggerMock;
        private readonly Mock<IEventStoreContext> _eventStoreContextMock;
        private readonly Mock<ISubscriptionClient> _subscriptionClientMock;
        private CommandEventStore commandEventStore;
        public WorkerTest()
        {
            _loggerMock = new Mock<ILogger<Worker>>();
            _eventStoreContextMock = new Mock<IEventStoreContext>();
            _subscriptionClientMock = new Mock<ISubscriptionClient>();
        }
        [Fact]
        public async Task InsertOneAsync_CommandEvent_Null_Throws_ArgumentNullException()
        {
            Worker worker = new Worker(_loggerMock.Object, _subscriptionClientMock.Object, _eventStoreContextMock.Object);
        }
    }
}
