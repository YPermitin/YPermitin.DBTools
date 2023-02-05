using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Exceptions;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Helpers;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models.Settings;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper
{
    /// <summary>
    /// Оболочка для работы с Bulk Copy Program (BCP)
    /// </summary>
    public sealed class BCP
    {
        /// <summary>
        /// Временный каталог библиотеки для хранения служебных данных
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly string BCPWrapperTempDirectory;

        /// <summary>
        /// Временный каталог для контроля запущенных процессов BCP
        /// </summary>
        private static readonly string BCPProcessControlDirectory;

        static BCP()
        {
            BCPWrapperTempDirectory = Path.Combine(Path.GetTempPath(), "BulkCopyProgramWrapper");
            Directory.CreateDirectory(BCPWrapperTempDirectory);
            BCPProcessControlDirectory = Path.Combine(BCPWrapperTempDirectory, "ProcessControl");
            Directory.CreateDirectory(BCPProcessControlDirectory);
        }

        /// <summary>
        /// Завершение всех неактивных (зависших) процессов утилиты BCP.exe, которые ранее были запущены через библиотеку.
        ///
        /// Список процессов контролируется через промежуточный временный каталог,
        /// поэтому те процессы BCP.exe, которые были запущены в других местах, не будут затронуты.
        /// </summary>
        public static void TerminateStuckBulkCopyProcesses()
        {
            var controlProcessInfoItems = Directory.GetFiles(BCPProcessControlDirectory, "*.json");
            foreach (var controlProcessInfoItem in controlProcessInfoItems)
            {
                try
                {
                    var controlInfoAsJson = File.ReadAllText(controlProcessInfoItem);
                    var controlProcessInfo = JsonSerializer.Deserialize<ProcessControlInfo>(controlInfoAsJson);
                    if (controlProcessInfo != null)
                    {
                        var processInfo = Process.GetProcessById(controlProcessInfo.ProcessId);

                        if (processInfo.HasExited)
                        {
                            File.Delete(controlProcessInfoItem);
                        }
                        else
                        {
                            if (processInfo.ProcessName == controlProcessInfo.ProcessName
                                && processInfo.StartTime == controlProcessInfo.ProcessStartTime)
                            {
                                var lastActivityTimeLeft = DateTime.UtcNow - controlProcessInfo.ProcessLastActivity;
                                if (lastActivityTimeLeft.TotalMilliseconds > ProcessHelper.ProcessActivityWaitMs)
                                {
                                    processInfo.Kill(true);
                                    File.Delete(controlProcessInfoItem);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // TODO Можно продумать реакцию на разные виды исключений.
                    // При возникновении ошибки удаляем файл с информацией о запущенном процессе.
                    File.Delete(controlProcessInfoItem);
                }
            }
        }

        /// <summary>
        /// Текущий путь к утилите BCP
        /// </summary>
        private string _bcpUtilityPath;

        /// <summary>
        /// Конфигурация BCP-операции
        /// </summary>
        public BCPConfig Config { get; private set; }
        
        /// <summary>
        /// Информация о результате выполнения последней операции
        /// </summary>
        public ExecutionResult? LastExecutionResult { get; private set; }

        public BCP()
        {
            _bcpUtilityPath = DefaultValues.UtilityPath;

            Config = new BCPConfig();
        }
        
        /// <summary>
        /// Проверка доступности утилиты BCP
        /// </summary>
        /// <returns>Истина - утилита доступна, Ложь - в остальных случаях</returns>
        public bool Available()
        {
            try
            {
                ProcessHelper.RunCommand(new RunCommandOptions()
                {
                    FileName = _bcpUtilityPath
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reports the bcp utility version number and copyright.
        /// </summary>
        public string Version()
        {
            ProcessHelper.RunCommand(new RunCommandOptions()
            {
                FileName = _bcpUtilityPath,
                Arguments = "-v"
            }, out RunCommandResult commandResult);

            string version;
            var regexMatches = Regex.Matches(
                commandResult.Output ?? string.Empty, 
                @"\d+\.\d+\.\d+\.\d+");

            if (regexMatches.Count == 1)
            {
                var regexMatchItem = regexMatches.First();
                version = regexMatchItem.Value;
            }
            else
            {
                throw new VersionNotRecognizedException();
            }

            return version;
        }

        /// <summary>
        /// Текущий путь к утилите BCP
        /// </summary>
        /// <returns>Путь к утилите BCP</returns>
        public string GetUtilityPath()
        {
            return _bcpUtilityPath;
        }

        /// <summary>
        /// Ручная установка пути к утилите BCP
        /// </summary>
        /// <param name="utilityPath">Путь к утилите BCP</param>
        public void SetUtilityPath(string utilityPath)
        {
            _bcpUtilityPath = utilityPath;
        }
        
        /// <summary>
        /// Выполнить операцию импорта / экспорта
        /// </summary>
        public void Execute()
        {
            StringBuilder bcpArguments = new StringBuilder();

            ((ICommonSettings)Config.ImportExportSettings).AddCommandLineParameters(bcpArguments);
            ((ICommonSettings)Config.ConnectionSettings).AddCommandLineParameters(bcpArguments);
            ((ICommonSettings)Config.BulkSettings).AddCommandLineParameters(bcpArguments);
            ((ICommonSettings)Config.AdditionalSettings).AddCommandLineParameters(bcpArguments);

            ProcessHelper.RunCommand(
                new RunCommandOptions()
                {
                    FileName = _bcpUtilityPath,
                    Arguments = bcpArguments.ToString(),
                    ApplicationLogFile = Config.AdditionalSettings.OutputFile
                }, 
                out RunCommandResult commandResult, SetProcessStart, SetProcessFinish, SetAcceptActivityProcess);

            LastExecutionResult = new ExecutionResult()
            {
                Message = commandResult.Output ?? string.Empty,
                ErrorMessage = commandResult.ErrorOutput ?? string.Empty,
                ExitCode = commandResult.ExitCode
            };
        }

        /// <summary>
        /// Проверка возникновения ошибки
        /// </summary>
        /// <returns></returns>
        public bool ErrorOccurred()
        {
            return !string.IsNullOrEmpty(LastExecutionResult?.ErrorMessage) || LastExecutionResult?.ExitCode != 0;
        }

        /// <summary>
        /// Вызвать исключение, если при последнем запуске операции была ошибка
        /// </summary>
        /// <exception cref="RunCommandException">Исключение с выполнением команды BCP</exception>
        public void ThrowExceptionIfError()
        {
            if (ErrorOccurred())
            {
                throw new RunCommandException(
                    LastExecutionResult?.ExitCode ?? -1,
                    LastExecutionResult?.ErrorMessage,
                    LastExecutionResult?.Message);
            }
        }

        /// <summary>
        /// Обработка события "При старте процесса"
        /// </summary>
        /// <param name="processControlInfo">Информация о процессе для контроля</param>
        private void SetProcessStart(ProcessControlInfo processControlInfo)
        {
            var processControlFileName = $"{processControlInfo}.json";
            string processControlFileFullPath = Path.Combine(BCPProcessControlDirectory, processControlFileName);
            string processControlInfoAsJson = JsonSerializer.Serialize(processControlInfo);
            File.WriteAllText(processControlFileFullPath, processControlInfoAsJson);
        }

        /// <summary>
        /// Обработка события "При подтверждении активности"
        /// </summary>
        /// <param name="processControlInfo">Информация о процессе для контроля</param>
        private void SetAcceptActivityProcess(ProcessControlInfo processControlInfo)
        {
            var processControlFileName = $"{processControlInfo}.json";
            string processControlFileFullPath = Path.Combine(BCPProcessControlDirectory, processControlFileName);
            string processControlInfoAsJson = JsonSerializer.Serialize(processControlInfo);
            File.WriteAllText(processControlFileFullPath, processControlInfoAsJson);
        }

        /// <summary>
        /// Обработка события "При завершении процесса"
        /// </summary>
        /// <param name="processControlInfo">Информация о процессе для контроля</param>
        private void SetProcessFinish(ProcessControlInfo processControlInfo)
        {
            var processControlFileName = $"{processControlInfo}.json";
            string processControlFileFullPath = Path.Combine(BCPProcessControlDirectory, processControlFileName);
            if (File.Exists(processControlFileFullPath))
            {
                File.Delete(processControlFileFullPath);
            }
        }
    }
}