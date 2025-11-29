# 90 - Instruções para o GitHub Copilot / Agent

## Objetivo do Agent

Você (Copilot/Agent) será utilizado para:

- Criar e manter o código da solution **BuscaVoosTeste.sln**;
- Respeitar integralmente a arquitetura e as regras definidas neste arquivo;
- Implementar a integração **exclusiva com a API da Duffel** para busca de voos;
- Criar a API HTTP (`BuscaVoosTeste.Api`) e o servidor MCP (`BuscaVoosTeste.McpServer`).

## Regras gerais

1. **Não alterar a arquitetura proposta**  
   - Manter os projetos exatamente como descritos:
     - `BuscaVoosTeste.Domain`;
     - `BuscaVoosTeste.Application`;
     - `BuscaVoosTeste.Infrastructure`;
     - `BuscaVoosTeste.Api`;
     - `BuscaVoosTeste.McpServer`.

2. **Gerar código completo**  
   - Sempre produzir classes completas, com todos os `using` necessários;
   - Não usar “...”, `// código aqui` ou qualquer forma de omissão.

3. **Manter separação de camadas**  
   - Domain: só regras de negócio e modelos;
   - Application: casos de uso e interfaces (ports);
   - Infrastructure: detalhes da Duffel, HTTP, mapeamentos;
   - Api: endpoints HTTP, modelos de request/response;
   - McpServer: exposição dos casos de uso como Tools MCP.

4. **Naming conventions**  
   - Classes, interfaces, métodos e propriedades em **PascalCase**;
   - Campos privados em camelCase com `_` prefixado (ex.: `_client`, `_provider`).

5. **Foco total na Duffel**  
   - Não gerar código para outros provedores de voos;
   - Não criar interfaces genéricas adicionais sem necessidade;
   - Sempre que falar em provider externo de voos, considerar que é **Duffel**.

6. **Sem camada de testes nesta POC**  
   - Não criar projetos `*.Tests`;
   - Não configurar frameworks de teste;
   - Esta POC é exclusivamente para demonstração de arquitetura + integração com Duffel + IA.

## Como o Agent deve agir

Quando um comando for dado, por exemplo:

- “Criar a classe `FlightOffer` em `BuscaVoosTeste.Domain` conforme definido no 40 - Domain Model.”  
- “Implementar `IFlightSearchProvider` e `DuffelFlightSearchProvider` em `BuscaVoosTeste.Infrastructure`.”  
- “Criar o endpoint `GET /api/voos/busca` na API conforme seção 70.”  

O Agent deve:

1. Utilizar estas instruções como **fonte da verdade**;
2. Criar ou atualizar os arquivos `.cs` correspondentes;
3. Garantir a coerência entre as camadas e a DI;
4. Manter o foco somente em **busca de voos via Duffel**.

## Exemplos de comandos úteis para você dar ao Copilot

- “Crie a solution `BuscaVoosTeste.sln` com os projetos descritos nas instruções.”  
- “Implemente a entidade `FlightOffer` em `BuscaVoosTeste.Domain` conforme o modelo do arquivo de instruções.”  
- “Implemente o use case `BuscarVoosUseCase` em `BuscaVoosTeste.Application` usando `IFlightSearchProvider`.”  
- “Implemente `DuffelFlightSearchProvider` e `DuffelHttpClient` em `BuscaVoosTeste.Infrastructure`.”  
- “Crie o endpoint `GET /api/voos/busca` em `BuscaVoosTeste.Api` que consome `IBuscarVoosUseCase`.”  
- “Crie o projeto `BuscaVoosTeste.McpServer` e a Tool `buscar_voos` conforme seção 80 das instruções.”
