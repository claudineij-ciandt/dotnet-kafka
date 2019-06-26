using System;
using System.Threading.Tasks;
using DotNetKafka.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DotNetKafka.Application.UseCases.Json
{
    public class ProduceJson : IProduceJson
    {
        private readonly IAgnosticProducer<string> producer;
        private readonly IOptions<ApplicationSettings> settings;
        private readonly ILogger<ProduceJson> logger;

        public ProduceJson(IAgnosticProducer<string> producer, IOptions<ApplicationSettings> settings, ILogger<ProduceJson> logger)
        {
            this.producer = producer;
            this.settings = settings;
            this.logger = logger;
        }

        public async Task ExecuteAsync()
        {
            for (int i=0; i<settings.Value.ElementsToProduce; i++)
            {
                var domainObject = new SampleComplexObject(i);

                var message = new AgnosticMessage<string>
                {
                    Key = domainObject.Id.ToString(),
                    Payload = JsonConvert.SerializeObject(domainObject) //TODO: change serialization method
                };

                await producer.ProduceAsync("json", message);

                logger.LogInformation($"JSON object sent: {domainObject.GetFullDescription()}");
            }
        }
    }
}