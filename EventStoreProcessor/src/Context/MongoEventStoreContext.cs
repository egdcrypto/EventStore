using EGD.Command.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Integrations.Messaging.Context
{
    public class MongoEventStoreContext : IEventStoreContext
    {
        private readonly IMongoCollection<BsonDocument> _eventStoreCollection;
        private readonly ILogger<MongoEventStoreContext> _logger;
        private readonly StringEnumConverter _stringEnumConverter;
        public MongoEventStoreContext(ILogger<MongoEventStoreContext> logger, IMongoCollection<BsonDocument> eventStoreCollection)
        {
            _eventStoreCollection = eventStoreCollection;
            _logger = logger;
            _stringEnumConverter = new StringEnumConverter();
            var pack = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreIfNullConvention(true)
            };

            ConventionRegistry.Register("EnumStringConvention", pack, t => true);
        }

        public async Task InsertOneAsync(CommandEventStore commandEvent)
        {
            if (commandEvent == null)
            {
                throw new ArgumentNullException("commandEvent", "The parameter 'commandEvent' cannot be null.");
            };

            var json = JsonConvert.SerializeObject(commandEvent, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new JsonConverter[] { _stringEnumConverter }
            });

            BsonDocument document = BsonSerializer.Deserialize<BsonDocument>(json);
            await _eventStoreCollection.InsertOneAsync(document);
        }

    }


}
