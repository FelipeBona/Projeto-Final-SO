using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
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
        private List<ProcessInfo> _currentProcesses = new List<ProcessInfo>();
        private ToolStripTextBox _searchBox;
        private ToolStripLabel _searchLabel;
        private ToolStripStatusLabel _statusProcessesLabel;
        private ToolStripStatusLabel _statusCpuLabel;
        private ToolStripStatusLabel _statusMemLabel;
        private StatusStrip _statusStrip;
        private Panel _headerPanel;
        private Label _titleLabel;
        private RichTextBox _summaryBox;
        private int _sortColumn = -1;
        private bool _sortAscending = true;
        private ToolStripComboBox _filterModeCombo;
        private bool _isPaused = false;
        private ToolStripButton _pauseButton;
        private ToolStripMenuItem _themeLightItem;
        private ToolStripMenuItem _themeDarkItem;
        private ToolStripStatusLabel _statusIntervalLabel;
        private ToolStripStatusLabel _statusLastUpdateLabel;
        private string _settingsFilePath;
        private ToolStripProgressBar _cpuProgressBar;
        private ToolStripProgressBar _memProgressBar;
        private Label _headerUsageLabel;

        public MainForm()
        {
            InitializeComponent();
            _monitor = new ProcessMonitor();
            SetupUI();
            SetupTimer();
            this.KeyPreview = true;
            this.Icon = SystemIcons.Application;
            this.KeyDown += MainForm_KeyDown;
            this.Shown += (s, e) => AdjustHeaderHeight();
            this.Resize += (s, e) => AdjustHeaderHeight();
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProcessExplorerSettings.txt");
            LoadSettings();

            // Carregar processos de forma assíncrona após a janela ser exibida
            this.Load += async (s, e) => await LoadProcessesAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Process Explorer - Monitoramento de Sistema";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 250, 252);
        }

        private void SetupUI()
        {
            // StatusStrip profissional
            _statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                SizingGrip = false
            };
            _statusProcessesLabel = new ToolStripStatusLabel("Processos: 0") { Spring = false };
            _statusCpuLabel = new ToolStripStatusLabel("CPU: 0%") { Spring = false };
            _statusMemLabel = new ToolStripStatusLabel("Memória: 0 MB") { Spring = false };
            _statusIntervalLabel = new ToolStripStatusLabel("Intervalo: 2000ms");
            _statusLastUpdateLabel = new ToolStripStatusLabel("Última atualização: -");
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusProcessesLabel, new ToolStripSeparator(), _statusCpuLabel, new ToolStripSeparator(), _statusMemLabel, new ToolStripSeparator(), _statusIntervalLabel, new ToolStripSeparator(), _statusLastUpdateLabel });
            _cpuProgressBar = new ToolStripProgressBar { Width = 90, Maximum = 100 };            
            _memProgressBar = new ToolStripProgressBar { Width = 90, Maximum = 100 };
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusProcessesLabel, new ToolStripSeparator(), _statusCpuLabel, _cpuProgressBar, new ToolStripSeparator(), _statusMemLabel, _memProgressBar, new ToolStripSeparator(), _statusIntervalLabel, new ToolStripSeparator(), _statusLastUpdateLabel });
            
            // Menu superior (deve ter Dock.Top)
            _menuStrip = new MenuStrip
            {
                Dock = DockStyle.Top
            };

            var fileMenu = new ToolStripMenuItem("Arquivo");
            fileMenu.DropDownItems.Add("Atualizar", null, (s, e) => LoadProcesses());
            fileMenu.DropDownItems.Add("Exportar Lista...", null, (s, e) => ExportProcesses());
            fileMenu.DropDownItems.Add("Exportar JSON...", null, (s, e) => ExportProcessesJson());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Sair", null, (s, e) => Application.Exit());

            var viewMenu = new ToolStripMenuItem("Visualizar");
            viewMenu.DropDownItems.Add("Gráficos de Performance", null, (s, e) => ShowPerformanceGraphs());
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add("Atualizar a cada 1 segundo", null, (s, e) => SetRefreshRate(1000));
            viewMenu.DropDownItems.Add("Atualizar a cada 2 segundos", null, (s, e) => SetRefreshRate(2000));
            viewMenu.DropDownItems.Add("Atualizar a cada 5 segundos", null, (s, e) => SetRefreshRate(5000));
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add("Filtrar por Nome...", null, (s, e) => _searchBox.Focus());
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            var temaMenu = new ToolStripMenuItem("Tema");
            _themeLightItem = new ToolStripMenuItem("Claro", null, (s, e) => { ApplyTheme(false); SaveSettings(); });
            _themeDarkItem = new ToolStripMenuItem("Escuro", null, (s, e) => { ApplyTheme(true); SaveSettings(); });
            temaMenu.DropDownItems.AddRange(new[] { _themeLightItem, _themeDarkItem });
            viewMenu.DropDownItems.Add(temaMenu);

            var helpMenu = new ToolStripMenuItem("Ajuda");
            helpMenu.DropDownItems.Add("Atalhos de Teclado", null, (s, e) => ShowShortcuts());
            helpMenu.DropDownItems.Add("Dicas de Uso", null, (s, e) => ShowTips());
            helpMenu.DropDownItems.Add("Documentação", null, (s, e) => ShowDocs());
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
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
            _toolStrip.Items.Add(new ToolStripSeparator());
            _searchLabel = new ToolStripLabel("Buscar:");
            _searchBox = new ToolStripTextBox
            {
                AutoSize = false,
                Width = 180
            };
            // Placeholder manual
            _searchBox.Tag = "placeholder";
            _searchBox.ForeColor = Color.Gray;
            _searchBox.Text = "Digite parte do nome";
            _searchBox.Enter += (s, e) =>
            {
                var tag = _searchBox.Tag as string;
                if (tag == "placeholder")
                {
                    _searchBox.Tag = null;
                    _searchBox.Text = "";
                    _searchBox.ForeColor = SystemColors.WindowText;
                }
            };
            _searchBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_searchBox.Text))
                {
                    _searchBox.Tag = "placeholder";
                    _searchBox.ForeColor = Color.Gray;
                    _searchBox.Text = "Digite parte do nome";
                }
            };
            _searchBox.TextChanged += (s, e) =>
            {
                var tag = _searchBox.Tag as string;
                if (tag == "placeholder") return; // não filtrar placeholder
                UpdateProcessListView();
            };
            _toolStrip.Items.Add(_searchLabel);
            _toolStrip.Items.Add(_searchBox);
            _filterModeCombo = new ToolStripComboBox
            {
                AutoSize = false,
                Width = 80,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _filterModeCombo.Items.AddRange(new[] { "Nome", "PID" });
            _filterModeCombo.SelectedIndex = 0;
            _filterModeCombo.SelectedIndexChanged += (s, e) => UpdateProcessListView();
            _toolStrip.Items.Add(new ToolStripLabel("Modo:"));
            _toolStrip.Items.Add(_filterModeCombo);
            _pauseButton = new ToolStripButton("Pausar", null, (s, e) => TogglePause()) { CheckOnClick = true };
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_pauseButton);
            

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
            _processListView.Columns.Add("Caminho", 280, HorizontalAlignment.Left);

            _processListView.SelectedIndexChanged += ProcessListView_SelectedIndexChanged;
            _processListView.DoubleClick += ProcessListView_DoubleClick;
            _processListView.ColumnClick += ProcessListView_ColumnClick;

            EnableDoubleBuffering(_processListView);

            // Context menu para o ListView
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Finalizar Processo", null, (s, e) => KillSelectedProcess());
            var prioridadeMenu = new ToolStripMenuItem("Alterar Prioridade");
            prioridadeMenu.DropDownItems.Add("Alta", null, (s, e) => ChangePriority(ProcessPriorityClass.High));
            prioridadeMenu.DropDownItems.Add("Normal", null, (s, e) => ChangePriority(ProcessPriorityClass.Normal));
            prioridadeMenu.DropDownItems.Add("Baixa", null, (s, e) => ChangePriority(ProcessPriorityClass.BelowNormal));
            contextMenu.Items.Add(prioridadeMenu);
            contextMenu.Items.Add("Abrir Local do Arquivo", null, (s, e) => OpenFileLocation());
            contextMenu.Items.Add("Copiar Nome", null, (s, e) => CopySelectedField(1));
            contextMenu.Items.Add("Copiar PID", null, (s, e) => CopySelectedField(0));
            contextMenu.Items.Add("Copiar Linha Completa", null, (s, e) => CopySelectedRow());
            contextMenu.Items.Add("Ver Detalhes", null, (s, e) => ShowProcessDetails());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Atualizar", null, (s, e) => LoadProcesses());
            _processListView.ContextMenuStrip = contextMenu;

            splitContainer.Panel1.Controls.Add(_processListView);

            // Painel de título e resumo (Dock.Top dentro Panel1)
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 240, // altura inicial maior para evitar corte inicial
                Padding = new Padding(10),
                BackColor = Color.FromArgb(234, 238, 242)
            };
            _titleLabel = new Label
            {
                Text = "Process Explorer - Visão Geral do Sistema",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Height = 40,
                ForeColor = Color.FromArgb(30, 50, 70)
            };
            _summaryBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(234, 238, 242),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ScrollBars = RichTextBoxScrollBars.None,
                WordWrap = true,
                ShortcutsEnabled = false,
                Text =
"Resumo do Programa:\n" +
"Este Process Explorer acadêmico fornece monitoramento em tempo real de processos do sistema, CPU, memória, threads e handles. \n" +
"Principais funcionalidades:\n" +
"• Atualização periódica configurável (1s, 2s, 5s).\n" +
"• Filtro rápido de processos por nome.\n" +
"• Destaque visual para processos com alto uso de CPU.\n" +
"• Painel de detalhes com propriedades completas do processo.\n" +
"• Gráficos de performance (CPU/Memória).\n" +
"• Exportação da lista para CSV.\n" +
"• Atalhos de teclado para produtividade (F5, Delete, Ctrl+F, Ctrl+G).\n" +
"• Menu de ajuda com dicas e documentação resumida.\n" +
"Utilize de forma responsável: finalizar processos críticos pode afetar a estabilidade do sistema." 
            };
            _headerPanel.Controls.Add(_summaryBox);
            _headerPanel.Controls.Add(_titleLabel);
            _headerUsageLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(50, 70, 90),
                Text = "Memória usada: -"
            };
            _headerPanel.Controls.Add(_headerUsageLabel);
            splitContainer.Panel1.Controls.Add(_headerPanel);

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
            this.Controls.Add(_statusStrip);        // status bar no rodapé
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
                _statusProcessesLabel.Text = "Carregando processos...";
                this.Cursor = Cursors.WaitCursor;

                // Executar operações pesadas em thread separada
                var (processes, stats) = await Task.Run(() =>
                {
                    var procs = _monitor.GetProcesses();
                    var st = _monitor.GetSystemStats();
                    return (procs, st);
                });

                // Atualizar UI na thread principal
                _currentProcesses = processes.ToList();
                UpdateProcessListView();

                // Atualizar barra de status
                _statusProcessesLabel.Text = $"Processos: {stats.ProcessCount} | Threads: {stats.ThreadCount} | Handles: {stats.HandleCount}";
                _statusCpuLabel.Text = $"CPU: {stats.TotalCpuUsage:F1}%";
                _statusMemLabel.Text = $"Memória: {stats.TotalMemoryUsedMB}";
                _statusLastUpdateLabel.Text = $"Última atualização: {DateTime.Now:HH:mm:ss}";
                _cpuProgressBar.Value = (int)Math.Min(100, stats.TotalCpuUsage);
                _memProgressBar.Value = (int)Math.Min(100, stats.MemoryUsagePercent);
                var totalPhysMB = stats.TotalPhysicalMemory > 0 ? (stats.TotalPhysicalMemory / 1024.0 / 1024.0) : 0;
                var availableMB = stats.TotalMemoryAvailable / 1024.0 / 1024.0;
                var usedMB = totalPhysMB > 0 ? (totalPhysMB - availableMB) : (stats.TotalMemoryUsed / 1024.0 / 1024.0);
                _headerUsageLabel.Text = totalPhysMB > 0
                    ? $"Memória usada: {usedMB:F0} MB de {totalPhysMB:F0} MB (Livre: {availableMB:F0} MB)"
                    : $"Memória usada (aprox soma processos): {usedMB:F0} MB | Livre: {availableMB:F0} MB";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar processos: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusProcessesLabel.Text = "Erro ao carregar processos";
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

        private void ProcessListView_SelectedIndexChanged(object sender, EventArgs e)
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

        private void ProcessListView_DoubleClick(object sender, EventArgs e)
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
            _statusIntervalLabel.Text = $"Intervalo: {milliseconds}ms";
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

        private void ShowShortcuts()
        {
            MessageBox.Show(
                "Atalhos Disponíveis:\n\n" +
                "F5: Atualizar lista\n" +
                "Delete: Finalizar processo selecionado\n" +
                "Enter: Ver detalhes do processo\n" +
                "Ctrl+F: Focar busca\n" +
                "Ctrl+G: Abrir gráficos de performance", "Atalhos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowTips()
        {
            MessageBox.Show(
                "Dicas de Uso:\n\n" +
                "1. Use a busca para filtrar processos rapidamente.\n" +
                "2. Dê duplo clique para ver detalhes.\n" +
                "3. Analise picos de CPU pelo destaque visual.\n" +
                "4. Abra gráficos para visão histórica.\n" +
                "5. Finalize apenas processos que você reconhece.", "Dicas de Uso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowDocs()
        {
            MessageBox.Show(
                "Documentação Resumida:\n\n" +
                "Este explorador demonstra coleta de informações de processos, uso de CPU, memória, threads e handles.\n" +
                "O monitor usa APIs do sistema operacional para consulta periódica e atualiza a interface de forma assíncrona.",
                "Documentação", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportProcesses()
        {
            if (_currentProcesses == null || _currentProcesses.Count == 0)
            {
                MessageBox.Show("Não há dados para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = "processos.csv",
                Title = "Exportar Lista de Processos"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var sw = new StreamWriter(sfd.FileName);
                    sw.WriteLine("PID;Nome;CPU%;Memória;Threads;Handles;Usuário;TempoExecução;Caminho");
                    foreach (var p in _currentProcesses)
                    {
                        sw.WriteLine($"{p.ProcessId};{p.ProcessName};{p.CpuUsage:F2};{p.WorkingSetMB};{p.ThreadCount};{p.HandleCount};{p.UserName};{p.RunningTime};{p.FilePath}");
                    }
                    MessageBox.Show("Exportação concluída.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Falha ao exportar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportProcessesJson()
        {
            if (_currentProcesses == null || _currentProcesses.Count == 0)
            {
                MessageBox.Show("Não há dados para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using var sfd = new SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                FileName = "processos.json",
                Title = "Exportar Lista de Processos (JSON)"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var exportObj = _currentProcesses.Select(p => new
                    {
                        p.ProcessId,
                        p.ProcessName,
                        p.CpuUsage,
                        WorkingSetMB = p.WorkingSetMB,
                        p.ThreadCount,
                        p.HandleCount,
                        p.UserName,
                        p.RunningTime,
                        p.FilePath
                    });
                    var json = JsonSerializer.Serialize(exportObj, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(sfd.FileName, json);
                    MessageBox.Show("Exportação JSON concluída.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Falha ao exportar JSON: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateProcessListView()
        {
            if (_currentProcesses == null) return;
            var tag = _searchBox.Tag as string;
            string filter = (tag == "placeholder") ? string.Empty : (_searchBox?.Text?.Trim() ?? string.Empty);
            IEnumerable<ProcessInfo> filteredEnum = _currentProcesses;
            if (!string.IsNullOrEmpty(filter))
            {
                if (_filterModeCombo != null && _filterModeCombo.SelectedItem?.ToString() == "PID")
                {
                    if (int.TryParse(filter, out int pid))
                        filteredEnum = filteredEnum.Where(p => p.ProcessId == pid);
                    else
                        filteredEnum = Enumerable.Empty<ProcessInfo>();
                }
                else
                {
                    filteredEnum = filteredEnum.Where(p => p.ProcessName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }
            var filtered = filteredEnum.ToList();

            _processListView.BeginUpdate();
            _processListView.Items.Clear();
            foreach (var proc in filtered)
            {
                var item = new ListViewItem(proc.ProcessId.ToString());
                item.SubItems.Add(proc.ProcessName);
                item.SubItems.Add($"{proc.CpuUsage:F2}");
                item.SubItems.Add(proc.WorkingSetMB);
                item.SubItems.Add(proc.ThreadCount.ToString());
                item.SubItems.Add(proc.HandleCount.ToString());
                item.SubItems.Add(proc.UserName);
                item.SubItems.Add(proc.RunningTime);
                item.SubItems.Add(proc.FilePath);
                item.Tag = proc;
                if (proc.CpuUsage > 50)
                    item.BackColor = Color.FromArgb(255, 204, 204);
                else if (proc.CpuUsage > 25)
                    item.BackColor = Color.FromArgb(255, 249, 196);
                _processListView.Items.Add(item);
            }
            _processListView.EndUpdate();
            UpdateSortHeaderIndicators();
        }

        private void ProcessListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_sortColumn == e.Column)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                _sortColumn = e.Column;
                _sortAscending = true;
            }

            var items = _processListView.Items.Cast<ListViewItem>().ToList();
            Func<ListViewItem, object> keySelector = item => item.SubItems[e.Column].Text;

            if (e.Column == 0 || e.Column == 2 || e.Column == 3 || e.Column == 4 || e.Column == 5)
            {
                keySelector = item =>
                {
                    var txt = item.SubItems[e.Column].Text.Replace("%", "").Replace(" MB", "");
                    if (double.TryParse(txt, out double val)) return val;
                    return -1d;
                };
            }

            items = _sortAscending
                ? items.OrderBy(keySelector).ToList()
                : items.OrderByDescending(keySelector).ToList();

            _processListView.BeginUpdate();
            _processListView.Items.Clear();
            foreach (var it in items) _processListView.Items.Add(it);
            _processListView.EndUpdate();
            UpdateSortHeaderIndicators();
        }

        private void EnableDoubleBuffering(ListView listView)
        {
            // Reduz flicker em atualizações frequentes
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(listView, true, null);
        }

        private void UpdateSortHeaderIndicators()
        {
            if (_sortColumn < 0) return;
            for (int i = 0; i < _processListView.Columns.Count; i++)
            {
                var col = _processListView.Columns[i];
                string baseText = col.Text.Split(' ')[0];
                if (i == _sortColumn)
                {
                    col.Text = baseText + (_sortAscending ? " ▲" : " ▼");
                }
                else
                {
                    col.Text = baseText;
                }
            }
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                _refreshTimer.Stop();
                _pauseButton.Text = "Retomar";
                _statusLastUpdateLabel.Text = "Atualização pausada";
            }
            else
            {
                _refreshTimer.Start();
                _pauseButton.Text = "Pausar";
            }
        }

        private void ChangePriority(ProcessPriorityClass priority)
        {
            if (_selectedProcessId == -1) return;
            try
            {
                var proc = Process.GetProcessById(_selectedProcessId);
                proc.PriorityClass = priority;
                MessageBox.Show($"Prioridade alterada para {priority}.", "Prioridade", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha ao alterar prioridade: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenFileLocation()
        {
            if (_selectedProcessId == -1) return;
            try
            {
                var proc = Process.GetProcessById(_selectedProcessId);
                string path;
                try { path = proc.MainModule?.FileName ?? string.Empty; }
                catch { path = string.Empty; }
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    MessageBox.Show("Caminho indisponível.", "Abrir Local", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha ao abrir local: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopySelectedField(int subIndex)
        {
            if (_processListView.SelectedItems.Count == 0) return;
            var item = _processListView.SelectedItems[0];
            if (subIndex < item.SubItems.Count)
                Clipboard.SetText(item.SubItems[subIndex].Text);
        }

        private void CopySelectedRow()
        {
            if (_processListView.SelectedItems.Count == 0) return;
            var item = _processListView.SelectedItems[0];
            var parts = item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text);
            Clipboard.SetText(string.Join(";", parts));
        }

        private void ApplyTheme(bool dark)
        {
            Color back;
            Color header;
            Color fore;
            if (dark)
            {
                back = Color.FromArgb(32, 37, 43);
                header = Color.FromArgb(45, 52, 60);
                fore = Color.Gainsboro;
            }
            else
            {
                back = Color.FromArgb(248, 250, 252);
                header = Color.FromArgb(234, 238, 242);
                fore = Color.FromArgb(30, 50, 70);
            }
            this.BackColor = back;
            if (_headerPanel != null) _headerPanel.BackColor = header;
            if (_summaryBox != null)
            {
                _summaryBox.BackColor = header;
                _summaryBox.ForeColor = dark ? Color.LightGray : Color.Black;
            }
            if (_titleLabel != null) _titleLabel.ForeColor = fore;
            if (_processListView != null)
            {
                _processListView.BackColor = dark ? Color.FromArgb(40, 44, 52) : Color.White;
                _processListView.ForeColor = dark ? Color.Gainsboro : Color.Black;
            }
            if (_statusStrip != null) _statusStrip.BackColor = header;
            if (_menuStrip != null) _menuStrip.BackColor = header;
            if (_toolStrip != null) _toolStrip.BackColor = header;
            if (_themeDarkItem != null) _themeDarkItem.Checked = dark;
            if (_themeLightItem != null) _themeLightItem.Checked = !dark;
        }

        private void SaveSettings()
        {
            try
            {
                var dark = _themeDarkItem != null && _themeDarkItem.Checked;
                File.WriteAllText(_settingsFilePath, $"dark={(dark ? 1 : 0)}");
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var content = File.ReadAllText(_settingsFilePath);
                    var dark = content.Contains("dark=1");
                    ApplyTheme(dark);
                }
            }
            catch { }
        }

        private void AdjustHeaderHeight()
        {
            if (_summaryBox == null || _headerPanel == null) return;
            try
            {
                // Garantir medição consistente após layout
                int availableWidth = _headerPanel.ClientSize.Width - _headerPanel.Padding.Horizontal - 4;
                if (availableWidth < 200) availableWidth = 200;
                // Força recalcular WordWrap
                _summaryBox.Width = availableWidth;
                // Usa TextRenderer para altura total aproximada
                var size = TextRenderer.MeasureText(_summaryBox.Text + "\n", _summaryBox.Font, new Size(availableWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                // Alternativa precisa via posição do último caractere
                int lastChar = Math.Max(0, _summaryBox.TextLength - 1);
                var pt = _summaryBox.GetPositionFromCharIndex(lastChar);
                int textBottom = pt.Y + _summaryBox.Font.Height + 6; // margem
                int measured = Math.Max(size.Height, textBottom);
                int desired = measured + _titleLabel.Height + _headerPanel.Padding.Vertical + 4;
                // Remover limite máximo para evitar corte; garantir mínimo razoável
                if (desired < 160) desired = 160;
                _headerPanel.Height = desired;
            }
            catch
            {
                // fallback simples
                _headerPanel.Height = 240;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadProcesses();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                KillSelectedProcess();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ShowProcessDetails();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                _searchBox.Focus();
                _searchBox.SelectAll();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                ShowPerformanceGraphs();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedRow();
                e.Handled = true;
            }
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
