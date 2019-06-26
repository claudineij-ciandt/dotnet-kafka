using System.Collections.Generic;
using DotNetKafka.Application;
using DotNetKafka.Application.Common;
using DotNetKafka.Application.UseCases.Binary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Application.UseCases.Binary
{
    public class ProduceBinaryTest
    {
        [Fact(DisplayName = "ExecuteAsync: should produce binary messages")]
        public void ExecuteAsyncShouldProduceBinaryMessages()
        {
            // Arrange
            var inputDirectory = "/my-var";

            var optionsMock = new Mock<IOptions<ApplicationSettings>>();
            optionsMock.SetupGet(options => options.Value)
                .Returns(new ApplicationSettings
                {
                    InputDirectory = inputDirectory
                });

            var agnosticProducerMock = new Mock<IAgnosticProducer<byte[]>>();
            var loggerMock = new Mock<ILogger<ProduceBinary>>();

            var fileClientMock = new Mock<IFileClient>();
            fileClientMock.Setup(fileClient => fileClient.GetFiles(inputDirectory))
                .Returns(new List<string> { "file1", "file2", "file3" });

            var produceBinaryUseCase = new ProduceBinary(agnosticProducerMock.Object, fileClientMock.Object, optionsMock.Object, loggerMock.Object);

            // Act
            produceBinaryUseCase.ExecuteAsync().Wait();

            // Assert
            agnosticProducerMock.Verify(agnosticProducer =>
                agnosticProducer.ProduceAsync("binary", It.IsAny<AgnosticMessage<byte[]>>()),
                Times.Exactly(3));
        }
    }
}