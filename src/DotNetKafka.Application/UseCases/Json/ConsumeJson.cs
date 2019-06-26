using System;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetKafka.Application.UseCases.Json
{
    public class ConsumeJson : IConsumeJson
    {
        private readonly IAgnosticConsumer<string> consumer;
        private readonly ILogger<ConsumeJson> logger;

        public ConsumeJson(IAgnosticConsumer<string> consumer, ILogger<ConsumeJson> logger)
        {
            this.consumer = consumer;
            this.logger = logger;
        }

        public void Execute()
        {
            consumer.Consume("json", message =>
            {
                var domainObject = JsonConvert.DeserializeObject<SampleComplexObject>(message.Payload); //TODO: change serialization method
                logger.LogInformation($"JSON object received: {domainObject.GetFullDescription()}");
            });
        }
    }
}