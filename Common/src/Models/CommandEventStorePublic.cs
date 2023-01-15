using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace EGD.Command.Models
{
    public class CommandEventStorePublic : CommandEventStore
    {
        [BsonId]
        [JsonIgnore]
        public object _id { get; set; }
    }
}
