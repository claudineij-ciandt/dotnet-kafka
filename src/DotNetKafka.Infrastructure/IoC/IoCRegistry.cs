using DotNetKafka.Application.Common;
using DotNetKafka.Application.UseCases.Binary;
using DotNetKafka.Application.UseCases.Json;
using DotNetKafka.Application.UseCases.Text;
using DotNetKafka.Infrastructure.File;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetKafka.Infrastructure.IoC
{
    public static class IoCRegistry
    {
        public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IFileClient, FileClient>()
                .AddTransient<IProduceText, ProduceText>()
                .AddTransient<IConsumeText, ConsumeText>()
                .AddTransient<IProduceJson, ProduceJson>()
                .AddTransient<IConsumeJson, ConsumeJson>()
                .AddTransient<IProduceBinary, ProduceBinary>()
                .AddTransient<IConsumeBinary, ConsumeBinary>();

            return serviceCollection;
        }
    }
}