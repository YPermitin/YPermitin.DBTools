using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace YPermitin.DBTools.SQLServer.ExtendedEventsToClickHouse
{
    class Program
    {
        public static IConfiguration Configuration;
        private static XEventsExportApplicationSettings _settings;
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
            if (_settings == null)
            {
                Console.WriteLine("Failed to initialize data export settings. Check the path to the config file.");
                return;
            }

            if (_settings.AllowInteractiveActions)
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLine("Stopping data export ......");
                    Console.WriteLine();

                    _cancellationTokenSource.Cancel();
                    e.Cancel = true;
                };
            }

            var services = new ServiceCollection();
            ConfigureServices(services);

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                XEventToClickHouse app = serviceProvider.GetService<XEventToClickHouse>();
                try
                {
                    await app.Run(_cancellationTokenSource.Token);
                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Operation canceled.");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("Operation canceled due to timeout.");
                        Console.WriteLine();
                    }
                }
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<XEventToClickHouse>()
                .AddSingleton(x => _settings);

            string logDirectoryPath = string.Format("{0}{1}{2}{3}{4:yyyyMMddHHmmss}-{5}{6}log.txt",
                _settings.LogDirectoryPath,
                Path.DirectorySeparatorChar,
                _settings.StorageType,
                Path.DirectorySeparatorChar, DateTime.UtcNow,
                Process.GetCurrentProcess().Id,
                Path.DirectorySeparatorChar);

            var serilogLogger = new LoggerConfiguration()
                .WriteTo.RollingFile(logDirectoryPath)
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(logger: serilogLogger, dispose: true);
            });
        }
        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = e.ExceptionObject.ToString();
            Console.WriteLine(errorMessage);
            Environment.Exit(1);
        }
        private static void RunOptions(CommandLineOptions options)
        {
            string logDirectoryPath = string.IsNullOrEmpty(options.LogDirectoryPath) ? "XEventsExportLogs" : options.LogDirectoryPath;
            string configFile = options.ConfigFile ?? "appsettings.json";

            Configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile, false, false)
                .Build();

            _settings = XEventsExportApplicationSettings.CreateSettings(
                Configuration,
                options.AllowInteractiveCommands,
                logDirectoryPath);
        }
        private static void HandleParseError(IEnumerable<Error> errors)
        {
            _settings = XEventsExportApplicationSettings.CreateSettings(null, false, "XEventsExportLogs");
        }
    }
}
