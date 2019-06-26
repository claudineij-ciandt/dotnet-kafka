using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetKafka.Infrastructure.Logging
{
    public static class LoggingBootstrapper
    {
        public static IServiceCollection AddLoggingProviders(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(builder => builder.AddConsole());

            return serviceCollection;
        }
    }
}