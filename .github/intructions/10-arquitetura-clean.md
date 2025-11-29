# 10 - Arquitetura Clean e Estrutura da Solution

## Estrutura da Solution

A solution **BuscaVoosTeste.sln** deverá conter os seguintes projetos:

```text
/BuscaVoosTeste.sln
 └── src/
      ├── BuscaVoosTeste.Domain/
      ├── BuscaVoosTeste.Application/
      ├── BuscaVoosTeste.Infrastructure/
      ├── BuscaVoosTeste.Api/
      └── BuscaVoosTeste.McpServer/
```

> Não haverá projetos de testes nesta POC.

## Responsabilidades de cada projeto

### 1. BuscaVoosTeste.Domain

- Contém **apenas regras de negócio puras** e modelos de domínio;
- Não referencia nenhum outro projeto;
- Responsável por:
  - Entidades de domínio:
    - `FlightOffer`;
    - `FlightSegment`;
    - (Opcional) `Airport`, `Airline`, etc.
  - Value Objects (VOs):
    - `Money`;
    - `Duration` (encapsulando `TimeSpan`).

- Não conhece Duffel nem HTTP, apenas o conceito de **oferta de voo**.

### 2. BuscaVoosTeste.Application

- Implementa os **casos de uso de aplicação**;
- Depende **apenas** de `BuscaVoosTeste.Domain`;
- Responsabilidades principais:
  - Caso de uso **principal**:
    - `BuscarVoosUseCase`;
  - DTOs de entrada/saída:
    - `BuscarVoosInput` (entrada);
  - Interfaces (ports) para comunicação com a infraestrutura:
    - `IFlightSearchProvider`;
  - Regras de negócio de aplicação:
    - validação básica dos parâmetros;
    - orquestração de chamadas ao provider da Duffel;
    - tratamento de cenários de “nenhum voo encontrado”.

- Não conhece detalhes do JSON da Duffel; só fala com uma interface (`IFlightSearchProvider`).

### 3. BuscaVoosTeste.Infrastructure

- Depende de:
  - `BuscaVoosTeste.Domain`;
  - `BuscaVoosTeste.Application`;
- Implementa a integração **exclusiva com a Duffel API**:
  - `DuffelFlightSearchProvider : IFlightSearchProvider`;
  - `DuffelHttpClient` para lidar com chamadas HTTP;
  - DTOs específicos da Duffel:
    - `DuffelOfferRequestDto`;
    - `DuffelOfferResponseDto`;
  - Mapeamento Duffel → Domínio:
    - `DuffelMappingExtensions` ou equivalente.

- Lida com:
  - `HttpClient` / `HttpClientFactory`;
  - Configurações `DuffelOptions`.

### 4. BuscaVoosTeste.Api

- Projeto ASP.NET Core que expõe a API HTTP pública da POC;
- Depende de:
  - `BuscaVoosTeste.Application`;
  - `BuscaVoosTeste.Infrastructure`.

Responsabilidades:

- Expor o endpoint principal de busca de voos:
  - `GET /api/voos/busca`;
- Receber parâmetros de query string ou body;
- Montar o DTO `BuscarVoosInput`;
- Invocar `IBuscarVoosUseCase`;
- Mapear as entidades de domínio (`FlightOffer`) para um response JSON amigável;
- Configurar:
  - DI (injeção de dependência);
  - Swagger/OpenAPI;
  - Configurações de Duffel via `IOptions<DuffelOptions>`;
  - `HttpClientFactory` para Duffel.

### 5. BuscaVoosTeste.McpServer

- Projeto voltado para integração com IA via **MCP (Model Context Protocol)**;
- Depende de:
  - `BuscaVoosTeste.Application`;
  - (Opcional) `BuscaVoosTeste.Infrastructure`, se for necessário registrar DI e provider.

Responsabilidades:

- Expor uma **Tool MCP** principal:
  - Nome sugerido: `buscar_voos`;
- Receber um input em JSON com parâmetros de busca;
- Converter para `BuscarVoosInput`;
- Chamar `IBuscarVoosUseCase`;
- Retornar lista de `FlightOffer` em JSON.

## Regras de dependência (Clean Architecture)

- `BuscaVoosTeste.Domain` não referencia nenhum outro projeto;
- `BuscaVoosTeste.Application` referencia apenas `BuscaVoosTeste.Domain`;
- `BuscaVoosTeste.Infrastructure` referencia `BuscaVoosTeste.Domain` e `BuscaVoosTeste.Application`;
- `BuscaVoosTeste.Api` referencia `BuscaVoosTeste.Application` e `BuscaVoosTeste.Infrastructure`;
- `BuscaVoosTeste.McpServer` referencia `BuscaVoosTeste.Application` e, se necessário, `BuscaVoosTeste.Infrastructure`.
