# Conceitos de Sistemas Operacionais Demonstrados

## 📚 Documento Complementar - Trabalho de SO

Este documento explica em detalhes os conceitos de Sistemas Operacionais implementados no projeto Process Explorer.

---

## 1. Processos

### 1.1 O que é um Processo?

Um **processo** é um programa em execução. Quando você executa um arquivo executável (.exe), o sistema operacional cria um processo que contém:
- **Código** do programa (instruções)
- **Dados** (variáveis, heap)
- **Stack** (pilha de execução)
- **Process Control Block (PCB)** com informações de controle

### 1.2 Process ID (PID)

Cada processo recebe um identificador único chamado **PID** (Process ID). No Windows:
- PIDs são números inteiros positivos
- São únicos enquanto o processo está em execução
- Podem ser reutilizados após o processo terminar

**No código:**
```csharp
process.Id  // Retorna o PID do processo
```

### 1.3 Estados de um Processo

Embora não implementado visualmente, processos podem estar em diferentes estados:
- **New**: Processo sendo criado
- **Running**: Executando na CPU
- **Waiting**: Aguardando I/O ou evento
- **Ready**: Pronto para executar
- **Terminated**: Processo finalizado

### 1.4 Término de Processos

O método `Kill()` envia um sinal de término para o processo:
```csharp
process.Kill();  // Força o término do processo
```

Isso demonstra como o SO gerencia o ciclo de vida dos processos.

---

## 2. Threads

### 2.1 O que são Threads?

**Threads** são unidades de execução dentro de um processo. Um processo pode ter múltiplas threads:
- Compartilham o mesmo espaço de memória
- Possuem seus próprios registradores e stack
- Permitem execução paralela

### 2.2 Multithreading

A maioria dos processos modernos usa múltiplas threads:
- **Thread principal**: Criada com o processo
- **Threads de trabalho**: Criadas para tarefas específicas
- **Threads de I/O**: Para operações assíncronas

**No código:**
```csharp
process.Threads.Count  // Número total de threads do processo
```

### 2.3 Benefícios do Multithreading

1. **Responsividade**: UI pode permanecer responsiva enquanto processa dados
2. **Compartilhamento de recursos**: Threads compartilham memória
3. **Economia**: Threads são mais leves que processos
4. **Escalabilidade**: Aproveita múltiplos núcleos de CPU

---

## 3. Escalonamento de CPU

### 3.1 O que é Escalonamento?

O **escalonador (scheduler)** do SO decide qual processo/thread executar e por quanto tempo. Isso é necessário porque:
- Existem mais processos que CPUs
- Cria a ilusão de execução simultânea
- Maximiza utilização da CPU

### 3.2 Tempo de CPU

O SO mantém registro de quanto tempo de CPU cada processo consome:

**No código:**
```csharp
process.TotalProcessorTime  // Tempo total de CPU usado
```

### 3.3 Cálculo de Uso de CPU

O percentual de CPU é calculado comparando duas medições:

```csharp
// Fórmula implementada
CPU% = (ΔProcessorTime / ΔRealTime) * 100 / NumProcessors

// Onde:
// ΔProcessorTime = Tempo de CPU usado entre medições
// ΔRealTime = Tempo real decorrido
// NumProcessors = Número de núcleos lógicos
```

### 3.4 Múltiplos Núcleos

Processadores modernos têm múltiplos núcleos:
- Um processo pode usar até 100% * NumCores
- Dividimos por `Environment.ProcessorCount` para normalizar
- Threads podem executar em núcleos diferentes

---

## 4. Gerenciamento de Memória

### 4.1 Tipos de Memória no Windows

#### Working Set (Conjunto de Trabalho)
- **Memória física (RAM)** atualmente ocupada pelo processo
- Páginas de memória que estão na RAM
- Pode variar conforme o SO faz paginação

**No código:**
```csharp
process.WorkingSet64  // Memória física em bytes
```

#### Private Bytes (Bytes Privados)
- Memória alocada exclusivamente para o processo
- Não pode ser compartilhada com outros processos
- Inclui heap e stack privados

```csharp
process.PrivateMemorySize64  // Memória privada em bytes
```

### 4.2 Memória Virtual

O Windows usa **memória virtual**:
- Cada processo tem seu próprio espaço de endereçamento
- Endereços virtuais são mapeados para endereços físicos
- Permite mais memória que a RAM física disponível

### 4.3 Paginação

Quando a RAM está cheia, o SO usa **paginação**:
1. Move páginas menos usadas para o disco (Page File)
2. Libera RAM para processos ativos
3. Carrega páginas de volta quando necessário

### 4.4 Cálculo de Uso de Memória

```csharp
// Memória total usada por todos os processos
TotalMemoryUsed = Σ(process.WorkingSet64)

// Percentual de uso
MemoryUsage% = (TotalMemoryUsed / TotalPhysicalMemory) * 100
```

---

## 5. Handles e Recursos do Sistema

### 5.1 O que são Handles?

**Handles** são referências a recursos do sistema operacional:
- Arquivos abertos
- Sockets de rede
- Chaves de registro
- Eventos e semáforos
- Threads
- Janelas

**No código:**
```csharp
process.HandleCount  // Número de handles abertos
```

### 5.2 Por que são Importantes?

- SO limita o número de handles por processo
- Muitos handles podem indicar vazamento de recursos
- Cada handle consome memória do kernel

### 5.3 Gerenciamento de Recursos

O SO é responsável por:
1. Criar handles quando um recurso é solicitado
2. Rastrear quais processos usam quais recursos
3. Liberar recursos quando handles são fechados
4. Prevenir vazamento de recursos

---

## 6. Permissões e Segurança

### 6.1 Proprietário do Processo

Cada processo executa sob um contexto de usuário:
- **SYSTEM**: Serviços do Windows
- **Administrador**: Programas elevados
- **Usuário**: Aplicações normais

**No código (usando WMI):**
```csharp
Win32_Process.GetOwner()  // Retorna o proprietário
```

### 6.2 Níveis de Privilégio

Windows implementa níveis de privilégio:
- **Kernel Mode**: Acesso total ao hardware
- **User Mode**: Acesso restrito
- **UAC**: User Account Control

### 6.3 Acesso Negado

Algumas operações falham por falta de permissão:
- Ler informações de processos do sistema
- Finalizar processos críticos
- Acessar memória de outros processos

```csharp
try {
    var path = process.MainModule?.FileName;
} catch {
    // Acesso negado - processo protegido
}
```

---

## 7. Performance Counters

### 7.1 O que são Performance Counters?

**Performance Counters** são métricas do sistema mantidas pelo Windows:
- CPU total
- Memória disponível
- Disco I/O
- Rede
- E muito mais

**No código:**
```csharp
var cpuCounter = new PerformanceCounter(
    "Processor",           // Categoria
    "% Processor Time",    // Nome do contador
    "_Total"               // Instância (total de todos os núcleos)
);

var cpuUsage = cpuCounter.NextValue();
```

### 7.2 Categorias Usadas no Projeto

1. **Processor**
   - `% Processor Time` - Percentual de uso da CPU

2. **Memory**
   - `Available MBytes` - Memória RAM disponível

### 7.3 Como Funcionam?

- SO mantém contadores em tempo real
- Aplicações podem ler esses contadores
- Primeira leitura geralmente retorna 0 (precisa de baseline)

---

## 8. WMI (Windows Management Instrumentation)

### 8.1 O que é WMI?

**WMI** é uma infraestrutura para gerenciar sistemas Windows:
- Acessa informações detalhadas do sistema
- Permite consultas tipo SQL
- Fornece dados que APIs normais não expõem

### 8.2 Uso no Projeto

Obtemos o proprietário do processo:
```csharp
var query = $"SELECT * FROM Win32_Process WHERE ProcessId = {pid}";
using var searcher = new ManagementObjectSearcher(query);

foreach (ManagementObject obj in searcher.Get())
{
    var ownerInfo = new string[2];
    obj.InvokeMethod("GetOwner", ownerInfo);
    return ownerInfo[0];  // Nome do usuário
}
```

### 8.3 Classes WMI Relevantes

- `Win32_Process` - Informações de processos
- `Win32_OperatingSystem` - Info do SO
- `Win32_Processor` - Info da CPU
- `Win32_PhysicalMemory` - Info de RAM

---

## 9. Monitoramento em Tempo Real

### 9.1 Timer e Atualização Periódica

O projeto usa um `Timer` para atualização automática:
```csharp
var timer = new Timer();
timer.Interval = 2000;  // 2 segundos
timer.Tick += (s, e) => AtualizarProcessos();
timer.Start();
```

### 9.2 Desafios do Monitoramento

1. **Performance**: Coletar dados consome recursos
2. **Precisão**: Valores mudam rapidamente
3. **Sincronização**: Processos podem terminar durante a leitura

### 9.3 Estratégias Implementadas

- **Tratamento de exceções**: Processos podem terminar
- **Cache de valores**: Para cálculos de CPU
- **Throttling**: Limitar frequência de atualização

---

## 10. Conceitos Avançados

### 10.1 Context Switching

Quando o SO alterna entre processos:
1. Salva o estado do processo atual (registradores, PC)
2. Carrega o estado do próximo processo
3. Retoma execução

**Overhead**: Context switching consome tempo de CPU

### 10.2 Prioridade de Processos

Processos têm diferentes prioridades:
- **Realtime**: Maior prioridade
- **High**: Alta prioridade
- **Normal**: Padrão
- **Low**: Baixa prioridade

### 10.3 Afinidade de CPU

Define em quais núcleos um processo pode executar:
```csharp
process.ProcessorAffinity  // Máscara de bits de CPUs
```

### 10.4 Sinais e Comunicação

Processos se comunicam através de:
- **Signals**: Notificações (ex: SIGKILL)
- **Pipes**: Comunicação unidirecional
- **Shared Memory**: Memória compartilhada
- **Sockets**: Comunicação em rede

---

## 📊 Métricas e Análise

### Como Interpretar os Dados

**CPU Alta (>50%)**
- Processo fazendo processamento intensivo
- Pode estar em loop infinito
- Pode estar minerando/compilando

**Memória Alta**
- Vazamento de memória
- Processamento de grandes volumes de dados
- Múltiplas instâncias

**Muitos Handles**
- Possível vazamento de recursos
- Aplicação com muitos arquivos/conexões abertos

**Muitas Threads**
- Aplicação multithread complexa
- Pode indicar problema de design
- Servidores web/banco de dados são normais

---

## 🎓 Conclusão

Este projeto demonstra conceitos fundamentais de Sistemas Operacionais:

1. ✅ **Gerenciamento de Processos**: Criação, listagem, término
2. ✅ **Escalonamento**: Cálculo e monitoramento de CPU
3. ✅ **Memória**: Working set, memória virtual, paginação
4. ✅ **Threads**: Contagem e conceitos de multithreading
5. ✅ **Recursos**: Handles e gerenciamento de recursos
6. ✅ **Segurança**: Permissões e proprietários
7. ✅ **Monitoramento**: Performance counters e WMI

Cada linha de código implementada reflete um conceito teórico estudado na disciplina, proporcionando aprendizado prático sobre como Sistemas Operacionais funcionam nos bastidores.

---

**Referências Acadêmicas:**
- Tanenbaum, A. S. - Modern Operating Systems
- Silberschatz, A. - Operating System Concepts
- Stallings, W. - Operating Systems: Internals and Design Principles
- Microsoft Documentation - Windows Internals
