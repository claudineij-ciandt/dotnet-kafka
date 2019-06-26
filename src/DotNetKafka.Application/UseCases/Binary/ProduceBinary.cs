using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetKafka.Application.UseCases.Binary
{
    public class ProduceBinary : IProduceBinary
    {
        private readonly IAgnosticProducer<byte[]> producer;
        private readonly IFileClient fileClient;
        private readonly IOptions<ApplicationSettings> settings;
        private readonly ILogger<ProduceBinary> logger;

        public ProduceBinary(IAgnosticProducer<byte[]> producer, IFileClient fileClient, IOptions<ApplicationSettings> settings, ILogger<ProduceBinary> logger)
        {
            this.producer = producer;
            this.fileClient = fileClient;
            this.settings = settings;
            this.logger = logger;
        }

        public async Task ExecuteAsync()
        {
            foreach (var filePath in fileClient.GetFiles(settings.Value.InputDirectory))
            {
                var message = new AgnosticMessage<byte[]>
                {
                    Key = filePath.Split('/').Last(),
                    Payload = fileClient.Read(filePath)
                };

                await producer.ProduceAsync("binary", message);

                logger.LogInformation($"Binary file read and sent: {message.Key}");
            }
        }
    }
}