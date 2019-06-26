using System.Threading.Tasks;

namespace DotNetKafka.Application.UseCases.Json
{
    public interface IProduceJson
    {
        Task ExecuteAsync();
    }
}