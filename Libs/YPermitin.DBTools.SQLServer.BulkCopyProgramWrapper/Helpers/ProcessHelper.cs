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
        /// Время ожидания проявления активнсоти процессом, после которого процесс завершается.
        /// Активности определяется по выводу сообщений в консоль или файл лога (если указан).
        ///
        /// При указании значения 0 проверки активности не происходит.
        /// </summary>
        public static int ProcessActivityWaitMs = 180000;

        /// <summary>
        /// Время ожидания завершения потока вывода на консоль или завершения работы процесса.
        /// По завершению ожидания проверяется завершился ли процесс и ожидание возобновляется.
        ///
        /// При указании значения меньше 5 мс, оно будет проигнорировано и все равно будет установлено ожидание в 5 мс.
        /// </summary>
        public static int ConsoleOutputWaitTimeoutMs = 15000;

        /// <summary>
        /// Запуск команды операционной системы
        /// </summary>
        /// <param name="fileName">Выполняемый файл для запуска</param>
        /// <param name="arguments">Аргументы командной строки</param>
        /// <param name="output">Вывод на консоль в результате выполнения</param>
        /// <param name="errorOutput">Вывод на консоль ошибок в результате выполнения</param>
        /// <param name="exitCode">Код завершения приложения</param>
        /// <param name="throwIfError">Вызывать исключение при возникновении ошибки.</param>
        /// <param name="applicationLogFile">Путь к файлу лога, в котором содержится консольный вывод работы приложения.
        ///     Используется для анализа активности приложения и контроля зависания.
        /// </param>
        /// <exception cref="RunCommandException">Общее исключение при запуске приложения</exception>
        public static void RunCommand(string fileName, string arguments,
            out string output, out string errorOutput, out int exitCode,
            bool throwIfError = false, string? applicationLogFile = null)
        {
            StringBuilder outputMessage = new StringBuilder();
            StringBuilder outputErrorMessage = new StringBuilder();
            DateTime lastOutputDate = DateTime.MinValue;
            DateTime lastErrorOutputDate = DateTime.MinValue;
            DateTime lastLogFileOutputDate = DateTime.MinValue;

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
                                lastOutputDate = DateTime.UtcNow;
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
                                lastErrorOutputDate = DateTime.UtcNow;
                            }
                        };

                        int currentConsoleOutputWaitTimeoutMs = ConsoleOutputWaitTimeoutMs < 5 ? 5 : ConsoleOutputWaitTimeoutMs;
                        DateTime lastActivityTime = DateTime.UtcNow;

                        process.Start();

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        do
                        {
                            if (process.WaitForExit(currentConsoleOutputWaitTimeoutMs) &&
                                outputWaitHandle.WaitOne(currentConsoleOutputWaitTimeoutMs) &&
                                errorWaitHandle.WaitOne(currentConsoleOutputWaitTimeoutMs))
                            {
                                exitCode = process.ExitCode;
                            }
                            else
                            {
                                if (ProcessActivityWaitMs > 0)
                                {
                                    // Время с последней проверки активности приложения
                                    var lastActivityCheckTimeLeft = DateTime.UtcNow - lastActivityTime;
                                    if (lastActivityCheckTimeLeft.TotalMilliseconds > ProcessActivityWaitMs)
                                    {
                                        // Проверка новых данных, выведенных в лог или консоль
                                        if (!string.IsNullOrEmpty(applicationLogFile))
                                        {
                                            if (File.Exists(applicationLogFile))
                                            {
                                                lastLogFileOutputDate = (new FileInfo(applicationLogFile)).LastWriteTimeUtc;
                                            }
                                        }

                                        lastActivityTime = new[] { lastOutputDate, lastErrorOutputDate, lastLogFileOutputDate }
                                            .DefaultIfEmpty()
                                            .Max();

                                        var lastActivityTimeLeft = DateTime.UtcNow - lastActivityTime;
                                        if (lastActivityTimeLeft.TotalMilliseconds > ProcessActivityWaitMs)
                                        {
                                            if (!process.HasExited)
                                            {
                                                process.Kill();
                                                throw new RunCommandException(process.ExitCode, 
                                                    "Application hangs without any kind of activity. Process terminated.",
                                                    string.Empty);
                                            }
                                        }
                                    }
                                }

                                // Переходим к следующему шагу проверки.
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
