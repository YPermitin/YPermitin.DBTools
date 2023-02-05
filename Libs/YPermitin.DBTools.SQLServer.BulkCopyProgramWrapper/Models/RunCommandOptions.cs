namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models
{
    /// <summary>
    /// Параметры запуска команды операционной системы
    /// </summary>
    public class RunCommandOptions
    {
        /// <summary>
        /// Путь к исполняемому файлу.
        ///
        /// Может быть указана команда без полного пути к исполняемому файлу, если она доступна в текущем контексте.
        /// </summary>
        public string? FileName { get; set; }
        
        /// <summary>
        /// Аргументы к исполняемому файлу
        /// </summary>
        public string? Arguments { get; set; }

        /// <summary>
        /// Путь к файлу лога, в который приложение записывает информацию о своей работе.
        ///
        /// Необязательный параметр. Используется для анализа активности приложения для защиты от зависаний.
        /// </summary>
        public string? ApplicationLogFile { get; set; }

        /// <summary>
        /// Вызывать исключение, если запуск команды прошел неудачно.
        /// </summary>
        public bool ThrowIfError { get; set; } = false;
    }
}
