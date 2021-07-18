using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XI.Portal.Bus.Client;
using XI.Portal.Bus.Extensions;

namespace XI.Portal.Bot.RaiseEvent
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            await serviceProvider.GetService<App>().Run(args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => { builder.AddConsole(); });

            Console.WriteLine($"Application Directory: {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables()
                .Build();

            services.Configure<PortalServiceBusOptions>(configuration.GetSection("ServiceBus"));
            services.AddServiceBus();

            services.AddTransient<App>();
        }
    }
}