using Integrations.Messaging;
using Integrations.Messaging.Context;
using Integrations.Messaging.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configBuilder =>
    {
        var config = configBuilder.Build();
        string keyVaultName = config.GetValue<string>("KeyVaultName");
        if (!string.IsNullOrWhiteSpace(keyVaultName))
        {
            //TODO: need to add vault settings here
            //configBuilder.AddManagedIdentityKeyVault(keyVaultName);
        }
    })
    .ConfigureServices(services =>
    {
        IServiceProvider provider = services.BuildServiceProvider();
        var config = provider.GetRequiredService<IConfiguration>();

        services.AddLogging();

        services.ConfigureSubscriptionClient(config);
        services.ConfigureEventStoreCollection(config);
        services.AddTransient<IEventStoreContext, MongoEventStoreContext>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();