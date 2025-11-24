using System;
using System.Drawing;
using System.Windows.Forms;
using ProcessExplorer.Controls;
using ProcessExplorer.Core;

namespace ProcessExplorer.Forms
{
    /// Formulário para monitoramento gráfico de performance do sistema.
    public class PerformanceMonitorForm : Form
    {
        private ProcessMonitor _monitor;
        private Timer _updateTimer;
        private PerformanceGraph _cpuGraph;
        private PerformanceGraph _memoryGraph;
        private Label _cpuInfoLabel;
        private Label _memoryInfoLabel;
        private Panel _cpuPanel;
        private Panel _memoryPanel;

        public PerformanceMonitorForm(ProcessMonitor monitor)
        {
            _monitor = monitor;
            InitializeComponent();
            SetupGraphs();
            SetupTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "Monitor de Performance do Sistema";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(600, 400);
        }

        private void SetupGraphs()
        {
            // Painel principal
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Painel CPU
            _cpuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(10),
                Margin = new Padding(5)
            };

            _cpuInfoLabel = new Label
            {
                Dock = DockStyle.Top,
                Text = "Uso de CPU",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 30
            };

            _cpuGraph = new PerformanceGraph
            {
                Dock = DockStyle.Fill,
                Title = "CPU",
                LineColor = Color.FromArgb(0, 200, 255),
                MaxValue = 100
            };

            _cpuPanel.Controls.Add(_cpuGraph);
            _cpuPanel.Controls.Add(_cpuInfoLabel);

            // Painel Memória
            _memoryPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(10),
                Margin = new Padding(5)
            };

            _memoryInfoLabel = new Label
            {
                Dock = DockStyle.Top,
                Text = "Uso de Memória",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 30
            };

            _memoryGraph = new PerformanceGraph
            {
                Dock = DockStyle.Fill,
                Title = "Memória",
                LineColor = Color.FromArgb(255, 128, 0),
                MaxValue = 100
            };

            _memoryPanel.Controls.Add(_memoryGraph);
            _memoryPanel.Controls.Add(_memoryInfoLabel);

            // Adicionar painéis ao layout
            mainPanel.Controls.Add(_cpuPanel, 0, 0);
            mainPanel.Controls.Add(_memoryPanel, 0, 1);

            this.Controls.Add(mainPanel);
        }

        private void SetupTimer()
        {
            _updateTimer = new Timer
            {
                Interval = 1000 // Atualizar a cada 1 segundo
            };
            _updateTimer.Tick += UpdateGraphs;
            _updateTimer.Start();
        }

        private void UpdateGraphs(object? sender, EventArgs e)
        {
            try
            {
                var stats = _monitor.GetSystemStats();

                // Atualizar gráfico de CPU
                _cpuGraph.AddValue(stats.TotalCpuUsage);
                _cpuInfoLabel.Text = $"Uso de CPU: {stats.TotalCpuUsage:F1}% | Processos: {stats.ProcessCount} | Threads: {stats.ThreadCount}";

                // Atualizar gráfico de Memória
                _memoryGraph.AddValue(stats.MemoryUsagePercent);
                _memoryInfoLabel.Text = $"Uso de Memória: {stats.MemoryUsagePercent:F1}% | " +
                                       $"Usado: {stats.TotalMemoryUsedMB} | " +
                                       $"Disponível: {stats.TotalMemoryAvailableMB}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar gráficos: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
