using DotNetKafka.Application;
using DotNetKafka.Application.Common;
using DotNetKafka.Application.UseCases.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Application.UseCases.Json
{
    public class ProduceJsonTest
    {
        [Fact(DisplayName = "ExecuteAsync: should produce JSON messages")]
        public void ExecuteAsyncShouldProduceJsonMessages()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<ApplicationSettings>>();
            optionsMock.SetupGet(options => options.Value)
                .Returns(new ApplicationSettings
                {
                    ElementsToProduce = 5
                });

            var agnosticProducerMock = new Mock<IAgnosticProducer<string>>();
            var loggerMock = new Mock<ILogger<ProduceJson>>();

            var produceJsonUseCase = new ProduceJson(agnosticProducerMock.Object, optionsMock.Object, loggerMock.Object);

            // Act
            produceJsonUseCase.ExecuteAsync().Wait();

            // Assert
            agnosticProducerMock.Verify(agnosticProducer =>
                agnosticProducer.ProduceAsync("json", It.IsAny<AgnosticMessage<string>>()),
                Times.Exactly(5));
        }
    }
}