using System.Diagnostics;
using System.Text;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Exceptions;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Helpers
{
    /// <summary>
    /// Служебный класс запуска внешний процессов
    /// </summary>
    internal static class ProcessHelper
    {
        /// <summary>
        /// Время ожидания завершения потока вывода на консоль или завершения работы процесса.
        /// По завершению ожидания проверяется завершился ли процесс и ожидание возобновляется.
        /// </summary>
        private static int WaitTimeoutMs = 30000;

        /// <summary>
        /// Запуск команды операционной системы
        /// </summary>
        /// <param name="fileName">Выполняемый файл для запуска</param>
        /// <param name="arguments">Аргументы командной строки</param>
        /// <param name="output">Вывод на консоль в результате выполнения</param>
        /// <param name="errorOutput">Вывод на консоль ошибок в результате выполнения</param>
        /// <param name="exitCode">Код завершения приложения</param>
        /// <param name="throwIfError">Вызывать исключение при возникновении ошибки.</param>
        /// <exception cref="RunCommandException">Общее исключение при запуске приложения</exception>
        public static void RunCommand(string fileName, string arguments,
            out string output, out string errorOutput, out int exitCode,
            bool throwIfError = false)
        {
            StringBuilder outputMessage = new StringBuilder();
            StringBuilder outputErrorMessage = new StringBuilder();

            using (Process process = new Process())
            {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                {
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                outputMessage.AppendLine(e.Data);
                            }
                        };
                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                outputErrorMessage.AppendLine(e.Data);
                            }
                        };

                        process.Start();

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        do
                        {
                            if (process.WaitForExit(WaitTimeoutMs) &&
                                outputWaitHandle.WaitOne(WaitTimeoutMs) &&
                                errorWaitHandle.WaitOne(WaitTimeoutMs))
                            {
                                exitCode = process.ExitCode;
                            }
                            else
                            {
                                // Timed out.
                            }
                        } while (!process.HasExited);
                    }
                }

                output = outputMessage.ToString();
                errorOutput = outputErrorMessage.ToString();
                exitCode = process.ExitCode;
                
                if (throwIfError)
                {
                    if (!string.IsNullOrEmpty(errorOutput) || exitCode != 0)
                    {
                        throw new RunCommandException(exitCode, errorOutput, output);
                    }
                }
            }
        }
    }
}
