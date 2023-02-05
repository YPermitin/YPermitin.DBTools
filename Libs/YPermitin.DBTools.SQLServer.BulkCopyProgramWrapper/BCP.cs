using System.Text;
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
        public ExecutionResult LastExecutionResult { get; private set; }

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
                ProcessHelper.RunCommand(_bcpUtilityPath, string.Empty, out _, out _, out _);

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
            ProcessHelper.RunCommand(_bcpUtilityPath, "-v",
                out string output, out _, out _, true);
            
            string version;
            var regexMatches = Regex.Matches(output, @"\d+\.\d+\.\d+\.\d+");
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

            ProcessHelper.RunCommand(_bcpUtilityPath, bcpArguments.ToString(),
                out string output,
                out string errorOutput,
                out int exitCode,
                false,
                Config.AdditionalSettings.OutputFile);

            LastExecutionResult = new ExecutionResult()
            {
                Message = output,
                ErrorMessage = errorOutput,
                ExitCode = exitCode
            };
        }

        /// <summary>
        /// Проверка возникновения ошибки
        /// </summary>
        /// <returns></returns>
        public bool ErrorOccurred()
        {
            return !string.IsNullOrEmpty(LastExecutionResult.ErrorMessage) || LastExecutionResult.ExitCode != 0;
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
                    LastExecutionResult.ExitCode,
                    LastExecutionResult.ErrorMessage,
                    LastExecutionResult.Message);
            }
        }
    }
}