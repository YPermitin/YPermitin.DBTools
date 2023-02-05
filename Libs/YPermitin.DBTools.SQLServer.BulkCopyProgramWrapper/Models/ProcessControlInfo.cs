using System.Diagnostics;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models
{
    /// <summary>
    /// Информация о процессе для контроля выполнения
    /// </summary>
    public class ProcessControlInfo
    {
        /// <summary>
        /// Идентификатор процесса
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Наименование процесса
        /// </summary>
        public string? ProcessName { get; set; }
        
        /// <summary>
        /// Дата запуска процесса
        /// </summary>
        public DateTime ProcessStartTime { get; set; }

        public DateTime ProcessLastActivity { get; set; }

        public ProcessControlInfo()
        {
            
        }

        public ProcessControlInfo(Process process)
        {
            ProcessId = process.Id;
            ProcessName = process.ProcessName;
            ProcessStartTime = process.StartTime;
            ProcessLastActivity = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return string.Format("{0}_{1}_{2:yyyyMMddTHHmmss}",
                ProcessName, ProcessId, ProcessStartTime);
        }
    }
}
