
namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models
{
    /// <summary>
    /// Результат запуска команды операционной системы
    /// </summary>
    public class RunCommandResult
    {
        /// <summary>
        /// Сообщение приложения при выполнении команды
        /// </summary>
        public string? Output { get; set; }

        /// <summary>
        /// Сообщение приложения об ошибках при выполнении команды
        /// </summary>
        public string? ErrorOutput { get; set; } 
        
        /// <summary>
        /// Код возврата приложения.
        ///
        /// Значение 0 обычно означает успешное выполнение команды. Остальные значения говорят об ошибках.
        /// </summary>
        public int ExitCode { get; set; }
    }
}
