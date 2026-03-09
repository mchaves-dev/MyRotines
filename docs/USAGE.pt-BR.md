# Guia de Utilização - MyRotines

## 1. Visão geral

`MyRotines` é uma CLI para automação operacional de demandas:
- download, extração e movimentação de arquivos
- monitoramento contínuo de diretórios
- limpeza por retenção
- restore de backup SQL Server
- pipeline de steps customizável (OCP)

## 2. Pré-requisitos

- Windows com `.NET SDK` compatível com `net9.0`
- Para restore SQL: `sqlcmd` disponível no `PATH`

Build sucinto:
```bat
dotnet build .\MyRotines.sln -v minimal
```

## 3. Regra importante para URLs no CMD

Se a URL tiver `&` (ex.: Dropbox com `...&dl=1`), use aspas duplas.

Correto:
```bat
MyRotines.exe routine --url "https://www.dropbox.com/...zip?rlkey=abc&dl=1" --move-extract --move-to "c:\demandas\entrada"
```

## 4. Comandos

### 4.1 `profiles`
Lista profiles e steps disponíveis.

```bat
MyRotines.exe profiles
```

### 4.2 `inventory`
Inventário por extensão e tamanho.

```bat
MyRotines.exe inventory --path "c:\demandas" --recursive
```

### 4.3 `cleanup-files`
Limpeza de arquivos antigos por retenção.

```bat
MyRotines.exe cleanup-files --path "c:\demandas\entrada" --pattern "*.zip" --days 7 --recursive --dry-run
```

Execução real (sem dry-run):
```bat
MyRotines.exe cleanup-files --path "c:\demandas\entrada" --pattern "*.zip" --days 7 --recursive
```

### 4.4 `watch`
Monitoramento contínuo com processamento automático.

```bat
MyRotines.exe watch --path "c:\demandas\entrada" --filter "*.zip" --recursive --extract --move-to "c:\demandas\processados"
```

### 4.5 `download`
Download de arquivo com opção de extrair e mover.

```bat
MyRotines.exe download --url "https://host/arquivo.zip" --output "c:\temp\arquivo.zip" --extract --move-to "c:\demandas\entrada" --cleanup-downloaded
```

### 4.6 `extract`
Extração direta de ZIP.

```bat
MyRotines.exe extract --file "c:\temp\arquivo.zip" --dest "c:\temp\arquivo"
```

### 4.7 `move`
Move arquivo/pasta para diretório destino.

```bat
MyRotines.exe move --source "c:\temp\arquivo" --dest "c:\demandas\entrada"
```

### 4.8 `restore-sql`
Restore de `.bak` no SQL Server.

Autenticação integrada:
```bat
MyRotines.exe restore-sql --backup "c:\demandas\entrada\MinhaBase.bak" --sql-server ".\SQLEXPRESS" --sql-db "MinhaBase" --sql-trusted --sql-replace
```

Autenticação SQL:
```bat
MyRotines.exe restore-sql --backup "c:\demandas\entrada\MinhaBase.bak" --sql-server "10.0.0.10,1433" --sql-db "MinhaBase" --sql-user "sa" --sql-pass "senha" --sql-replace
```

### 4.9 `routine`
Pipeline customizável por profile, shortcut ou steps explícitos.

Shortcut:
```bat
MyRotines.exe routine --url "https://host/arquivo.zip" --move-extract --move-to "c:\demandas\entrada"
```

Steps explícitos:
```bat
MyRotines.exe routine --url "https://host/arquivo.zip" --steps "download,extract,move,cleanup" --move-to "c:\demandas\entrada" --cleanup-downloaded
```

Com restore SQL no pipeline:
```bat
MyRotines.exe routine --url "https://host/arquivo.zip" --steps "download,extract,restore-sql" --sql-server ".\SQLEXPRESS" --sql-db "MinhaBase" --sql-trusted --sql-replace
```

## 5. Profiles e configuração

Arquivo: `MyRotines\appsettings.json`

Exemplo:
```json
{
  "RoutineProfiles": {
    "Profiles": {
      "default": [ "download", "extract" ],
      "move-extract": [ "download", "extract", "move" ],
      "full": [ "download", "extract", "move", "cleanup" ],
      "restore": [ "download", "extract", "restore-sql" ]
    }
  }
}
```

## 6. Extensão via OCP

Para adicionar step novo:
1. Criar classe implementando `IRoutineStep`
2. Registrar no `Program.cs` (`services.AddScoped<IRoutineStep, SeuStep>();`)
3. Usar no `--steps` ou em profile no `appsettings.json`

## 7. Troubleshooting

- `"dl" não é reconhecido...`: URL com `&` sem aspas no `cmd`
- `End of Central Directory record...`: arquivo baixado não era ZIP válido
- `Arquivo .bak não encontrado`: caminho inválido no restore SQL
- `sqlcmd` não encontrado: instalar SQL Server Command Line Utilities e adicionar no PATH
