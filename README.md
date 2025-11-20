# Simple Process Explorer

**Trabalho Final - Sistemas Operacionais**

Réplica simplificada do Process Explorer (Sysinternals) desenvolvida em C# com Windows Forms.

## 📋 Descrição do Projeto

Este projeto é uma aplicação educacional que demonstra conceitos fundamentais de Sistemas Operacionais através de um monitor de processos em tempo real. A aplicação permite visualizar, analisar e gerenciar processos do sistema Windows, similar ao Process Explorer da Sysinternals.

## 🎯 Objetivos de Aprendizado

O projeto demonstra os seguintes conceitos de Sistemas Operacionais:

### 1. **Gerenciamento de Processos**
- Listagem de processos em execução
- Identificação de processos (PID - Process ID)
- Hierarquia e estados de processos
- Criação e término de processos

### 2. **Gerenciamento de Memória**
- Working Set (memória física utilizada)
- Private Bytes (memória privada do processo)
- Monitoramento de uso de memória do sistema

### 3. **Escalonamento e CPU**
- Cálculo de uso de CPU por processo
- Tempo de processador (TotalProcessorTime)
- Distribuição de CPU entre múltiplos núcleos

### 4. **Threads**
- Contagem de threads por processo
- Conceito de multithreading

### 5. **Recursos do Sistema**
- Handles (referências a recursos do SO)
- Permissões e proprietários de processos
- Informações do sistema em tempo real

## 🚀 Funcionalidades

### Interface Principal
- ✅ **Lista de Processos em Tempo Real**
  - PID (Process ID)
  - Nome do processo
  - Uso de CPU (%)
  - Uso de Memória (MB)
  - Número de Threads
  - Número de Handles
  - Usuário/Proprietário
  - Tempo de execução

- ✅ **Atualização Automática**
  - Configurável (1, 2 ou 5 segundos)
  - Cores indicativas de alto uso de CPU

- ✅ **Gerenciamento de Processos**
  - Finalizar processos selecionados
  - Confirmação de segurança

- ✅ **Painel de Detalhes**
  - Visualização detalhada de processos selecionados
  - Informações completas através do PropertyGrid

### Monitor de Performance
- 📊 **Gráficos em Tempo Real**
  - Gráfico de uso de CPU do sistema
  - Gráfico de uso de Memória
  - Histórico visual de 60 segundos

- 📈 **Estatísticas do Sistema**
  - Total de processos
  - Total de threads
  - Memória disponível/usada

## 🛠️ Tecnologias Utilizadas

- **Linguagem**: C# (.NET 8.0)
- **Framework UI**: Windows Forms
- **APIs do Sistema**:
  - `System.Diagnostics.Process` - Gerenciamento de processos
  - `System.Diagnostics.PerformanceCounter` - Contadores de performance
  - `System.Management` (WMI) - Informações avançadas do sistema

## 📦 Estrutura do Projeto

```
ProcessExplorer/
│
├── ProcessExplorer.csproj          # Arquivo do projeto
├── Program.cs                       # Ponto de entrada da aplicação
├── MainForm.cs                      # Formulário principal
│
├── Models/                          # Modelos de dados
│   ├── ProcessInfo.cs              # Informações de processo
│   └── SystemStats.cs              # Estatísticas do sistema
│
├── Core/                            # Lógica de negócio
│   └── ProcessMonitor.cs           # Monitoramento de processos
│
├── Forms/                           # Formulários adicionais
│   └── PerformanceMonitorForm.cs   # Gráficos de performance
│
└── Controls/                        # Controles personalizados
    └── PerformanceGraph.cs         # Componente de gráfico
```

## 🔧 Como Compilar e Executar

### Pré-requisitos
- Windows 10 ou superior
- .NET 8.0 SDK ou superior
- Visual Studio 2022 ou VS Code (opcional)

### Compilação via Linha de Comando

1. **Abra o terminal** na pasta do projeto

2. **Restaure as dependências**:
```bash
dotnet restore
```

3. **Compile o projeto**:
```bash
dotnet build
```

4. **Execute a aplicação**:
```bash
dotnet run
```

### Compilação via Visual Studio

1. Abra o arquivo `ProcessExplorer.csproj` no Visual Studio
2. Pressione `F5` ou clique em "Iniciar" para compilar e executar
3. Para criar um executável publicável:
   - Clique com botão direito no projeto → Publicar
   - Configure o perfil de publicação
   - Publique a aplicação

### Gerar Executável Standalone

Para criar um executável independente que não requer .NET instalado:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

O executável estará em: `bin\Release\net8.0-windows\win-x64\publish\`

## 📖 Como Usar

### Navegação Básica

1. **Visualizar Processos**
   - A lista de processos é atualizada automaticamente
   - Processos com alto uso de CPU aparecem destacados em cores

2. **Ordenar Processos**
   - Clique nas colunas para ordenar

3. **Ver Detalhes**
   - Duplo clique em um processo
   - Ou clique com botão direito → "Ver Detalhes"

4. **Finalizar Processo**
   - Selecione um processo
   - Clique em "Finalizar Processo" na barra de ferramentas
   - Ou botão direito → "Finalizar Processo"
   - Confirme a ação

5. **Gráficos de Performance**
   - Menu: Visualizar → Gráficos de Performance
   - Visualize uso de CPU e Memória em tempo real

6. **Ajustar Taxa de Atualização**
   - Menu: Visualizar → Selecione intervalo desejado

### Atalhos

- **F5**: Atualizar lista manualmente
- **Alt + F4**: Fechar aplicação

## 🔍 Conceitos Técnicos Implementados

### 1. Coleta de Informações de Processos
```csharp
Process.GetProcesses() // Obtém lista de todos os processos
process.Id              // PID do processo
process.ProcessName     // Nome do processo
process.Threads.Count   // Número de threads
```

### 2. Cálculo de Uso de CPU
O uso de CPU é calculado comparando o tempo de processador entre duas medições:
```csharp
CPU% = (ΔProcessorTime / ΔRealTime) * 100 / NumProcessors
```

### 3. Informações de Memória
- **Working Set**: Memória física (RAM) atualmente em uso
- **Private Bytes**: Memória alocada exclusivamente pelo processo

### 4. WMI (Windows Management Instrumentation)
Utilizado para obter informações avançadas como o proprietário do processo:
```csharp
Win32_Process.GetOwner()
```

### 5. Performance Counters
Contadores de sistema para CPU e memória total:
```csharp
PerformanceCounter("Processor", "% Processor Time", "_Total")
PerformanceCounter("Memory", "Available MBytes")
```

## ⚠️ Permissões e Limitações

- **Processos do Sistema**: Alguns processos requerem privilégios administrativos
- **Acesso Negado**: Processos protegidos podem não exibir todas as informações
- **Performance**: A atualização frequente pode consumir recursos
- **Windows Only**: Aplicação desenvolvida especificamente para Windows

## 📚 Referências

- [Microsoft Docs - Process Class](https://docs.microsoft.com/dotnet/api/system.diagnostics.process)
- [Microsoft Docs - PerformanceCounter](https://docs.microsoft.com/dotnet/api/system.diagnostics.performancecounter)
- [Sysinternals Process Explorer](https://docs.microsoft.com/sysinternals/downloads/process-explorer)
- Silberschatz, A., Galvin, P. B., & Gagne, G. - Operating System Concepts

## 👨‍💻 Desenvolvimento

**Autor**: Estudante de Ciência da Computação
**Disciplina**: Sistemas Operacionais
**Versão**: 1.0.0
**Data**: 2025

## 📝 Licença

Este projeto é desenvolvido para fins educacionais como trabalho acadêmico da disciplina de Sistemas Operacionais.

## 🤝 Contribuições

Este é um projeto acadêmico. Sugestões e melhorias são bem-vindas para fins de aprendizado.

---

**Nota**: Execute a aplicação com privilégios administrativos para acessar informações completas de todos os processos do sistema.
