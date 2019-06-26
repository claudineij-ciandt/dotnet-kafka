using System;
using System.Threading.Tasks;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetKafka.Application.UseCases.Text
{
    public class ProduceText : IProduceText
    {
        private readonly IAgnosticProducer<string> producer;
        private readonly IOptions<ApplicationSettings> settings;
        private readonly ILogger<ProduceText> logger;

        public ProduceText(IAgnosticProducer<string> producer, IOptions<ApplicationSettings> settings, ILogger<ProduceText> logger)
        {
            this.producer = producer;
            this.settings = settings;
            this.logger = logger;
        }

        public async Task ExecuteAsync()
        {
            for (int i=0; i<settings.Value.ElementsToProduce; i++)
            {
                var message = new AgnosticMessage<string>
                {
                    Key = $"msg-{i}",
                    Payload = $"msg-{i}-content"
                };

                await producer.ProduceAsync("text", message);

                logger.LogInformation($"String sent: {message.Payload}");
            }
        }
    }
}