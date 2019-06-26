using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetKafka.Application.Common;

namespace DotNetKafka.Infrastructure.File
{
    public class FileClient : IFileClient
    {
        public List<string> GetFiles(string directory)
        {
            return Directory.EnumerateFiles(directory).ToList();
        }

        public byte[] Read(string filePath)
        {
            return System.IO.File.ReadAllBytes(filePath);
        }

        public void Create(string directory, string fileName, byte[] content)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            System.IO.File.WriteAllBytes($"{directory}/{fileName}", content);
        }
    }
}