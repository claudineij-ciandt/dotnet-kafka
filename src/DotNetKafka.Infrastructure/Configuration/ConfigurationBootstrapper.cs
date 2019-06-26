using System.Collections.Generic;
using DotNetKafka.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetKafka.Infrastructure.Configuration
{
    public static class ConfigurationBootstrapper
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection, string[] commandLineArgs, Dictionary<string, string> commandLineMappings)
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, false)
                .AddCommandLine(commandLineArgs, commandLineMappings)
                .Build();

            serviceCollection
                .AddOptions()
                .Configure<ApplicationSettings>(configurationRoot.GetSection("application"))
                .Configure<InfrastructureSettings>(configurationRoot.GetSection("infrastructure"));

            return serviceCollection;
        }
    }
}