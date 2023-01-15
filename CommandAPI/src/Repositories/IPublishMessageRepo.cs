using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using System;
using System.Threading.Tasks;

namespace CommandAPI.Repositories
{
    public interface IPublishMessageRepo : IAsyncDisposable
    {
        public Task PublishServiceBusMessageAsync(Message message);
       
    }
}
