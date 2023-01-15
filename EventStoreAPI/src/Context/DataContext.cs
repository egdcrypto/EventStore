using EGD.Command.Models;
using EGD.Command.Models.Enums;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;

namespace EventStoreAPI.Context
{
    public class DataContext : IDataContext
    {
        private readonly IMongoCollection<CommandEventStorePublic> _eventStoreCollection;
        private readonly IMongoCollection<BsonDocument> _bsonEventStoreCollection;
        private readonly ILogger<DataContext> _logger;
        private SortDefinition<CommandEventStorePublic> _ascSort;
        private SortDefinition<BsonDocument> _bsonAscSort;
        private SortDefinition<CommandEventStorePublic> _descSort;
        public DataContext(ILogger<DataContext> logger, IMongoCollection<CommandEventStorePublic> eventStoreCollection, IMongoCollection<BsonDocument> bsonEventStoreCollection)
        {
            _bsonEventStoreCollection = bsonEventStoreCollection;
            _eventStoreCollection = eventStoreCollection;
            _logger = logger;
            _ascSort = Builders<CommandEventStorePublic>.Sort.Ascending(x => x.Timestamp);
            _descSort = Builders<CommandEventStorePublic>.Sort.Descending(x => x.Timestamp);
            _bsonAscSort = Builders<BsonDocument>.Sort.Ascending("Timestamp");
        }
        public async Task<List<CommandEventStorePublic>> Get(string action, Guid? correlationId = null, CommandStatus? status = null, int? limit = null, int? skip = null)
        {
            //TODO:  need to work on aggregate here to build and retrieve TotalCount, Records
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentNullException("action");
            }
            var filter = Builders<BsonDocument>.Filter.Eq("Action", action);

            if (correlationId.HasValue)
            {
                filter = filter & Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId.Value.ToString());
            }
            if (status.HasValue)
            {
                filter = filter & Builders<BsonDocument>.Filter.Eq("Status", status.ToString());
            }

            FindOptions<BsonDocument> findOptions = new FindOptions<BsonDocument>()
            {
                Limit = limit.HasValue ? limit.Value : 10,
                Skip = skip.HasValue ? skip.Value : 0
            };
            var results = (await _bsonEventStoreCollection.FindAsync(filter, findOptions)).ToList();

            if (results == null)
            {
                return null;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<CommandEventStorePublic>>(results.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson }));
        }
        public async Task<List<CommandEventStorePublic>> Get(Guid? correlationId, int? limit = null, int? skip = null)
        {
            if (!correlationId.HasValue)
            {
                throw new ArgumentNullException("correlationId");
            }
            var filterId = Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId.Value.ToString());
            FindOptions<BsonDocument> findOptions = new FindOptions<BsonDocument>()
            {
                Limit = limit,
                Skip = skip,
                Sort = _bsonAscSort
            };
            var results = (await _bsonEventStoreCollection.FindAsync(filterId, findOptions)).ToList();

            if (results == null)
            {
                return null;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<CommandEventStorePublic>>(results.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson }));
        }
        public async Task<CommandEventStorePublic> GetLatest(Guid? correlationId)
        {
            var limit = 1;
            var skip = 0;
            if (!correlationId.HasValue)
            {
                throw new ArgumentNullException("correlationId");
            }
            var filterId = Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId.Value.ToString());

            FindOptions<BsonDocument> findOptions = new FindOptions<BsonDocument>()
            {
                Limit = limit,
                Skip = skip,
                Sort = _bsonAscSort
            };
            var results = (await _bsonEventStoreCollection.FindAsync(filterId, findOptions)).SingleOrDefault();

            if (results == null)
            {
                return null;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<CommandEventStorePublic>(results.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson }));
        }
        public async Task<CommandEventStorePublic> GetEarliest(Guid? correlationId)
        {
            var limit = 1;
            var skip = 0;
            if (!correlationId.HasValue)
            {
                throw new ArgumentNullException("correlationId");
            }
            var filterId = Builders<CommandEventStorePublic>.Filter.Eq("CorrelationId", correlationId.Value.ToString());

            FindOptions<CommandEventStorePublic> findOptions = new FindOptions<CommandEventStorePublic>()
            {
                Limit = limit,
                Skip = skip,
                Sort = _descSort
            };
            var results = (await _eventStoreCollection.FindAsync(filterId, findOptions)).SingleOrDefault();

            return results;
        }
        public async Task<List<CommandEventSummary>> GroupByActionStatus(Guid? correlationId)
        {
            //TODO: look into group by mongo query instead of returning all results and then grouping
            if (!correlationId.HasValue)
            {
                throw new ArgumentNullException("correlationId");
            }
            var filterId = Builders<BsonDocument>.Filter.Eq("CorrelationId", correlationId.Value.ToString());
           
            var query = (await _bsonEventStoreCollection.FindAsync(filterId)).ToList();

            var results =  query.GroupBy(g => new { Action = g["Action"].ToString(), Status = Enum.Parse<CommandStatus>(g["Status"].ToString()) }).Select(s => new CommandEventSummary
            {
                Action = s.Key.Action,
                Status = s.Key.Status,
                Correlationid = correlationId.Value,
                Count = s.Count()
            }).ToList();

            return results;
        }
    }

   
}
