using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Integrations.Messaging.Extensions
{
    public static class ServiceCollectionExtensions
    {        
        public static void ConfigureEventStoreCollection(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MongoDB");
            var client = new MongoClient(connectionString);

            var databaseName = config.GetValue<string>("DatabaseName");
            var database = client.GetDatabase(databaseName);
            
            var createIndexesStr = "{ createIndexes: 'EventStore', indexes: [ { key: { Action: 1 }, name: 'Action', unique: false },{ key: { CorrelationId: 1 }, name: 'CorrelationId',        unique:     false }, { key: { Status: 1 }, name: 'Status', unique: false }, { key: { Timestamp: 1 }, name: 'Timestamp', unique: false }, { key: { Action: 1,    CorrelationId: 1,  Status:1 },  name: 'ActionCorrelationIdStatus', unique: false } ] }";
            database.RunCommand<BsonDocument>(createIndexesStr);

            var collection = database.GetCollection<BsonDocument>("EventStore");
            services.AddTransient(_ => { return collection; });
        }

        public static void ConfigureSubscriptionClient(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("ServiceBus");
            var topicName = config.GetValue<string>("TopicName");
            var subscriptionClient = new SubscriptionClient(connectionString, topicName, "all", TokenProvider.CreateManagedIdentityTokenProvider());
            services.AddSingleton<ISubscriptionClient>(subscriptionClient);
        }
    }
}
