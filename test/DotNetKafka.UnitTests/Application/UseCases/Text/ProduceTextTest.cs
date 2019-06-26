using DotNetKafka.Application;
using DotNetKafka.Application.Common;
using DotNetKafka.Application.UseCases.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Application.UseCases.Text
{
    public class ProduceTextTest
    {
        [Fact(DisplayName = "ExecuteAsync: should produce text messages")]
        public void ExecuteAsyncShouldProduceTextMessages()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<ApplicationSettings>>();
            optionsMock.SetupGet(options => options.Value)
                .Returns(new ApplicationSettings
                {
                    ElementsToProduce = 5
                });

            var agnosticProducerMock = new Mock<IAgnosticProducer<string>>();
            var loggerMock = new Mock<ILogger<ProduceText>>();

            var produceTextUseCase = new ProduceText(agnosticProducerMock.Object, optionsMock.Object, loggerMock.Object);

            // Act
            produceTextUseCase.ExecuteAsync().Wait();

            // Assert
            agnosticProducerMock.Verify(agnosticProducer =>
                agnosticProducer.ProduceAsync("text", It.IsAny<AgnosticMessage<string>>()),
                Times.Exactly(5));
        }
    }
}