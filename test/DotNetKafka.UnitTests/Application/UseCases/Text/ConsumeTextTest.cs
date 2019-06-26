using System;
using DotNetKafka.Application.Common;
using DotNetKafka.Application.UseCases.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Application.UseCases.Text
{
    public class ConsumeTextTest
    {
        [Fact(DisplayName = "Execute: should consume from 'text' topic")]
        public void ExecuteShouldConsumeFromTextTopic()
        {
            // Arrange
            var agnosticConsumerMock = new Mock<IAgnosticConsumer<string>>();
            var loggerMock = new Mock<ILogger<ConsumeText>>();

            var consumeTextUseCase = new ConsumeText(agnosticConsumerMock.Object, loggerMock.Object);

            // Act
            consumeTextUseCase.Execute();

            // Assert
            agnosticConsumerMock.Verify(agnosticConsumer => agnosticConsumer.Consume("text", It.IsAny<Action<AgnosticMessage<string>>>()));
        }

        // In this example, the consuming action is a simple Console.WriteLine.
        // Therefore, we don't need any other test here.
    }
}