using System;

namespace ProcessExplorer.Models
{
    /// <summary>
    /// Classe que representa estatísticas gerais do sistema.
    /// </summary>
    public class SystemStats
    {
        public double TotalCpuUsage { get; set; }
        public long TotalMemoryUsed { get; set; }
        public long TotalMemoryAvailable { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        public string TotalMemoryUsedMB => $"{TotalMemoryUsed / 1024.0 / 1024.0:F2} MB";
        public string TotalMemoryAvailableMB => $"{TotalMemoryAvailable / 1024.0 / 1024.0:F2} MB";
        public double MemoryUsagePercent
        {
            get
            {
                if (TotalMemoryAvailable == 0) return 0;
                return (TotalMemoryUsed * 10.0) / TotalMemoryAvailable;
            }
        }
    }
}
