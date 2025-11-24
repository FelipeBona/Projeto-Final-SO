using System;

namespace ProcessExplorer.Models
{
    /// Classe que representa estatísticas gerais do sistema.
    public class SystemStats
    {
        public double TotalCpuUsage { get; set; }
        public long TotalMemoryUsed { get; set; }
        public long TotalMemoryAvailable { get; set; }
        public long TotalPhysicalMemory { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        public string TotalMemoryUsedMB => $"{TotalMemoryUsed / 1024.0 / 1024.0:F2} MB";
        public string TotalMemoryAvailableMB => $"{TotalMemoryAvailable / 1024.0 / 1024.0:F2} MB";
        public double MemoryUsagePercent
        {
            get
            {
                if (TotalPhysicalMemory > 0 && TotalMemoryAvailable > 0)
                {
                    var usedPhysical = TotalPhysicalMemory - TotalMemoryAvailable;
                    if (usedPhysical < 0) usedPhysical = 0;
                    return (usedPhysical * 100.0) / TotalPhysicalMemory;
                }
                // Fallback: proporção entre soma de working sets e available (não ideal)
                if (TotalMemoryAvailable == 0) return 0;
                var approxTotal = TotalMemoryUsed + TotalMemoryAvailable;
                return (TotalMemoryUsed * 100.0) / approxTotal;
            }
        }
    }
}
