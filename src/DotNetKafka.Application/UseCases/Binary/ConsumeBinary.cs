using System;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetKafka.Application.UseCases.Binary
{
    public class ConsumeBinary : IConsumeBinary
    {
        private readonly IAgnosticConsumer<byte[]> consumer;
        private readonly IFileClient fileClient;
        private readonly IOptions<ApplicationSettings> settings;
        private readonly ILogger<ConsumeBinary> logger;

        public ConsumeBinary(IAgnosticConsumer<byte[]> consumer, IFileClient fileClient, IOptions<ApplicationSettings> settings, ILogger<ConsumeBinary> logger)
        {
            this.consumer = consumer;
            this.fileClient = fileClient;
            this.settings = settings;
            this.logger = logger;
        }

        public void Execute()
        {
            consumer.Consume("binary", message =>
            {
                var fileName = message.Key;
                fileClient.Create(settings.Value.OutputDirectory, fileName, message.Payload);

                logger.LogInformation($"Binary file received and created: {fileName}");
            });
        }
    }
}