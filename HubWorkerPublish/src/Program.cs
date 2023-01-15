using Hubs;
using HubWorkerPublish;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.ServiceBus;

try
{
    var builder = WebApplication.CreateBuilder(args);

    IServiceProvider provider = builder.Services.BuildServiceProvider();
    var config = provider.GetRequiredService<IConfiguration>();

    string signalRConnectionString = config.GetConnectionString("SignalR");

    builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);
    builder.Services.AddLogging();

    string serviceBusConnectionString = config.GetConnectionString("ServiceBus");
    ISubscriptionClient subscriptionClient = new SubscriptionClient(serviceBusConnectionString, "eventstore", "push-notifications");


    builder.Services.AddSingleton<EventHub>();
    builder.Services.AddSingleton(subscriptionClient);

    builder.Services.AddHostedService<Worker>();

    var app = builder.Build();
    app.MapHub<EventHub>("/commandevents");
    await app.RunAsync();
}
catch (Exception ex)
{

}

//app.Run();
