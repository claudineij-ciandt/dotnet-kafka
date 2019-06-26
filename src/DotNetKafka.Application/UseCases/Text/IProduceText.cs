using System.Threading.Tasks;

namespace DotNetKafka.Application.UseCases.Text
{
    public interface IProduceText
    {
        Task ExecuteAsync();
    }
}