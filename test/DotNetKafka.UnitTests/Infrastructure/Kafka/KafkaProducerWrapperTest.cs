using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotNetKafka.Application.Common;
using DotNetKafka.Infrastructure.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Infrastructure.Kafka
{
    public class KafkaProducerWrapperTest
    {
        [Fact(DisplayName = "ProduceAsync: should send message to Kafka topic")]
        public void ProduceAsyncShouldSendMessageToKafkaTopic()
        {
            // Arrange
            var topic = "my-topic";

            var message = new AgnosticMessage<string>
            {
                Key = "msg-id",
                Payload = "msg-content"
            };

            var producerMock = new Mock<IProducer<string, string>>();
            producerMock.Setup(producer => producer.ProduceAsync(topic, It.Is<Message<string, string>>(msg => msg.Key == message.Key)))
                .ReturnsAsync(new DeliveryResult<string, string>());

            var loggerMock = new Mock<ILogger<KafkaProducerWrapper<string>>>();

            // Act
            using (var kafkaProducerWrapper = new KafkaProducerWrapper<string>(producerMock.Object, loggerMock.Object))
            {
                kafkaProducerWrapper.ProduceAsync(topic, message).Wait();
            }

            // Assert
            producerMock.Verify(producer =>
                producer.ProduceAsync(topic, It.Is<Message<string, string>>(msg => msg.Key == message.Key)),
                Times.Once);
        }

        [Fact(DisplayName = "ProduceAsync: should throw ProducerException")]
        public void ProduceAsyncShouldRethrowProducerException()
        {
            // Arrange
            var topic = "my-topic";

            var message = new AgnosticMessage<string>
            {
                Key = "msg-id",
                Payload = "msg-content"
            };

            var producerMock = new Mock<IProducer<string, string>>();
            producerMock.Setup(producer => producer.ProduceAsync(topic, It.Is<Message<string, string>>(msg => msg.Key == message.Key)))
                .ThrowsAsync(new ProduceException<string, string>(new Error(ErrorCode.BrokerNotAvailable), null));

            var loggerMock = new Mock<ILogger<KafkaProducerWrapper<string>>>();

            using (var kafkaProducerWrapper = new KafkaProducerWrapper<string>(producerMock.Object, loggerMock.Object))
            {
                // Act
                Func<Task> methodCall = () => kafkaProducerWrapper.ProduceAsync(topic, message);

                // Assert
                methodCall.Should().Throw<ProduceException<string, string>>();
            }
        }
    }
}