using System;
using System.Threading.Tasks;

namespace DotNetKafka.Application.Common
{
    public interface IAgnosticProducer<TPayload> : IDisposable
    {
        Task ProduceAsync(string topic, AgnosticMessage<TPayload> message);
    }
}