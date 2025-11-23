using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using ProcessExplorer.Models;

namespace ProcessExplorer.Core
{
    /// <summary>
    /// Classe responsável por monitorar processos do sistema.
    /// Demonstra conceitos de SO: gerenciamento de processos, threads, memória.
    /// </summary>
    public class ProcessMonitor
    {
        private readonly Dictionary<int, DateTime> _lastCpuTime = new();
        private readonly Dictionary<int, TimeSpan> _lastProcessorTime = new();
        private readonly Dictionary<int, (string UserName, DateTime CachedAt)> _userNameCache = new();
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _ramCounter;
        private const int CACHE_DURATION_SECONDS = 30;
        private long _totalPhysicalMemory = 0;

        public ProcessMonitor()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                // Obter memória física total (bytes) uma vez via WMI
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        _totalPhysicalMemory = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                        break;
                    }
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"Falha ao obter memória física total: {ex2.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao inicializar contadores de performance: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém a lista de todos os processos em execução.
        /// </summary>
        public List<ProcessInfo> GetProcesses()
        {
            var processList = new List<ProcessInfo>();
            var allProcesses = Process.GetProcesses();

            foreach (var process in allProcesses)
            {
                try
                {
                    // Verifica se o processo ainda existe antes de acessar
                    if (process.HasExited)
                        continue;

                    var processInfo = new ProcessInfo
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName
                    };

                    // Tenta obter informações que podem gerar exceção
                    try { processInfo.ThreadCount = process.Threads.Count; }
                    catch { processInfo.ThreadCount = 0; }

                    try { processInfo.HandleCount = process.HandleCount; }
                    catch { processInfo.HandleCount = 0; }

                    try { processInfo.WorkingSet = process.WorkingSet64; }
                    catch { processInfo.WorkingSet = 0; }

                    try { processInfo.PrivateBytes = process.PrivateMemorySize64; }
                    catch { processInfo.PrivateBytes = 0; }

                    try { processInfo.CpuUsage = CalculateCpuUsage(process); }
                    catch { processInfo.CpuUsage = 0; }

                    try
                    {
                        processInfo.StartTime = process.StartTime;
                    }
                    catch
                    {
                        processInfo.StartTime = DateTime.MinValue;
                    }

                    try
                    {
                        processInfo.FilePath = process.MainModule?.FileName ?? "N/A";
                    }
                    catch
                    {
                        processInfo.FilePath = "Acesso Negado";
                    }

                    // NÃO buscar usuário na listagem inicial - é muito lento!
                    // Será buscado apenas quando necessário (detalhes)
                    processInfo.UserName = "-";

                    processList.Add(processInfo);
                }
                catch (Exception ex)
                {
                    // Exceção esperada para processos protegidos - não é erro
                    Debug.WriteLine($"Processo inacessível: {ex.Message}");
                }
            }

            return processList.OrderByDescending(p => p.CpuUsage).ToList();
        }

        /// <summary>
        /// Calcula o uso de CPU de um processo específico.
        /// Demonstra: conceito de tempo de CPU e escalonamento de processos.
        /// </summary>
        private double CalculateCpuUsage(Process process)
        {
            try
            {
                var currentTime = DateTime.Now;
                var currentProcessorTime = process.TotalProcessorTime;

                if (_lastCpuTime.ContainsKey(process.Id) && _lastProcessorTime.ContainsKey(process.Id))
                {
                    var timeDiff = (currentTime - _lastCpuTime[process.Id]).TotalMilliseconds;
                    var processorTimeDiff = (currentProcessorTime - _lastProcessorTime[process.Id]).TotalMilliseconds;

                    if (timeDiff > 0)
                    {
                        var cpuUsage = (processorTimeDiff / timeDiff) * 100.0 / Environment.ProcessorCount;
                        _lastCpuTime[process.Id] = currentTime;
                        _lastProcessorTime[process.Id] = currentProcessorTime;
                        return Math.Min(cpuUsage, 100.0);
                    }
                }

                _lastCpuTime[process.Id] = currentTime;
                _lastProcessorTime[process.Id] = currentProcessorTime;
                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Obtém o proprietário (usuário) de um processo usando WMI com cache.
        /// Cache reduz chamadas WMI que são muito lentas.
        /// </summary>
        private string GetProcessOwner(int processId)
        {
            // Verificar cache primeiro
            if (_userNameCache.TryGetValue(processId, out var cached))
            {
                if ((DateTime.Now - cached.CachedAt).TotalSeconds < CACHE_DURATION_SECONDS)
                {
                    return cached.UserName;
                }
                else
                {
                    _userNameCache.Remove(processId);
                }
            }

            try
            {
                var query = $"SELECT * FROM Win32_Process WHERE ProcessId = {processId}";
                using var searcher = new ManagementObjectSearcher(query);
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var ownerInfo = new string[2];
                    obj.InvokeMethod("GetOwner", ownerInfo);
                    var userName = ownerInfo[0] ?? "SYSTEM";

                    // Adicionar ao cache
                    _userNameCache[processId] = (userName, DateTime.Now);
                    return userName;
                }
            }
            catch
            {
                var fallback = "N/A";
                _userNameCache[processId] = (fallback, DateTime.Now);
                return fallback;
            }

            return "N/A";
        }

        /// <summary>
        /// Obtém estatísticas gerais do sistema.
        /// </summary>
        public SystemStats GetSystemStats()
        {
            var stats = new SystemStats();

            try
            {
                var processes = Process.GetProcesses();
                stats.ProcessCount = processes.Length;
                stats.ThreadCount = processes.Sum(p =>
                {
                    try { return p.Threads.Count; }
                    catch { return 0; }
                });
                stats.HandleCount = processes.Sum(p =>
                {
                    try { return p.HandleCount; }
                    catch { return 0; }
                });
                stats.TotalMemoryUsed = processes.Sum(p =>
                {
                    try { return p.WorkingSet64; }
                    catch { return 0; }
                });

                if (_cpuCounter != null)
                {
                    stats.TotalCpuUsage = _cpuCounter.NextValue();
                }

                if (_ramCounter != null)
                {
                    var availableMB = _ramCounter.NextValue();
                    stats.TotalMemoryAvailable = (long)(availableMB * 1024 * 1024);
                }
                stats.TotalPhysicalMemory = _totalPhysicalMemory;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao obter estatísticas do sistema: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// Finaliza um processo pelo ID.
        /// Demonstra: gerenciamento de processos e sinais do SO.
        /// </summary>
        public bool KillProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao finalizar processo {processId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtém informações detalhadas de um processo específico.
        /// </summary>
        public ProcessInfo? GetProcessDetails(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                var info = new ProcessInfo
                {
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    WorkingSet = process.WorkingSet64,
                    PrivateBytes = process.PrivateMemorySize64,
                    CpuUsage = CalculateCpuUsage(process),
                    UserName = GetProcessOwner(process.Id)
                };

                try
                {
                    info.StartTime = process.StartTime;
                    info.FilePath = process.MainModule?.FileName ?? "N/A";
                }
                catch
                {
                    info.StartTime = DateTime.MinValue;
                    info.FilePath = "Acesso Negado";
                }

                return info;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
        }
    }
}
