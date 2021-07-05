using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

[assembly: FunctionsStartup(typeof(AzureFunctionsMongoDB.Startup))]
namespace AzureFunctionsMongoDB
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // need this to just get the MongoDB CONNECTION_STRING from local.settings.json
            var startupConfiguration = builder.GetContext().Configuration;
            var startupMongoOptions = new MongoDBOptions();
            startupConfiguration.GetSection("MongoDBOptions").Bind(startupMongoOptions);

            // extract values from the IConfiguration instance into a custom type
            // https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#working-with-options-and-settings
            builder.Services.AddOptions<MongoDBOptions>().Configure<IConfiguration>((mongoOptions, configuration) =>
            {
                // Calling Bind copies values that have matching property names from the configuration into the custom instance.
                // The options instance is now available in the IoC container to inject into a function.
                configuration.GetSection("MongoDBOptions").Bind(mongoOptions);
            });


            builder.Services.AddSingleton<MongoClient>((s) =>
            {
                return new MongoClient(startupMongoOptions.CONNECTION_STRING);

            });

        }
    }
}
