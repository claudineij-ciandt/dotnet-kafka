using System;
using DotNetKafka.Application.Common;
using DotNetKafka.Application.UseCases.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Application.UseCases.Json
{
    public class ConsumeJsonTest
    {
        [Fact(DisplayName = "Execute: should consume from 'json' topic")]
        public void ExecuteShouldConsumeFromJsonTopic()
        {
            // Arrange
            var agnosticConsumerMock = new Mock<IAgnosticConsumer<string>>();
            var loggerMock = new Mock<ILogger<ConsumeJson>>();

            var consumeJsonUseCase = new ConsumeJson(agnosticConsumerMock.Object, loggerMock.Object);

            // Act
            consumeJsonUseCase.Execute();

            // Assert
            agnosticConsumerMock.Verify(agnosticConsumer => agnosticConsumer.Consume("json", It.IsAny<Action<AgnosticMessage<string>>>()));
        }

        // In this example, the consuming action is a simple Console.WriteLine.
        // Therefore, we don't need any other test here.
    }
}