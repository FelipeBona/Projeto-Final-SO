# 🚀 Como Executar o Process Explorer

## ✅ Métodos de Execução (do mais fácil para o mais técnico)

### Método 1: Duplo Clique no Executável ⭐ MAIS FÁCIL
1. Navegue até a pasta do projeto
2. Entre em: `bin\Release\net8.0-windows\`
3. **Duplo clique** em `ProcessExplorer.exe`

---

### Método 2: Usar o Script executar.bat
```bash
# Clique duas vezes em:
executar.bat
```

---

### Método 3: Usar o build.bat (Compila + Executa)
```bash
# Clique duas vezes em:
build.bat
```

---

### Método 4: Via Explorador de Arquivos
1. Abra o Explorador de Arquivos
2. Navegue até:
   ```
   C:\Users\Administrador\OneDrive\Área de Trabalho\Trabalho Final - SO\Projeto c-sharp\bin\Release\net8.0-windows
   ```
3. Duplo clique em `ProcessExplorer.exe`

---

### Método 5: Via Linha de Comando
```bash
# Abra o PowerShell ou CMD na pasta do projeto e execute:
cd bin\Release\net8.0-windows
.\ProcessExplorer.exe
```

---

### Método 6: Visual Studio
1. Abra o arquivo `ProcessExplorer.csproj` no Visual Studio
2. Pressione `F5` ou clique no botão ▶️ "Iniciar"

---

## 🔧 Solução de Problemas

### Problema 1: "Nada acontece ao executar"

**Possível causa:** Aplicação pode estar rodando em segundo plano

**Solução:**
1. Pressione `Ctrl+Shift+Esc` para abrir o Gerenciador de Tarefas
2. Procure por "ProcessExplorer" na lista
3. Se estiver lá, clique com botão direito → "Trazer para frente"
4. Se não funcionar, finalize e tente novamente

---

### Problema 2: "Executável não encontrado"

**Solução:** Compile primeiro
```bash
dotnet build -c Release
```

Ou use:
```bash
build.bat
```

---

### Problema 3: "Erro ao executar dotnet run"

**Solução:** Execute o .exe diretamente em vez de usar `dotnet run`
```bash
cd bin\Release\net8.0-windows
.\ProcessExplorer.exe
```

---

### Problema 4: "Precisa de .NET Runtime"

**Se você não tem o .NET instalado**, gere um executável standalone:

```bash
# Use o script:
publish.bat

# Ou manualmente:
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

O executável independente estará em:
```
bin\Release\net8.0-windows\win-x64\publish\ProcessExplorer.exe
```

Este arquivo **NÃO precisa do .NET instalado** e pode ser copiado para qualquer PC Windows.

---

### Problema 5: "Acesso Negado ao ver processos"

**Solução:** Execute como Administrador

1. Clique com botão direito em `ProcessExplorer.exe`
2. Selecione **"Executar como administrador"**
3. Clique "Sim" na janela do UAC

**Nota:** Mesmo como administrador, alguns processos do sistema permanecerão protegidos (isso é normal!).

---

### Problema 6: "Janela abre e fecha rapidamente"

**Possível causa:** Erro em tempo de execução

**Solução:** Execute via terminal para ver o erro
```bash
cd bin\Release\net8.0-windows
.\ProcessExplorer.exe
```

Ou veja os logs no Visual Studio (F5 em modo Debug).

---

### Problema 7: "Muitas exceções Win32 no Visual Studio"

**Isso é NORMAL!** ✅

Leia o arquivo `SOLUCAO_EXCECOES.md` para entender por quê.

**Solução rápida:**
1. Visual Studio → Debug → Windows → Exception Settings (`Ctrl+Alt+E`)
2. Desmarque `System.ComponentModel.Win32Exception`

---

## 🎯 Teste Rápido

### Se a aplicação abriu corretamente, você deve ver:

✅ Uma janela com o título "Simple Process Explorer - Trabalho de Sistemas Operacionais"
✅ Uma lista de processos com várias colunas (PID, Nome, CPU%, etc.)
✅ A lista atualizando automaticamente a cada 2 segundos
✅ Menu superior com "Arquivo", "Visualizar", "Ajuda"
✅ Barra de ferramentas com botões
✅ Barra de status na parte inferior com estatísticas

### Se você vê tudo isso: 🎉 FUNCIONANDO PERFEITAMENTE!

---

## 📊 Verificação de Funcionalidades

Execute estes testes para confirmar que tudo funciona:

### ✅ Teste 1: Lista de Processos
- [ ] Vejo vários processos na lista
- [ ] A lista atualiza sozinha
- [ ] Consigo clicar em um processo

### ✅ Teste 2: Informações
- [ ] Vejo PID, nome, CPU%, memória
- [ ] Alguns processos mostram valores (não todos zero)

### ✅ Teste 3: Menu
- [ ] Menu "Arquivo" funciona
- [ ] Menu "Visualizar" funciona
- [ ] Menu "Ajuda" → "Sobre" mostra informações

### ✅ Teste 4: Gráficos
- [ ] Menu → Visualizar → Gráficos de Performance
- [ ] Janela nova abre com gráficos
- [ ] Gráficos atualizam em tempo real

### ✅ Teste 5: Finalizar Processo
- [ ] Abro o Bloco de Notas (notepad)
- [ ] Vejo ele aparecer na lista
- [ ] Seleciono e clico "Finalizar Processo"
- [ ] Aparece confirmação
- [ ] Clico "Sim" e o Bloco de Notas fecha

**Se todos os testes passaram: ✅ TUDO FUNCIONANDO!**

---

## 💡 Dicas Importantes

### Para Desenvolvimento/Testes:
```bash
# Compile em Debug (mais informações de erro)
dotnet build -c Debug
dotnet run -c Debug
```

### Para Apresentação:
```bash
# Compile em Release (otimizado, sem warnings)
dotnet build -c Release

# Execute o .exe diretamente
cd bin\Release\net8.0-windows
.\ProcessExplorer.exe
```

### Para Distribuir:
```bash
# Gere executável standalone
publish.bat

# Ou:
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# O arquivo estará em:
# bin\Release\net8.0-windows\win-x64\publish\ProcessExplorer.exe
```

---

## 🔍 Comandos de Diagnóstico

### Verificar se o .NET está instalado:
```bash
dotnet --version
```

**Esperado:** `8.0.xxx` ou superior

### Verificar se o projeto compila:
```bash
dotnet build
```

**Esperado:** "Compilação com êxito" (warnings são ok)

### Verificar se o executável existe:
```bash
dir bin\Release\net8.0-windows\ProcessExplorer.exe
```

**Esperado:** Deve mostrar o arquivo com tamanho (~150 KB)

### Ver processos .NET rodando:
```bash
dotnet --list-runtimes
```

---

## 📁 Estrutura de Pastas de Build

```
Projeto c-sharp/
│
├── bin/
│   ├── Debug/
│   │   └── net8.0-windows/
│   │       └── ProcessExplorer.exe    (Debug)
│   │
│   └── Release/
│       └── net8.0-windows/
│           ├── ProcessExplorer.exe    ⭐ Execute este!
│           ├── ProcessExplorer.dll
│           └── [outros arquivos]
│
└── obj/
    └── [arquivos temporários]
```

---

## ✅ Checklist Executar pela Primeira Vez

- [ ] .NET 8.0 SDK instalado (`dotnet --version`)
- [ ] Projeto compilado (`dotnet build -c Release`)
- [ ] Executável existe (`bin\Release\net8.0-windows\ProcessExplorer.exe`)
- [ ] Duplo clique no executável
- [ ] Janela abre mostrando processos
- [ ] Tudo funciona!

---

## 🆘 Ainda Não Funciona?

Se depois de tentar tudo ainda não funcionar:

1. **Verifique o .NET:**
   ```bash
   dotnet --version
   ```
   Deve ser 8.0 ou superior

2. **Recompile do zero:**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build -c Release
   ```

3. **Execute em modo Debug para ver erros:**
   ```bash
   dotnet run -c Debug
   ```

4. **Verifique o Windows Defender:**
   - Pode estar bloqueando o executável
   - Adicione exceção se necessário

5. **Execute via Visual Studio:**
   - Abre melhor visualização de erros
   - Pressione F5 para debugar

---

## 📞 Comandos Rápidos

### Compilar
```bash
dotnet build -c Release
```

### Executar (após compilar)
```bash
bin\Release\net8.0-windows\ProcessExplorer.exe
```

### Compilar + Executar (um comando)
```bash
dotnet build -c Release && bin\Release\net8.0-windows\ProcessExplorer.exe
```

### Limpar e recompilar
```bash
dotnet clean
dotnet build -c Release
```

---

## 🎯 Resumo Rápido

**Forma mais fácil de executar:**

1. Navegue até: `bin\Release\net8.0-windows\`
2. Duplo clique em: `ProcessExplorer.exe`
3. Pronto! ✅

**Forma mais completa:**

1. Execute: `build.bat`
2. Aplicação abre automaticamente
3. Pronto! ✅

---

**Se ainda tiver problemas, revise o `README.md` ou consulte os outros arquivos de documentação.**

Boa execução! 🚀
