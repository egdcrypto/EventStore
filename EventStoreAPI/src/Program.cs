using EventStoreAPI.Context;
using EGD.Command.Models;
using EventStoreAPI.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var config = builder.Configuration;

string databaseName = config.GetValue<string>("DatabaseName");
string mongoDbConnectionString = config.GetConnectionString("EventStoreMongoDB");
IMongoClient client = new MongoClient(mongoDbConnectionString);


var collection = client.GetDatabase(databaseName).GetCollection<CommandEventStorePublic>("EventStore");
var bsonCollection = client.GetDatabase(databaseName).GetCollection<BsonDocument>("EventStore");

BsonDocument result = null;
//await client.GetDatabase(databaseName).CreateCollectionAsync("EventStore");
var createIndexesStr = "{ createIndexes: 'EventStore', indexes: [ { key: { Action: 1 }, name: 'Action', unique: false },{ key: { CorrelationId: 1 }, name: 'CorrelationId', unique: false }, { key: { Status: 1 }, name: 'Status', unique: false }, { key: { Timestamp: 1 }, name: 'Timestamp', unique: false }, { key: { Action: 1, CorrelationId: 1, Status:1 }, name: 'ActionCorrelationIdStatus', unique: false } ] }";
result = client.GetDatabase(databaseName).RunCommand<BsonDocument>(createIndexesStr);

builder.Services.AddSingleton(collection);
builder.Services.AddSingleton(bsonCollection);
builder.Services.AddSingleton<IDataContext, DataContext>();
builder.Services.AddSingleton<ICommandEventRepository, CommandEventRepository>();



builder.Services.AddLogging();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
