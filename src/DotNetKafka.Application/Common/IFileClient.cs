using System.Collections.Generic;

namespace DotNetKafka.Application.Common
{
    public interface IFileClient
    {
        List<string> GetFiles(string directory);

        byte[] Read(string filePath);

        void Create(string directory, string fileName, byte[] content);
    }
}