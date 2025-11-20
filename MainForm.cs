using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using ProcessExplorer.Core;
using ProcessExplorer.Models;
using ProcessExplorer.Forms;

namespace ProcessExplorer
{
    /// <summary>
    /// Formulário principal do Process Explorer.
    /// </summary>
    public partial class MainForm : Form
    {
        private ProcessMonitor _monitor;
        private Timer _refreshTimer;
        private ListView _processListView;
        private Panel _statusPanel;
        private Label _statusLabel;
        private Label _cpuLabel;
        private Label _memoryLabel;
        private MenuStrip _menuStrip;
        private ToolStrip _toolStrip;
        private Panel _detailsPanel;
        private PropertyGrid _propertyGrid;
        private int _selectedProcessId = -1;
        private bool _isLoading = false;

        public MainForm()
        {
            InitializeComponent();
            _monitor = new ProcessMonitor();
            SetupUI();
            SetupTimer();

            // Carregar processos de forma assíncrona após a janela ser exibida
            this.Load += async (s, e) => await LoadProcessesAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Simple Process Explorer - Trabalho de Sistemas Operacionais";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
        }

        private void SetupUI()
        {
            // Painel de status (adicionar primeiro para garantir que fique no bottom)
            _statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            _statusLabel = new Label
            {
                Text = "Processos: 0",
                AutoSize = true,
                Location = new Point(10, 7)
            };

            _cpuLabel = new Label
            {
                Text = "CPU: 0%",
                AutoSize = true,
                Location = new Point(150, 7)
            };

            _memoryLabel = new Label
            {
                Text = "Memória: 0 MB",
                AutoSize = true,
                Location = new Point(250, 7)
            };

            _statusPanel.Controls.AddRange(new Control[] { _statusLabel, _cpuLabel, _memoryLabel });
            
            // Menu superior (deve ter Dock.Top)
            _menuStrip = new MenuStrip
            {
                Dock = DockStyle.Top
            };

            var fileMenu = new ToolStripMenuItem("Arquivo");
            fileMenu.DropDownItems.Add("Atualizar", null, (s, e) => LoadProcesses());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Sair", null, (s, e) => Application.Exit());

            var viewMenu = new ToolStripMenuItem("Visualizar");
            viewMenu.DropDownItems.Add("Gráficos de Performance", null, (s, e) => ShowPerformanceGraphs());
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add("Atualizar a cada 1 segundo", null, (s, e) => SetRefreshRate(1000));
            viewMenu.DropDownItems.Add("Atualizar a cada 2 segundos", null, (s, e) => SetRefreshRate(2000));
            viewMenu.DropDownItems.Add("Atualizar a cada 5 segundos", null, (s, e) => SetRefreshRate(5000));

            var helpMenu = new ToolStripMenuItem("Ajuda");
            helpMenu.DropDownItems.Add("Sobre", null, (s, e) => ShowAbout());

            _menuStrip.Items.AddRange(new[] { fileMenu, viewMenu, helpMenu });
            
            // Toolbar (Dock.Top abaixo do menu)
            _toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top
            };
            _toolStrip.Items.Add("Atualizar", null, (s, e) => LoadProcesses());
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add("Finalizar Processo", null, (s, e) => KillSelectedProcess());
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add("Detalhes", null, (s, e) => ToggleDetailsPanel());
            

            // Container principal com splitter
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400,
                Panel2Collapsed = true
            };

            // ListView de processos
            _processListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                Font = new Font("Consolas", 9F),
                HeaderStyle = ColumnHeaderStyle.Clickable
            };

            _processListView.Columns.Add("PID", 70, HorizontalAlignment.Left);
            _processListView.Columns.Add("Nome do Processo", 200, HorizontalAlignment.Left);
            _processListView.Columns.Add("CPU %", 80, HorizontalAlignment.Right);
            _processListView.Columns.Add("Memória", 120, HorizontalAlignment.Right);
            _processListView.Columns.Add("Threads", 80, HorizontalAlignment.Center);
            _processListView.Columns.Add("Handles", 80, HorizontalAlignment.Center);
            _processListView.Columns.Add("Usuário", 120, HorizontalAlignment.Left);
            _processListView.Columns.Add("Tempo de Execução", 130, HorizontalAlignment.Left);

            _processListView.SelectedIndexChanged += ProcessListView_SelectedIndexChanged;
            _processListView.DoubleClick += ProcessListView_DoubleClick;

            // Context menu para o ListView
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Finalizar Processo", null, (s, e) => KillSelectedProcess());
            contextMenu.Items.Add("Ver Detalhes", null, (s, e) => ShowProcessDetails());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Atualizar", null, (s, e) => LoadProcesses());
            _processListView.ContextMenuStrip = contextMenu;

            splitContainer.Panel1.Controls.Add(_processListView);

            // Painel de detalhes (inicialmente escondido)
            _detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(5)
            };

            var detailsLabel = new Label
            {
                Text = "Detalhes do Processo",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                HelpVisible = false,
                ToolbarVisible = false
            };

            _detailsPanel.Controls.Add(_propertyGrid);
            _detailsPanel.Controls.Add(detailsLabel);
            splitContainer.Panel2.Controls.Add(_detailsPanel);

            // Adicionar controles na ordem correta para respeitar o docking
            this.Controls.Add(splitContainer);      // ocupa o espaço principal
            this.Controls.Add(_toolStrip);          // barra de ferramentas logo abaixo do menu
            this.Controls.Add(_menuStrip);          // menu no topo
            this.Controls.Add(_statusPanel);        // status bar no rodapé
            this.MainMenuStrip = _menuStrip;
        }

        private void SetupTimer()
        {
            _refreshTimer = new Timer
            {
                Interval = 2000 // 2 segundos
            };
            _refreshTimer.Tick += async (s, e) => await LoadProcessesAsync();
            _refreshTimer.Start();
        }

        private async Task LoadProcessesAsync()
        {
            // Evitar múltiplas carregamentos simultâneos
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                // Mostrar indicador de carregamento
                _statusLabel.Text = "Carregando processos...";
                this.Cursor = Cursors.WaitCursor;

                // Executar operações pesadas em thread separada
                var (processes, stats) = await Task.Run(() =>
                {
                    var procs = _monitor.GetProcesses();
                    var st = _monitor.GetSystemStats();
                    return (procs, st);
                });

                // Atualizar UI na thread principal
                _processListView.BeginUpdate();
                _processListView.Items.Clear();

                foreach (var proc in processes)
                {
                    var item = new ListViewItem(proc.ProcessId.ToString());
                    item.SubItems.Add(proc.ProcessName);
                    item.SubItems.Add($"{proc.CpuUsage:F2}");
                    item.SubItems.Add(proc.WorkingSetMB);
                    item.SubItems.Add(proc.ThreadCount.ToString());
                    item.SubItems.Add(proc.HandleCount.ToString());
                    item.SubItems.Add(proc.UserName);
                    item.SubItems.Add(proc.RunningTime);
                    item.Tag = proc;

                    // Colorir processos com alto uso de CPU
                    if (proc.CpuUsage > 50)
                        item.BackColor = Color.LightCoral;
                    else if (proc.CpuUsage > 25)
                        item.BackColor = Color.LightYellow;

                    _processListView.Items.Add(item);
                }

                _processListView.EndUpdate();

                // Atualizar barra de status
                _statusLabel.Text = $"Processos: {stats.ProcessCount} | Threads: {stats.ThreadCount} | Handles: {stats.HandleCount}";
                _cpuLabel.Text = $"CPU: {stats.TotalCpuUsage:F1}%";
                _memoryLabel.Text = $"Memória: {stats.TotalMemoryUsedMB}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar processos: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "Erro ao carregar processos";
            }
            finally
            {
                this.Cursor = Cursors.Default;
                _isLoading = false;
            }
        }

        private void LoadProcesses()
        {
            // Método síncrono mantido para compatibilidade
            _ = LoadProcessesAsync();
        }

        private void ProcessListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_processListView.SelectedItems.Count > 0)
            {
                var item = _processListView.SelectedItems[0];
                var processInfo = item.Tag as ProcessInfo;
                if (processInfo != null)
                {
                    _selectedProcessId = processInfo.ProcessId;
                    if (_propertyGrid.Visible)
                    {
                        _propertyGrid.SelectedObject = processInfo;
                    }
                }
            }
        }

        private void ProcessListView_DoubleClick(object? sender, EventArgs e)
        {
            ShowProcessDetails();
        }

        private void KillSelectedProcess()
        {
            if (_selectedProcessId == -1)
            {
                MessageBox.Show("Selecione um processo primeiro.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Deseja realmente finalizar o processo (PID: {_selectedProcessId})?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (_monitor.KillProcess(_selectedProcessId))
                {
                    MessageBox.Show("Processo finalizado com sucesso.", "Sucesso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProcesses();
                }
                else
                {
                    MessageBox.Show("Falha ao finalizar o processo. Verifique as permissões.", "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowProcessDetails()
        {
            if (_selectedProcessId == -1) return;

            var details = _monitor.GetProcessDetails(_selectedProcessId);
            if (details != null)
            {
                _propertyGrid.SelectedObject = details;
                var parent = _processListView.Parent as SplitContainer;
                if (parent != null)
                {
                    parent.Panel2Collapsed = false;
                }
            }
        }

        private void ToggleDetailsPanel()
        {
            var parent = _processListView.Parent as SplitContainer;
            if (parent != null)
            {
                parent.Panel2Collapsed = !parent.Panel2Collapsed;
            }
        }

        private void SetRefreshRate(int milliseconds)
        {
            _refreshTimer.Interval = milliseconds;
        }

        private void ShowPerformanceGraphs()
        {
            var performanceForm = new PerformanceMonitorForm(_monitor);
            performanceForm.Show();
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Simple Process Explorer\n\n" +
                "Trabalho Final de Sistemas Operacionais\n" +
                "Réplica simplificada do Process Explorer (Sysinternals)\n\n" +
                "Desenvolvido em C# com Windows Forms\n" +
                "Versão 1.0.0\n\n" +
                "Demonstra conceitos de:\n" +
                "- Gerenciamento de Processos\n" +
                "- Uso de CPU e Memória\n" +
                "- Threads e Handles\n" +
                "- Monitoramento em Tempo Real",
                "Sobre",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _monitor?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
