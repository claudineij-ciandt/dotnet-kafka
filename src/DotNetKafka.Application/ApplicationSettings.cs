namespace DotNetKafka.Application
{
    public class ApplicationSettings
    {
        public string UseCase { get; set; }

        public int ElementsToProduce { get; set; }

        public string InputDirectory { get; set; }

        public string OutputDirectory { get; set; }
    }
}