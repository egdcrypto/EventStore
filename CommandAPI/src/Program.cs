using Azure.Storage.Blobs;
using CommandAPI.Middleware;
using CommandAPI.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var config = builder.Configuration;
string serviceBusConnectionString = config.GetConnectionString("POCServiceBus");
string blobConnectionString = config.GetConnectionString("Blob");
string blobContainerName = config.GetValue<string>("BlobContainerName");

builder.Services.AddScoped((s) => {
    ITopicClient topicClient = new TopicClient(serviceBusConnectionString, "cqrs");
    return topicClient;
});
builder.Services.AddScoped<IPublishMessageRepo, PublishMessageRepo>();

BlobContainerClient blobContainerClient = new BlobContainerClient(blobConnectionString, blobContainerName);

builder.Services.AddSingleton(blobContainerClient);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
});
builder.Services.AddMvc().AddNewtonsoftJson();

builder.Services.AddCorrelationIdGenerator();
//builder.Services.AddSwaggerGen(opt =>
//{
//    opt.SchemaFilter<SwaggerExcludePropertySchemaFilter>();
//});

builder.Services.AddCors();

builder.Services.AddLogging();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler(c => c.Run(async context =>
{
    var exception = context.Features
        .Get<IExceptionHandlerPathFeature>()
        .Error;
    //var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = 400;
    var serializedError = Newtonsoft.Json.JsonConvert.SerializeObject(exception);
    byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(serializedError);
    await context.Response.Body.ReadAsync(buffer);
    //context.Response
    //await context.Response.WriteAsJsonAsync(serializedError);
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.AddCorrelationIdMiddleware();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
