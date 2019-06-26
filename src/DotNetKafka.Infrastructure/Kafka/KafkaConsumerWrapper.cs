using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;
using Polly;

namespace DotNetKafka.Infrastructure.Kafka
{
    public class KafkaConsumerWrapper<TPayload> : IAgnosticConsumer<TPayload>
    {
        private readonly IConsumer<string, TPayload> kafkaConsumer;
        private readonly IProducer<string, TPayload> kafkaProducer;
        private readonly ILogger<KafkaConsumerWrapper<TPayload>> logger;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly ISyncPolicy resiliencePolicy;

        public KafkaConsumerWrapper(IConsumer<string, TPayload> kafkaConsumer, IProducer<string, TPayload> kafkaProducer, ILogger<KafkaConsumerWrapper<TPayload>> logger)
        {
            this.kafkaConsumer = kafkaConsumer;
            this.kafkaProducer = kafkaProducer;
            this.logger = logger;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.resiliencePolicy = CreateResiliencePolicy();
        }

        public void Consume(string topic, Action<AgnosticMessage<TPayload>> processingAction)
        {
            kafkaConsumer.Subscribe(topic);
            RegisterConsoleCancelKeyPressEvent();

            try
            {
                while (true)
                {
                    var consumeResult = kafkaConsumer.Consume(cancellationTokenSource.Token);

                    if (!consumeResult.IsPartitionEOF)
                    {
                        ProcessMessage(consumeResult, processingAction);
                    }
                }
            }
            catch (ConsumeException ex)
            {
                logger.LogError($"Failed to consume message: {ex.Error.Reason}");
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation($"Gracefully stopping the consumer..");
                kafkaConsumer.Close(); // Ensure the consumer leaves the group cleanly and final offsets are committed.
            }
        }

        private void ProcessMessage(ConsumeResult<string, TPayload> consumeResult, Action<AgnosticMessage<TPayload>> processingAction)
        {
            var executionContext = CreateNewExecutionContext(consumeResult);
            resiliencePolicy.Execute((context) =>
            {
                var message = MapFromKafkaMessage(consumeResult);
                processingAction(message);

                logger.LogInformation($"Consumed message successfully processed: {consumeResult.TopicPartitionOffset}");
            }, executionContext);
        }


        private ISyncPolicy CreateResiliencePolicy()
        {
            var maxRetryAttempts = 3;

            void OnRetry(Exception exception, TimeSpan retryInterval, int retryAttempt, Context executionContext)
            {
                logger.LogWarning($"Failed to process a consumed message (attempt {retryAttempt}/{maxRetryAttempts}): {exception.Message}");
            }

            TimeSpan RetryIntervalProvider(int retryAttempt)
            {
                return TimeSpan.FromMilliseconds(100 * retryAttempt);
            }

            void FallbackAction(Context context)
            {
                logger.LogWarning("All retry attempts failed. Sending message do dead-letter topic..");

                var consumeResult = context["consume-result"] as ConsumeResult<string, TPayload>;
                var deadLetterTopic = $"_dead-letter_{consumeResult.Topic}";
                var produceResult = kafkaProducer.ProduceAsync(deadLetterTopic, consumeResult.Message).Result;

                logger.LogInformation($"Message produced to dead-letter: {produceResult.TopicPartitionOffset}");
            }

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(maxRetryAttempts, RetryIntervalProvider, OnRetry);

            var fallbackPolicy = Policy
                .Handle<Exception>()
                .Fallback(FallbackAction, (e, c) => {});

            return Policy.Wrap(fallbackPolicy, retryPolicy);
        }

        private void RegisterConsoleCancelKeyPressEvent()
        {
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cancellationTokenSource.Cancel();
            };

            logger.LogInformation($"Started consumer, press Ctrl-C to stop consuming");
        }

        private AgnosticMessage<TPayload> MapFromKafkaMessage(ConsumeResult<string, TPayload> kafkaMessage)
        {
            return new AgnosticMessage<TPayload>
            {
                Key = kafkaMessage.Key,
                Payload = kafkaMessage.Value
            };
        }

        private Context CreateNewExecutionContext(ConsumeResult<string, TPayload> consumeResult) =>
            new Context
            {
                { "consume-result", consumeResult }
            };

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
            {
                return;
            }

            if (disposing)
            {
                kafkaConsumer.Dispose();
                cancellationTokenSource.Dispose();
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}