# Validação Real de Execução

Data: 2026-03-08

## Escopo validado

- Build da solução
- Inventário de arquivos
- Limpeza por retenção (dry-run e execução)
- Movimentação de arquivo
- Monitoramento (`watch`) com extração e movimentação automáticas
- Restore SQL (fluxo de erro esperado para `.bak` ausente)

## Execuções realizadas

1. Build
```bat
dotnet build .\MyRotines.sln -v minimal
```
Resultado: sucesso (0 erros / 0 warnings)

2. Inventory
```bat
MyRotines.exe inventory --path .\tmp-test --recursive
```
Resultado: sucesso (tabela por extensão exibida)

3. Cleanup dry-run
```bat
MyRotines.exe cleanup-files --path .\tmp-test\cleanup --pattern "*.zip" --days 7 --dry-run
```
Resultado: sucesso (`Matched=1`, `Deleted=0`)

4. Move
```bat
MyRotines.exe move --source .\tmp-test\inbox\sample.zip --dest .\tmp-test\processed
```
Resultado: sucesso (arquivo movido)

5. Watch (teste real em background)
- Watch iniciado monitorando `.\tmp-test\inbox`
- Arquivo `watch-input.zip` injetado
- Resultado observado no log: detectou, extraiu e moveu para `.\tmp-test\processed\watch-input`

6. Cleanup execução
```bat
MyRotines.exe cleanup-files --path "C:\dev\mchaves-dev\MyRotines\tmp-test\cleanup" --pattern "*.zip" --days 7
```
Resultado: sucesso (`Deleted=1`)

7. Restore SQL (erro esperado)
```bat
MyRotines.exe restore-sql --backup .\tmp-test\missing.bak --sql-server .\SQLEXPRESS --sql-db TestDb --sql-trusted
```
Resultado: sucesso na validação de erro amigável (painel indicando `.bak` não encontrado)

## Correções feitas durante validação

- Corrigido bug no `cleanup-files` (cálculo de tamanho após deleção causava exceção)
