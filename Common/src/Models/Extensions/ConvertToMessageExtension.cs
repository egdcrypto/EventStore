using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using EGD.Command.Models.Enums;
using Microsoft.Azure.ServiceBus;

namespace EGD.Command.Models.Extensions
{
    public static class ConvertToMessageExtension
    {
        public static Message ConvertToMessage<T>(this T request, CommandStatus status, string action, Guid? correlationId = null, Dictionary<string, object> userProperties = null, List<EntityReference> entityReferences = null, Guid? batchId = null)

        {
            var commandModel = new CommandProcessEvent<T>()
            {
                ProcessData = request,
                Status = status,
                Action = action,
                CorrelationId = correlationId.HasValue ? correlationId.Value : Guid.NewGuid(),
                EntityReferences = entityReferences,
                BatchId = batchId.HasValue ? batchId.Value : Guid.NewGuid()
            };

            var buffer = commandModel.GetBuffer();
            var message = new Message(buffer);
            message.UserProperties.AddMany(userProperties);
            message.UserProperties.UpsertProperty("status", status.ToString());
            message.UserProperties.UpsertProperty("action", action);
            message.CorrelationId = commandModel.CorrelationId.ToString();

            return message;
        }

        private static byte[] GetBuffer<T>(this CommandProcessEvent<T> command)
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
            };

            string serialized = JsonConvert.SerializeObject(command, jsonSettings);

            return Encoding.UTF8.GetBytes(serialized);
        }

        private static void AddMany(this IDictionary<string, object> props, IDictionary<string, object> newProps)
        {
            if (newProps == null) {
                return;
            }

            foreach (var prop in newProps)
            {
                props.UpsertProperty(prop.Key, prop.Value);
            }
        }

        private static void UpsertProperty(this IDictionary<string, object> userProps, string key, object value)
        {
            var propertyAlreadyExists = userProps.ContainsKey(key);
            if (propertyAlreadyExists)
            {
                userProps[key] = value;
            }
            else
            {
                userProps.Add(key, value);
            }
        }
    }
}