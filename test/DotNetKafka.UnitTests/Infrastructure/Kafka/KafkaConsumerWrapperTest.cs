using System;
using System.Threading;
using Confluent.Kafka;
using DotNetKafka.Application.Common;
using DotNetKafka.Infrastructure.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Infrastructure.Kafka
{
    public class KafkaConsumerWrapperTest
    {
        [Fact(DisplayName = "Consume: should subscribe to topic")]
        public void ConsumeShouldSubscribeToTopic()
        {
            // Arrange
            var topic = "my-topic";

            var consumeResultMock = new ConsumeResult<string, string>
            {
                Message = new Message<string, string>()
            };

            // Simulates a consumer being terminated immediately.
            var consumerMock = new Mock<IConsumer<string, string>>();
            consumerMock.SetupSequence(consumer => consumer.Consume(It.IsAny<CancellationToken>()))
                .Returns(consumeResultMock)
                .Throws<OperationCanceledException>();

            var producerMock = new Mock<IProducer<string, string>>();
            var loggerMock = new Mock<ILogger<KafkaConsumerWrapper<string>>>();

            // Act
            using (var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(consumerMock.Object, producerMock.Object, loggerMock.Object))
            {
                kafkaConsumerWrapper.Consume(topic, (AgnosticMessage<string> message) => {});
            }

            // Assert
            consumerMock.Verify(consumer => consumer.Subscribe(topic), Times.Once);
        }

        [Fact(DisplayName = "Consume: should execute action to each consumed message")]
        public void ConsumeShouldExecuteActionToEachConsumedMessage()
        {
            // Arrange
            var topic = "my-topic";

            var consumeResultMock = new ConsumeResult<string, string>
            {
                Message = new Message<string, string>()
            };

            var actionMock = new Mock<Action<AgnosticMessage<string>>>();
            var producerMock = new Mock<IProducer<string, string>>();
            var loggerMock = new Mock<ILogger<KafkaConsumerWrapper<string>>>();

            // Simulates a consumer with 3 messages to be read before being terminated.
            var consumerMock = new Mock<IConsumer<string, string>>();
            consumerMock.SetupSequence(consumer => consumer.Consume(It.IsAny<CancellationToken>()))
                .Returns(consumeResultMock)
                .Returns(consumeResultMock)
                .Returns(consumeResultMock)
                .Throws<OperationCanceledException>();

            // Act
            using (var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(consumerMock.Object, producerMock.Object, loggerMock.Object))
            {
                kafkaConsumerWrapper.Consume(topic, actionMock.Object);
            }

            // Assert
            actionMock.Verify(action => action(It.IsAny<AgnosticMessage<string>>()), Times.Exactly(3));
        }

        [Fact(DisplayName = "Consume: should retry executing action upon failure")]
        public void ConsumeShouldRetryExecutingActionUponFailure()
        {
            // Arrange
            var topic = "my-topic";

            var consumeResultMock = new ConsumeResult<string, string>
            {
                Message = new Message<string, string> { Key = "msg-1" }
            };

            var producerMock = new Mock<IProducer<string, string>>();
            var loggerMock = new Mock<ILogger<KafkaConsumerWrapper<string>>>();

            // Simulates an action that fails twice and executes successfully on the third attempt.
            var actionMock = new Mock<Action<AgnosticMessage<string>>>();
            actionMock.SetupSequence(action => action(It.Is<AgnosticMessage<string>>(message => message.Key == consumeResultMock.Message.Key)))
                .Throws<Exception>()
                .Throws<Exception>();

            // Simulates a consumer with one message to be read before being terminated.
            var consumerMock = new Mock<IConsumer<string, string>>();
            consumerMock.SetupSequence(consumer => consumer.Consume(It.IsAny<CancellationToken>()))
                .Returns(consumeResultMock)
                .Throws<OperationCanceledException>();

            // Act
            using (var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(consumerMock.Object, producerMock.Object, loggerMock.Object))
            {
                kafkaConsumerWrapper.Consume(topic, actionMock.Object);
            }

            // Assert
            actionMock.Verify(action => action(It.IsAny<AgnosticMessage<string>>()), Times.Exactly(3));
        }

        [Fact(DisplayName = "Consume: should send message to dead-letter topic after successive failed retries")]
        public void ConsumeShouldSendMessageToDeadLetterTopicAfterSuccessiveFailedRetries()
        {
            // Arrange
            var topic = "my-topic";
            var deadLetterTopic = "_dead-letter_my-topic";

            var consumeResultMock = new ConsumeResult<string, string>
            {
                Topic = topic,
                Message = new Message<string, string> { Key = "msg-1" }
            };

            var loggerMock = new Mock<ILogger<KafkaConsumerWrapper<string>>>();

            // Simulates an action that always fail.
            var actionMock = new Mock<Action<AgnosticMessage<string>>>();
            actionMock.Setup(action => action(It.Is<AgnosticMessage<string>>(message => message.Key == consumeResultMock.Message.Key)))
                .Throws<Exception>();

            // Simulates a consumer with one message to be read before being terminated.
            var consumerMock = new Mock<IConsumer<string, string>>();
            consumerMock.SetupSequence(consumer => consumer.Consume(It.IsAny<CancellationToken>()))
                .Returns(consumeResultMock)
                .Throws<OperationCanceledException>();

            var producerMock = new Mock<IProducer<string, string>>();
            producerMock.Setup(producer => producer.ProduceAsync(deadLetterTopic, consumeResultMock.Message))
                .ReturnsAsync(new DeliveryResult<string, string>());

            // Act
            using (var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(consumerMock.Object, producerMock.Object, loggerMock.Object))
            {
                kafkaConsumerWrapper.Consume(topic, actionMock.Object);
            }

            // Assert
            actionMock.Verify(action => action(It.IsAny<AgnosticMessage<string>>()), Times.Exactly(4));
            producerMock.Verify(producer => producer.ProduceAsync(deadLetterTopic, consumeResultMock.Message), Times.Once);
        }

        [Fact(DisplayName = "Consume: should stop consuming messages and leave consumer group when process is canceled")]
        public void ConsumeShouldStopConsumingAndLeaveGroupWhenCanceled()
        {
            // Arrange
            var topic = "my-topic";

            var consumeResultMock = new ConsumeResult<string, string>
            {
                Message = new Message<string, string>()
            };

            var actionMock = new Mock<Action<AgnosticMessage<string>>>();
            var producerMock = new Mock<IProducer<string, string>>();
            var loggerMock = new Mock<ILogger<KafkaConsumerWrapper<string>>>();

            // Simulates a consumer being terminated immediately.
            var consumerMock = new Mock<IConsumer<string, string>>();
            consumerMock.SetupSequence(consumer => consumer.Consume(It.IsAny<CancellationToken>()))
                .Throws<OperationCanceledException>();

            // Act
            using (var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(consumerMock.Object, producerMock.Object, loggerMock.Object))
            {
                kafkaConsumerWrapper.Consume(topic, actionMock.Object);
            }

            // Assert
            actionMock.Verify(action => action(It.IsAny<AgnosticMessage<string>>()), Times.Never);
            consumerMock.Verify(consumer => consumer.Close(), Times.Once);
        }
    }
}