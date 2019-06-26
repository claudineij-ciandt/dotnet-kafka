namespace DotNetKafka.Application.Common
{
    public class AgnosticMessage<TPayload>
    {
        public string Key { get; set; }

        public TPayload Payload { get; set; }
    }
}