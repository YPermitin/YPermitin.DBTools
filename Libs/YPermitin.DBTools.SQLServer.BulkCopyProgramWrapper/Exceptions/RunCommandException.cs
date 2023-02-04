using System.Text;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Exceptions
{
    /// <summary>
    /// Ошибка запуска команды операционной системы
    /// </summary>
    public class RunCommandException : Exception
    {
        /// <summary>
        /// Код возврата приложения
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Консольный вывод приложения в части ошибок
        /// </summary>
        public string ErrorOutput { get; }

        /// <summary>
        /// Консольный вывод приложения
        /// </summary>
        public string FullOutput { get; }

        public RunCommandException(int exitCode, string errorOutput, string fullOutput)
        {
            ExitCode = exitCode;
            ErrorOutput = errorOutput;
            FullOutput = fullOutput;
        }

        public override string ToString()
        {
            StringBuilder fullErrorMessage = new StringBuilder();

            if (!string.IsNullOrEmpty(ErrorOutput))
            {

                fullErrorMessage.Append("Message: ");
                fullErrorMessage.Append(ErrorOutput);
                fullErrorMessage.AppendLine();
            }

            if (ExitCode != 0)
            {
                fullErrorMessage.Append("Exit code: ");
                fullErrorMessage.Append(ExitCode);
                fullErrorMessage.AppendLine();
            }

            return fullErrorMessage.ToString();
        }
    }
}
