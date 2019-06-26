using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetKafka.Application;
using DotNetKafka.Application.UseCases.Binary;
using DotNetKafka.Application.UseCases.Json;
using DotNetKafka.Application.UseCases.Text;
using DotNetKafka.Infrastructure.Configuration;
using DotNetKafka.Infrastructure.IoC;
using DotNetKafka.Infrastructure.Kafka;
using DotNetKafka.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetKafka.ConsoleApp
{
    public class Program
    {
        private static readonly string usage =
@"Usage:
    .. --use=<option> [--elem=<num>] [--group=<name>] [--in=<dir>] [--out=<dir>]

Options:
    --use       Options: produce-text, consume-text, produce-json, consume-json, produce-binary, consume-binary
    --elem      Number of elements (for text and json use cases) to generate and produce (default: 1000)
    --group     Consumer group name (default: default-group)
    --in        Input directory to read binary files from (default: <project_root>/.static/input)
    --out       Output directory to write binary files to (default: <project_root>/.static/output)";

        private static readonly Dictionary<string, string> commandLineMappings = new Dictionary<string, string>
        {
            { "--use",  "application:useCase" },
            { "--elem", "application:elementsToProduce" },
            { "--in",   "application:inputDirectory" },
            { "--out",  "application:outputDirectory" },
            { "--group","infrastructure:consumerGroup" },
        };

        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(usage);
                return;
            }

            using (var serviceProvider = ConfigureServices(args))
            {
                var settings = serviceProvider.GetService<IOptions<ApplicationSettings>>();
                await Execute(serviceProvider, settings.Value.UseCase);
            }
        }

        private static ServiceProvider ConfigureServices(string[] args)
        {
            return new ServiceCollection()
                .AddConfiguration(args, commandLineMappings)
                .AddLoggingProviders()
                .AddKafka()
                .AddServices()
                .BuildServiceProvider();
        }

        private static async Task Execute(ServiceProvider serviceProvider, string useCase)
        {
            var useCaseMappings = new Dictionary<string, Func<Task>>
            {
                { "produce-text",   () => serviceProvider.GetService<IProduceText>().ExecuteAsync() },
                { "consume-text",   () => Task.Run(() => serviceProvider.GetService<IConsumeText>().Execute()) },
                { "produce-json",   () => serviceProvider.GetService<IProduceJson>().ExecuteAsync() },
                { "consume-json",   () => Task.Run(() => serviceProvider.GetService<IConsumeJson>().Execute()) },
                { "produce-binary", () => serviceProvider.GetService<IProduceBinary>().ExecuteAsync() },
                { "consume-binary", () => Task.Run(() => serviceProvider.GetService<IConsumeBinary>().Execute()) },
            };

            try
            {
                var useCaseAction = useCaseMappings[useCase];
                await useCaseAction();
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine(usage);
            }
        }
    }
}
