using System.Diagnostics;
using System.Text;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Exceptions;
using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Helpers
{
    /// <summary>
    /// Служебный класс запуска внешний процессов
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Время ожидания проявления активнсоти процессом, после которого процесс завершается.
        /// Активность определяется по выводу сообщений в консоль или файл лога (если указан).
        ///
        /// При указании значения 0 проверки активности не происходит.
        /// </summary>
        public static int ProcessActivityWaitMs = 180000;

        /// <summary>
        /// Время ожидания завершения потока вывода на консоль или завершения работы процесса.
        /// По завершению ожидания проверяется завершился ли процесс и ожидание возобновляется.
        ///
        /// При указании значения меньше 5000 мс, оно будет проигнорировано и все равно будет установлено ожидание в 5000 мс.
        /// </summary>
        public static int ConsoleOutputWaitTimeoutMs = 15000;

        private static readonly object LockLastOutputDateUpdate = new();
        private static readonly object LockLastErrorOutputDateUpdate = new();

        /// <summary>
        /// Описание метода для обработки события "При запуске процесса"
        /// </summary>
        /// <param name="process">Информация о процессе</param>
        public delegate void OnStartProcessEventHandler(ProcessControlInfo process);

        /// <summary>
        /// Описание метода для обработки события "При подтверждении активности"
        /// </summary>
        /// <param name="process">Информация о процессе</param>
        public delegate void OnAcceptActivityEventHandler(ProcessControlInfo process);

        /// <summary>
        /// Описание метода для обработки события "При завершении процесса"
        /// </summary>
        /// <param name="process">Информация о процессе</param>
        public delegate void OnFinishProcessEventHandler(ProcessControlInfo process);

        /// <summary>
        /// Запуск команды операционной системы
        /// </summary>
        /// <param name="options">Параметры запуска команды</param>
        /// <param name="outputResult">Результат запуска команды (выходной параметр)</param>
        /// <param name="onStartProcessEventHandler">Обработчик события "При запуске процесса".
        ///     Срабатывает сразу после запуска команды и содержит информацию о запущенном процессе.
        /// </param>
        /// <param name="onFinishProcessEventHandler">Обработчик события "При завершении процесса"
        ///     Срабатывает по завершении работы команды и содержит информацию о запущенном процессе.</param>
        /// <param name="onAcceptActivityEventHandler">Обработчик события "При подтверждении активности процесса"
        ///     Срабатывает при изменении даты последней активности процесса, запущенный командой.</param>
        /// <exception cref="RunCommandException">Общее исключение при запуске приложения</exception>
        public static void RunCommand(RunCommandOptions options, out RunCommandResult outputResult, 
            OnStartProcessEventHandler? onStartProcessEventHandler = null,
            OnFinishProcessEventHandler? onFinishProcessEventHandler = null,
            OnAcceptActivityEventHandler? onAcceptActivityEventHandler = null)
        {
            outputResult = new RunCommandResult();

            StringBuilder outputMessage = new StringBuilder();
            StringBuilder outputErrorMessage = new StringBuilder();
            DateTime lastOutputDate = DateTime.MinValue;
            DateTime lastErrorOutputDate = DateTime.MinValue;
            DateTime lastLogFileOutputDate = DateTime.MinValue;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = options.FileName;
                process.StartInfo.Arguments = options.Arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                {
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        process.OutputDataReceived += (_, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                outputMessage.AppendLine(e.Data);
                                lock (LockLastOutputDateUpdate)
                                {
                                    lastOutputDate = DateTime.UtcNow;
                                }
                            }
                        };
                        process.ErrorDataReceived += (_, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                outputErrorMessage.AppendLine(e.Data);
                                lock (LockLastErrorOutputDateUpdate)
                                {
                                    lastErrorOutputDate = DateTime.UtcNow;
                                }
                            }
                        };

                        int currentConsoleOutputWaitTimeoutMs = ConsoleOutputWaitTimeoutMs < 5000 ? 5000 : ConsoleOutputWaitTimeoutMs;
                        DateTime lastActivityTime = DateTime.UtcNow;
                        
                        process.Start();
                        ProcessControlInfo? processControlInfo = null;
                        try
                        {
                            processControlInfo = new ProcessControlInfo(process);
                        }
                        catch
                        {
                            // Процесс уже был завершен и информация по нему недоступна.
                        }

                        if (onStartProcessEventHandler != null && processControlInfo != null)
                        {
                            onStartProcessEventHandler.Invoke(processControlInfo);
                        }

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        do
                        {
                            if (process.WaitForExit(currentConsoleOutputWaitTimeoutMs) &&
                                outputWaitHandle.WaitOne(currentConsoleOutputWaitTimeoutMs) &&
                                errorWaitHandle.WaitOne(currentConsoleOutputWaitTimeoutMs))
                            {
                                outputResult.ExitCode = process.ExitCode;
                            }
                            else
                            {
                                if (ProcessActivityWaitMs > 0)
                                {
                                    // Время с последней проверки активности приложения больше времени ождидания вывода на консоль.
                                    // Защита, чтобы не проверять слишком часто.
                                    var lastActivityCheckTimeLeft = DateTime.UtcNow - lastActivityTime;
                                    if (lastActivityCheckTimeLeft.TotalMilliseconds > ConsoleOutputWaitTimeoutMs)
                                    {
                                        DateTime lastActivityOutputDate;
                                        lock (LockLastOutputDateUpdate)
                                        {
                                            lastActivityOutputDate = lastOutputDate;
                                        }
                                        DateTime lastActivityOutputErrorDate;
                                        lock (LockLastErrorOutputDateUpdate)
                                        {
                                            lastActivityOutputErrorDate = lastErrorOutputDate;
                                        }

                                        // Проверка новых данных, выведенных в лог или консоль
                                        if (!string.IsNullOrEmpty(options.ApplicationLogFile))
                                        {
                                            if (File.Exists(options.ApplicationLogFile))
                                            {

                                                lastLogFileOutputDate = (new FileInfo(options.ApplicationLogFile)).LastWriteTimeUtc;
                                            }
                                        }

                                        lastActivityTime = new[] { lastActivityOutputDate, lastActivityOutputErrorDate, lastLogFileOutputDate }
                                            .DefaultIfEmpty()
                                            .Max();

                                        if (onAcceptActivityEventHandler != null && processControlInfo != null)
                                        {
                                            if (processControlInfo.ProcessLastActivity != lastActivityTime)
                                            {
                                                processControlInfo.ProcessLastActivity = lastActivityTime;
                                                onAcceptActivityEventHandler.Invoke(processControlInfo);
                                            }
                                        }

                                        var lastActivityTimeLeft = DateTime.UtcNow - lastActivityTime;
                                        if (lastActivityTimeLeft.TotalMilliseconds > ProcessActivityWaitMs)
                                        {
                                            if (!process.HasExited)
                                            {
                                                process.Kill();

                                                if (onFinishProcessEventHandler != null && processControlInfo != null)
                                                {
                                                    onFinishProcessEventHandler.Invoke(processControlInfo);
                                                }

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

                        if (onFinishProcessEventHandler != null && processControlInfo != null)
                        {
                            onFinishProcessEventHandler.Invoke(processControlInfo);
                        }
                    }
                }

                outputResult.Output = outputMessage.ToString();
                outputResult.ErrorOutput = outputErrorMessage.ToString();
                outputResult.ExitCode = process.ExitCode;
                
                if (options.ThrowIfError)
                {
                    if (!string.IsNullOrEmpty(outputResult.ErrorOutput) || outputResult.ExitCode != 0)
                    {
                        throw new RunCommandException(outputResult.ExitCode, outputResult.ErrorOutput, outputResult.Output);
                    }
                }
            }
        }
    }
}
