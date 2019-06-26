using System.Threading.Tasks;

namespace DotNetKafka.Application.UseCases.Binary
{
    public interface IProduceBinary
    {
        Task ExecuteAsync();
    }
}