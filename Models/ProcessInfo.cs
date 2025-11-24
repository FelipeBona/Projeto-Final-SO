using System;

namespace ProcessExplorer.Models
{
    /// Classe que representa informações de um processo do sistema.
    public class ProcessInfo
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public double CpuUsage { get; set; }
        public long WorkingSet { get; set; }  // Memória física (bytes)
        public long PrivateBytes { get; set; } // Memória privada (bytes)
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public DateTime StartTime { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Status { get; set; } = "Running";

        /// Retorna o uso de memória formatado em MB.
        public string WorkingSetMB => $"{WorkingSet / 1024.0 / 1024.0:F2} MB";

        /// Retorna o tempo de execução do processo.
  
        public string RunningTime
        {
            get
            {
                try
                {
                    var elapsed = DateTime.Now - StartTime;
                    if (elapsed.TotalDays >= 1)
                        return $"{(int)elapsed.TotalDays}d {elapsed.Hours}h";
                    if (elapsed.TotalHours >= 1)
                        return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
                    return $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";
                }
                catch
                {
                    return "N/A";
                }
            }
        }
    }
}
