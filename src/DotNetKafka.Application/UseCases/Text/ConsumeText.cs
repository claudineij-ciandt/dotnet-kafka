using System;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;

namespace DotNetKafka.Application.UseCases.Text
{
    public class ConsumeText : IConsumeText
    {
        private readonly IAgnosticConsumer<string> consumer;
        private readonly ILogger<ConsumeText> logger;

        public ConsumeText(IAgnosticConsumer<string> consumer, ILogger<ConsumeText> logger)
        {
            this.consumer = consumer;
            this.logger = logger;
        }

        public void Execute()
        {
            consumer.Consume("text", message =>
            {
                logger.LogInformation($"String received: {message.Payload}");
            });
        }
    }
}