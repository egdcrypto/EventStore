using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using CommandAPI.Models;
using EGD.Command.Models;
using EGD.Command.Models.Enums;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommandAPI.Repositories
{
    public class PublishMessageRepo : IPublishMessageRepo
    {
        private readonly ITopicClient _topicClient;
        private readonly ILogger<PublishMessageRepo> _logger;
        public PublishMessageRepo(ILogger<PublishMessageRepo> logger, ITopicClient topicClient)
        {
            _topicClient = topicClient;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_topicClient != null)
            {
                await _topicClient.CloseAsync();
            }
            await Task.Yield();
        }

        public async Task PublishServiceBusMessageAsync(Message message)
        {
            await _topicClient.SendAsync(message);
        }
    }

    public static class LargePayloadExtensions
    {
        private static BlobClient Blob(BlobContainerClient client, string blobName)
        {
            return client
                .GetBlobClient(blobName);
        }
        public static async Task<Message> ConvertToLargeMessage<T>(this T request, CommandStatus status, string action, BlobContainerClient client, Guid? correlationId = null, Dictionary<string, object> userProperties = null)
        {
            Message message = null;
            var commandModel = new CommandProcessEvent<T>()
            {
                Status = status,
                Action = action,
                CorrelationId = correlationId.HasValue ? correlationId.Value : Guid.NewGuid()
            };
            var json = JsonConvert.SerializeObject(request);
            var bytes = Encoding.UTF8.GetBytes(json);
            var blob = await Blob(client, commandModel.RecordId.ToString().Replace("-",String.Empty)).UploadAsync(BinaryData.FromBytes(bytes), overwrite: true);
            var response = blob.GetRawResponse();
            if (response.IsError)
            {
                throw new BlobStorageException("Error persisting package result to blob storage")
                {
                    Content = response.Content.ToString()
                };
            }
            if (commandModel.EntityReferences == null)
            {
                commandModel.EntityReferences = new List<EntityReference>();
            }
            commandModel.EntityReferences.Add(new EntityReference
            {
                ReferenceKey = "blobName",
                ReferenceValue = commandModel.RecordId.ToString().Replace("-", String.Empty)
            });
            
            Byte[] buffer = null;
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
            };

            string serialized = JsonConvert.SerializeObject(commandModel, jsonSettings);

            buffer = Encoding.UTF8.GetBytes(serialized);

            message = new Message(buffer);

            if (userProperties != null)
            {
                userProperties.Keys.ToList().ForEach(key =>
                {
                    if (message.UserProperties.ContainsKey(key))
                    {
                        message.UserProperties[key] = userProperties[key];
                    }
                    else
                    {
                        message.UserProperties.Add(key, userProperties[key]);
                    }
                });
            }

            message.UserProperties.Add("status", status.ToString());
            message.UserProperties.Add("action", action);
            message.UserProperties.Add("islargrequest", true);
            message.CorrelationId = commandModel.CorrelationId.ToString();

            return message;
        }
        public static async Task<T> GetPaylodFromBlobLargeMessage<T>(this string referenceId, BlobContainerClient client)
        {
            var blob = await Blob(client, referenceId).DownloadContentAsync();
            var result = JsonConvert.DeserializeObject<T>(blob.Value.Content.ToString());
            return result;
        }
    }
}
