using Confluent.Kafka;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetKafka.Infrastructure.Kafka
{
    public static class KafkaBootstrapper
    {
        public static IServiceCollection AddKafka(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddKafkaProducer<string>()
                .AddKafkaConsumer<string>()
                .AddKafkaProducer<byte[]>()
                .AddKafkaConsumer<byte[]>();

            return serviceCollection;
        }

        public static IServiceCollection AddKafkaProducer<TPayload>(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IAgnosticProducer<TPayload>, KafkaProducerWrapper<TPayload>>()
                .AddSingleton<IProducer<string, TPayload>>(sp =>
                {
                    var options = sp.GetService<IOptions<InfrastructureSettings>>();

                    var producerConfiguration = new ProducerConfig
                    {
                        BootstrapServers = options.Value.KafkaServers,
                        MessageTimeoutMs = 5000,
                        Partitioner = Partitioner.Consistent,
                        EnableIdempotence = true,
                        MessageSendMaxRetries = 3
                    };

                    return new ProducerBuilder<string, TPayload>(producerConfiguration).Build();
                });

            return serviceCollection;
        }

        public static IServiceCollection AddKafkaConsumer<TPayload>(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IAgnosticConsumer<TPayload>, KafkaConsumerWrapper<TPayload>>()
                .AddSingleton<IConsumer<string, TPayload>>(sp =>
                {
                    var options = sp.GetService<IOptions<InfrastructureSettings>>();

                    var consumerConfiguration = new ConsumerConfig
                    {
                        GroupId = options.Value.ConsumerGroup,
                        BootstrapServers = options.Value.KafkaServers,
                        EnablePartitionEof = true,
                        AutoOffsetReset = AutoOffsetReset.Earliest
                    };

                    return new ConsumerBuilder<string, TPayload>(consumerConfiguration).Build();
                });

            return serviceCollection;
        }
    }
}