using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;

namespace DotNetKafka.Infrastructure.Kafka
{
    public class KafkaProducerWrapper<TPayload> : IAgnosticProducer<TPayload>
    {
        private readonly IProducer<string, TPayload> kafkaProducer;
        private readonly ILogger<KafkaProducerWrapper<TPayload>> logger;

        public KafkaProducerWrapper(IProducer<string, TPayload> kafkaProducer, ILogger<KafkaProducerWrapper<TPayload>> logger)
        {
            this.kafkaProducer = kafkaProducer;
            this.logger = logger;
        }

        public async Task ProduceAsync(string topic, AgnosticMessage<TPayload> message)
        {
            var kafkaMessage = MapFromAgnosticMessage(message);

            try
            {
                var result = await kafkaProducer.ProduceAsync(topic, kafkaMessage);
                logger.LogInformation($"Message produced to: {result.TopicPartitionOffset}");
            }
            catch (ProduceException<string, TPayload> ex)
            {
                logger.LogError($"Failed to produce message: {ex.Error.Reason}");
                throw;
            }
        }

        private Message<string, TPayload> MapFromAgnosticMessage(AgnosticMessage<TPayload> agnosticMessage)
        {
            return new Message<string, TPayload>
            {
                Key = agnosticMessage.Key,
                Value = agnosticMessage.Payload
            };
        }

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
                kafkaProducer.Dispose();
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