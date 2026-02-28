# ⚙️ EventFlow Automation

Plataforma de automação baseada em **arquitetura orientada a eventos**, projetada para criar fluxos automatizados de forma **desacoplada, extensível e escalável**.

> Cada módulo faz sua parte.  
> Os eventos fazem o sistema trabalhar em conjunto.

## 🚀 Visão Geral

O **EventFlow Automation** permite construir rotinas automatizadas através da composição de eventos, sem dependências diretas entre serviços.

Em vez de um serviço chamar outro diretamente:

- Uma ação é executada
- Um evento é publicado
- Um ou mais manipuladores reagem ao evento

Isso permite criar fluxos complexos de forma modular e evolutiva.

## 🧠 Problema

Em sistemas tradicionais de automação:

- Serviços ficam acoplados
- Alterações quebram fluxos existentes
- Escalar funcionalidades se torna difícil
- A manutenção cresce em complexidade

Exemplo de acoplamento:

DownloadService → ExtracaoService → ProcessamentoService

## ✅ Solução

Arquitetura orientada a eventos:

DownloadService
↓
ExtrairEvent
↓
ExtracaoHandler

Nenhum serviço conhece o outro.  
A comunicação acontece apenas através de eventos.

## 🏗️ Arquitetura

```mermaid
flowchart LR
A[Comando / CLI] --> B[Serviço]
B --> C[Event Publisher]
C --> D[Dispatcher]
D --> E[Handler 1]
D --> F[Handler 2]
D --> G[Handler N]
Características

Arquitetura orientada a eventos
Baixo acoplamento
Extensível por novos handlers
Processamento assíncrono
Preparado para evolução distribuída

🔧 Tecnologias

.NET 9
C#
Dependency Injection
FileSystemWatcher
Execução de comandos CLI
Arquitetura modular

⚡ Exemplo de Fluxo

Fluxo implementado atualmente:

Executar comando de download
Publicar ExtrairArquivoEvent
Handler de extração reage automaticamente
Arquivo é extraído sem chamada direta entre serviços

Fluxo resultante:

Baixar → Evento → Extrair

🎯 Objetivo do Projeto

Evoluir para uma plataforma onde:

Novas automações possam ser adicionadas facilmente
Fluxos sejam compostos por configuração
Execuções ocorram de forma totalmente automática
Não seja necessário alterar o núcleo do sistema

🗺️ Roadmap

 Fila interna com Channel
 Retry automático
 Dead Letter
 Logs estruturados (Serilog)
 Integração com RabbitMQ
 Execução em Worker Service
 Configuração de fluxos via arquivo
 Interface para gerenciamento de rotinas
 Integração com IA para definição de fluxos

💡 Casos de Uso

Automação de downloads e processamento
Integração via diretórios monitorados
Pipelines de arquivos
Orquestração de tarefas locais
Automação de rotinas operacionais

🧩 Filosofia

Cada serviço tem uma única responsabilidade
Nenhum serviço depende diretamente de outro
Eventos são o contrato de integração
Automação cresce por composição, não por acoplamento

👨‍💻 Autor

Maurício Chaves
Desenvolvedor .NET

📜 Licença
Uso livre para fins de estudo e desenvolvimento.
