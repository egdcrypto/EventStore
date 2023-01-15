using EGD.Command.Models;
using Integrations.Messaging.Context;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace Integrations.Messaging
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISubscriptionClient _subscriptionClient;
        private readonly IEventStoreContext _eventStoreContext;

        public Worker(ILogger<Worker> logger, ISubscriptionClient subscriptionClient, IEventStoreContext eventStoreContext)
        {
            _logger = logger;
            _subscriptionClient = subscriptionClient;
            _eventStoreContext = eventStoreContext;
            _logger.LogInformation("Starting worker.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 100,
                AutoComplete = false
            };
            _subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
            await _subscriptionClient.CloseAsync();
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Exception:: {exceptionReceivedEventArgs.Exception}.");
            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var commandEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<CommandEventStore>(Encoding.UTF8.GetString(message.Body));
            if (commandEvent != null)
            {
                _logger.LogInformation(string.Format("{0} - Message received. {1}|{2}|{3}", commandEvent.Timestamp, commandEvent.Action, commandEvent.Status, commandEvent.CorrelationId));

                await _eventStoreContext.InsertOneAsync(commandEvent);
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }
    }
};