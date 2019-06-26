using System;

namespace DotNetKafka.Application.Common
{
    public interface IAgnosticConsumer<TPayload> : IDisposable
    {
        void Consume(string topic, Action<AgnosticMessage<TPayload>> processingAction);
    }
}