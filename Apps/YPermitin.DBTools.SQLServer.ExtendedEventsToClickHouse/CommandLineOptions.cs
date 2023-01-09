using CommandLine;

namespace YPermitin.DBTools.SQLServer.ExtendedEventsToClickHouse
{
    public class CommandLineOptions
    {
        [Option('c', "config", Required = false, HelpText = "Config file's path to export event log.")]
        public string ConfigFile { get; set; }
        [Option('l', "logDirectoryPath", Required = false, HelpText = "Directory path to save application's logs.")]
        public string LogDirectoryPath { get; set; }
        [Option('i', "interactiveCommands", Required = false, HelpText = "Allow interactive console commands.", Default = true)]
        public bool AllowInteractiveCommands { get; set; }
    }
}
