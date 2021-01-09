using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YY.DBTools.SQLServer.XEvents.ToClickHouse;

namespace YY.DBTools.SQLServer.ExtendedEventsToClickHouse
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            string connectionString = Configuration.GetConnectionString("XEventsDatabase");

            IConfigurationSection XEEventSection = Configuration.GetSection("XEvents");
            int portion = XEEventSection.GetValue("Portion", 10000);
            string SourcePath = XEEventSection.GetValue("SourcePath", string.Empty);

            using (IXEventExportMaster export = new XEventExportMaster(BeforeExportData, AfterExportData, OnErrorExportData))
            {
                export.SetXEventsPath(SourcePath);
                IXEventsOnTarget target = new ExtendedEventsOnClickHouse(connectionString, portion);
                export.SetTarget(target);
                await export.StartSendEventsToStorage(CancellationToken.None);
            }

            Console.WriteLine("Press any key for exit...");
            Console.ReadKey();
        }

        private static void OnErrorExportData(OnErrorExportDataEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
        }

        private static void AfterExportData(AfterExportDataEventArgs e)
        {
            Console.WriteLine(" (+)");
        }

        private static void BeforeExportData(BeforeExportDataEventArgs e)
        {
            Console.Write($"Выгружено: {e.Rows.Count}");
        }
    }
}
