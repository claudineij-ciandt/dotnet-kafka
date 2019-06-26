using System;
using DotNetKafka.Application;
using DotNetKafka.Application.Common;
using DotNetKafka.Application.UseCases.Binary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DotNetKafka.UnitTests.Application.UseCases.Binary
{
    public class ConsumeBinaryTest
    {
        [Fact(DisplayName = "Execute: should consume from 'binary' topic")]
        public void ExecuteShouldConsumeFromBinaryTopic()
        {
            // Arrange
            var outputDirectory = "/my-var";

            var optionsMock = new Mock<IOptions<ApplicationSettings>>();
            optionsMock.SetupGet(options => options.Value)
                .Returns(new ApplicationSettings
                {
                    OutputDirectory = outputDirectory
                });

            var agnosticConsumerMock = new Mock<IAgnosticConsumer<byte[]>>();
            var fileClientMock = new Mock<IFileClient>();
            var loggerMock = new Mock<ILogger<ConsumeBinary>>();

            var consumeBinaryUseCase = new ConsumeBinary(agnosticConsumerMock.Object, fileClientMock.Object, optionsMock.Object, loggerMock.Object);

            // Act
            consumeBinaryUseCase.Execute();

            // Assert
            agnosticConsumerMock.Verify(agnosticConsumer => agnosticConsumer.Consume("binary", It.IsAny<Action<AgnosticMessage<byte[]>>>()));
        }

        [Fact(DisplayName = "Execute: should create a file when a message is consumed")]
        public void ExecuteShouldCreateAFileWhenAMessageIsConsumed()
        {
            // Arrange
            var outputDirectory = "/my-var";

            var optionsMock = new Mock<IOptions<ApplicationSettings>>();
            optionsMock.SetupGet(options => options.Value)
                .Returns(new ApplicationSettings
                {
                    OutputDirectory = outputDirectory
                });

            var consumedMessageMock = new AgnosticMessage<byte[]>
            {
                Key = "file1",
                Payload = new byte[] {}
            };

            var agnosticConsumerMock = new Mock<IAgnosticConsumer<byte[]>>();
            agnosticConsumerMock.Setup(a => a.Consume("binary", It.IsAny<Action<AgnosticMessage<byte[]>>>()))
                .Callback<string, Action<AgnosticMessage<byte[]>>>((_, action) =>
                {
                    action(consumedMessageMock);
                });

            var fileClientMock = new Mock<IFileClient>();
            var loggerMock = new Mock<ILogger<ConsumeBinary>>();

            var consumeBinaryUseCase = new ConsumeBinary(agnosticConsumerMock.Object, fileClientMock.Object, optionsMock.Object, loggerMock.Object);

            // Act
            consumeBinaryUseCase.Execute();

            // Assert
            fileClientMock.Verify(fileClient =>
                fileClient.Create(outputDirectory, consumedMessageMock.Key, consumedMessageMock.Payload),
                Times.Once);
        }
    }
}