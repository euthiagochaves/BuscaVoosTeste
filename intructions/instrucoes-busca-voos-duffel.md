
# 00 - Visão Geral e Objetivos

## Nome da solução

A solution deverá se chamar **BuscaVoosTeste.sln**.

## Objetivo do projeto

Criar uma **POC (Proof of Concept)** em **.NET 10 + C# 14**, com **Clean Architecture**, para:

- Realizar **busca de voos** usando **exclusivamente a API da Duffel**;
- Expor uma **API HTTP** para busca de voos (estilo Decolar.com, apenas listagem);
- Estar preparada para integração futura com **IA / GitHub Copilot / MCP Server**, permitindo que a IA faça buscas de voos em linguagem natural.

## Escopo desta POC

### O que o projeto DEVE fazer

- Receber parâmetros básicos de busca:
  - Origem (código IATA);
  - Destino (código IATA);
  - Data de ida;
  - Data de volta (opcional);
  - Quantidade de passageiros;
  - Classe de cabine (opcional).
- Invocar **somente a Duffel API** para buscar ofertas de voo;
- Converter o response da Duffel em um **modelo de domínio próprio** (`FlightOffer`, etc.);
- Retornar os resultados via endpoint HTTP claro, tipado, documentado via Swagger;
- Implementar **Clean Architecture** com as camadas/projetos:
  - `BuscaVoosTeste.Domain`;
  - `BuscaVoosTeste.Application`;
  - `BuscaVoosTeste.Infrastructure`;
  - `BuscaVoosTeste.Api`;
  - `BuscaVoosTeste.McpServer` (IA / MCP).

### O que o projeto NÃO fará (por enquanto)

- Não haverá:
  - Compra de voo;
  - Reserva/booking;
  - Emissão de bilhete;
  - Pagamento;
  - Login ou autenticação de usuário final;
  - Persistência em banco de dados;
  - Integração com outras APIs de voos além da Duffel;
  - Projetos/camadas de testes (unitários ou integração) nesta POC.

## Objetivo para IA (futuro)

- Permitir que um **MCP Server** (projeto `BuscaVoosTeste.McpServer`) utilize esta POC para:
  - Interpretar intenções em linguagem natural (ex.: “quero voos de GRU para EZE amanhã de manhã”);
  - Converter essas intenções em chamadas estruturadas ao caso de uso `BuscarVoosUseCase`;
  - Retornar resultados em JSON pronto para ser consumido pela IA, front-end ou qualquer outro consumidor.

---

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

---

# 20 - Regras de Negócio da Busca de Voos

## Parâmetros de entrada obrigatórios

1. **Origem**  
   - Código IATA de 3 letras (ex.: `GRU`, `EZE`, `GIG`);
   - Obrigatório.

2. **Destino**  
   - Código IATA de 3 letras;
   - Obrigatório.

3. **Data de ida**  
   - Data no formato ISO (`yyyy-MM-dd`) ou `DateTime`;
   - Obrigatório.

4. **Quantidade de passageiros**  
   - Inteiro >= 1;
   - Se não informado, valor padrão = 1.

## Parâmetros de entrada opcionais

5. **Data de volta**  
   - Opcional;
   - Se informada, considerar busca de ida e volta (round-trip).

6. **Classe de cabine**  
   - Opcional;
   - Valores possíveis (internamente normalizados):
     - `Economy`;
     - `PremiumEconomy`;
     - `Business`;
     - `First`.

7. **Filtros adicionais (futuro, não obrigatório na POC)**  
   - Número máximo de escalas;
   - Companhia aérea preferida, etc.
   - A POC deve estar preparada no modelo de domínio para suportar futuras extensões.

## Regras de negócio de validação

- Origem e destino não podem ser nulos ou vazios;
- Origem e destino não podem ser iguais;
- Quantidade de passageiros deve ser >= 1;
- Em ambiente real, a data de ida deve ser maior ou igual à data atual (na POC pode ser flexibilizado, mas a regra deve estar documentada aqui).

## Regras relacionadas à Duffel

- O caso de uso **não** conhece a Duffel diretamente;
- Apenas a implementação de `IFlightSearchProvider` em Infraestrutura sabe:
  - como montar o request da Duffel;
  - como interpretar o response da Duffel.

## Regras de tradução para o domínio

- Toda resposta da Duffel deve ser convertida para o modelo de domínio `FlightOffer`, contendo pelo menos:
  - `Id` (GUID interno da POC);
  - `OrigemIata`;
  - `DestinoIata`;
  - `Partida`;
  - `Chegada`;
  - `DuracaoTotal` (`Duration`);
  - `PrecoTotal` (`Money`);
  - `CompanhiaAereaCodigo`;
  - `CompanhiaAereaNome` (quando disponível);
  - `Cabine`;
  - `Segmentos` (lista de `FlightSegment`).

- Caso não haja ofertas, o provider deve retornar uma lista vazia, **não** nula.

## Regras de erro

- Erros de autenticação/credencial inválida na Duffel (HTTP 401/403) devem ser logados e mapeados para um erro genérico de infraestrutura para a API;
- Erros de validação de parâmetros (400 Duffel) podem ser tratados como erro 400 na API, quando fizer sentido;
- Erros de rede (timeout, DNS, etc.) devem ser tratados como erro 502/503/500 na API, conforme política que você escolher.

---

# 30 - Dependências e Configurações (Duffel)

## Stack Tecnológica

- .NET 10;
- C# 14;
- ASP.NET Core (para a API);
- `HttpClientFactory` para consumo da Duffel;
- `IOptions<T>` para configurações da Duffel;
- Swagger / Swashbuckle para documentação da API;
- (Opcional) FluentValidation para validação de DTOs de entrada na Application ou na Api.

## Duffel API

- A **única API de voos utilizada neste projeto será a Duffel**;
- Base URL (sandbox ou produção) deve ser configurável;
- Autenticação via header HTTP:

```http
Authorization: Bearer {DUFFEL_ACCESS_TOKEN}
```

### Endpoints principais (visão conceitual)

- Criação de request de ofertas de voo:
  - `POST /air/offer_requests`

- Dependendo da documentação atual da Duffel, é possível que:
  - O próprio `offer_requests` retorne as ofertas; ou
  - Seja necessário buscar as ofertas depois em outro endpoint.

> A implementação concreta deve seguir a documentação oficial da Duffel, mas a POC foca na modelagem e integração geral.

## Configuração em appsettings.json

Exemplo:

```json
{
  "Duffel": {
    "BaseUrl": "https://api.duffel.com",
    "AccessToken": "SEU_TOKEN_DUFFEL_AQUI"
  }
}
```

### Regras de uso

- Nunca hardcodar o token diretamente no código;
- Usar `appsettings.Development.json` e/ou `User Secrets` para ambiente local;
- Em produção, usar variáveis de ambiente, Key Vault, etc.

## Registro de serviços no DI (exemplo conceitual)

Em `BuscaVoosTeste.Api` (ou em uma extensão em Infraestrutura):

```csharp
services.Configure<DuffelOptions>(configuration.GetSection("Duffel"));

services.AddHttpClient<IDuffelHttpClient, DuffelHttpClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DuffelOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", options.AccessToken);
});

services.AddScoped<IFlightSearchProvider, DuffelFlightSearchProvider>();
services.AddScoped<IBuscarVoosUseCase, BuscarVoosUseCase>();
```

---

# 40 - Domain Model (BuscaVoosTeste.Domain)

## Objetivo

Definir um modelo de domínio simples, expressivo e desacoplado da Duffel, para representar ofertas de voo.

## Entidade: FlightOffer

Sugestão de propriedades:

```csharp
public sealed class FlightOffer
{
    public Guid Id { get; }
    public string OrigemIata { get; }
    public string DestinoIata { get; }
    public DateTime Partida { get; }
    public DateTime Chegada { get; }
    public Duration DuracaoTotal { get; }
    public Money PrecoTotal { get; }
    public string CompanhiaAereaCodigo { get; }
    public string CompanhiaAereaNome { get; }
    public string Cabine { get; }
    public IReadOnlyCollection<FlightSegment> Segmentos { get; }

    // Construtor deve garantir consistência (ex.: Partida < Chegada)
}
```

## Value Object: Money

```csharp
public readonly struct Money
{
    public decimal Valor { get; }
    public string Moeda { get; }

    public Money(decimal valor, string moeda)
    {
        // Validar valor >= 0, moeda não nula/vazia
        Valor = valor;
        Moeda = moeda;
    }
}
```

## Value Object: Duration

```csharp
public readonly struct Duration
{
    public TimeSpan Value { get; }

    public Duration(TimeSpan value)
    {
        Value = value;
    }

    public override string ToString()
        => Value.ToString();
}
```

## Entidade/VO: FlightSegment

```csharp
public sealed class FlightSegment
{
    public string OrigemIata { get; }
    public string DestinoIata { get; }
    public DateTime Partida { get; }
    public DateTime Chegada { get; }
    public string NumeroVoo { get; }
    public string CompanhiaAereaCodigo { get; }
    public Duration Duracao { get; }

    // Construtor para garantir consistência
}
```

## (Opcional) Airport e Airline

Caso necessário, podem ser definidos como VOs ou entidades simples, mas não são obrigatórios para a primeira versão da POC.

---

# 50 - Application - Use Case de Busca de Voos

## DTO de Entrada: BuscarVoosInput

```csharp
public sealed class BuscarVoosInput
{
    public string OrigemIata { get; init; } = default!;
    public string DestinoIata { get; init; } = default!;
    public DateTime DataIda { get; init; }
    public DateTime? DataVolta { get; init; }
    public int Passageiros { get; init; } = 1;
    public string? Cabine { get; init; } // Economy, Business, etc.
}
```

## Interface do Use Case

```csharp
public interface IBuscarVoosUseCase
{
    Task<IReadOnlyCollection<FlightOffer>> ExecuteAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default);
}
```

## Implementação conceitual do Use Case

```csharp
public sealed class BuscarVoosUseCase : IBuscarVoosUseCase
{
    private readonly IFlightSearchProvider _provider;

    public BuscarVoosUseCase(IFlightSearchProvider provider)
    {
        _provider = provider;
    }

    public async Task<IReadOnlyCollection<FlightOffer>> ExecuteAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar entrada
        //    - origem/destino não vazios
        //    - origem != destino
        //    - passageiros >= 1
        //    - (regra de data, se desejado)

        // 2. Delegar ao provider (Duffel)
        var offers = await _provider.SearchAsync(input, cancellationToken);

        // 3. Regras adicionais (filtros pós Duffel, se necessário)

        return offers;
    }
}
```

## Porta de saída: IFlightSearchProvider

```csharp
public interface IFlightSearchProvider
{
    Task<IReadOnlyCollection<FlightOffer>> SearchAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default);
}
```

- Em Infrastructure, será implementada por `DuffelFlightSearchProvider`.

---

# 60 - Infrastructure - Duffel Client e Provider

## Configuração: DuffelOptions

```csharp
public sealed class DuffelOptions
{
    public string BaseUrl { get; init; } = default!;
    public string AccessToken { get; init; } = default!;
}
```

## HttpClient para Duffel

Interface para encapsular chamadas HTTP:

```csharp
public interface IDuffelHttpClient
{
    Task<DuffelOfferResponseDto> CreateOfferRequestAsync(
        DuffelOfferRequestDto request,
        CancellationToken cancellationToken);
}
```

Implementação (conceitual):

```csharp
public sealed class DuffelHttpClient : IDuffelHttpClient
{
    private readonly HttpClient _httpClient;

    public DuffelHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DuffelOfferResponseDto> CreateOfferRequestAsync(
        DuffelOfferRequestDto request,
        CancellationToken cancellationToken)
    {
        // Serializar request em JSON
        // POST /air/offer_requests
        // Tratar status code
        // Deserializar response em DuffelOfferResponseDto
        throw new NotImplementedException(); // Implementar de fato no código
    }
}
```

## Provider: DuffelFlightSearchProvider

```csharp
public sealed class DuffelFlightSearchProvider : IFlightSearchProvider
{
    private readonly IDuffelHttpClient _client;

    public DuffelFlightSearchProvider(IDuffelHttpClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyCollection<FlightOffer>> SearchAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default)
    {
        // 1. Mapear BuscarVoosInput -> DuffelOfferRequestDto
        var request = DuffelRequestMapper.Map(input);

        // 2. Chamar a Duffel via HttpClient encapsulado
        var response = await _client.CreateOfferRequestAsync(request, cancellationToken);

        // 3. Mapear DuffelOfferResponseDto -> List<FlightOffer>
        var offers = DuffelResponseMapper.MapToDomain(response);

        // 4. Retornar lista (nunca null)
        return offers;
    }
}
```

## DTOs específicos da Duffel (conceituais)

```csharp
public sealed class DuffelOfferRequestDto
{
    // Propriedades conforme payload de oferta da Duffel
}

public sealed class DuffelOfferResponseDto
{
    // Propriedades conforme resposta de oferta da Duffel
}
```

## Mapeadores

```csharp
public static class DuffelRequestMapper
{
    public static DuffelOfferRequestDto Map(BuscarVoosInput input)
    {
        // Converter parâmetros do use case para o formato esperado pela Duffel
        throw new NotImplementedException();
    }
}

public static class DuffelResponseMapper
{
    public static IReadOnlyCollection<FlightOffer> MapToDomain(DuffelOfferResponseDto response)
    {
        // Converter resposta da Duffel para entidades de domínio FlightOffer
        throw new NotImplementedException();
    }
}
```

## Tratamento de erros

- Tratar códigos HTTP de erro (400, 401, 403, 500, etc.);
- Lançar exceções específicas de infra ou retornar resultados vazios quando fizer sentido;
- Registrar logs (via ILogger) na implementação real.

---

# 70 - API HTTP (BuscaVoosTeste.Api)

## Endpoint principal de busca de voos

### Rota

```http
GET /api/voos/busca
```

### Parâmetros de query

- `origem` (obrigatório; IATA);
- `destino` (obrigatório; IATA);
- `dataIda` (obrigatório; `yyyy-MM-dd`);
- `dataVolta` (opcional; `yyyy-MM-dd`);
- `passageiros` (opcional; inteiro >= 1; padrão 1);
- `cabine` (opcional; `Economy`, `Business`, etc.).

### Exemplo de request

```http
GET /api/voos/busca?origem=GRU&destino=EZE&dataIda=2025-01-10&passageiros=1&cabine=Economy
```

### Exemplo de response (200 OK)

```json
[
  {
    "id": "4c76d3c2-7853-4b6f-9b8f-9ad4d3efb3ab",
    "origemIata": "GRU",
    "destinoIata": "EZE",
    "partida": "2025-01-10T08:55:00Z",
    "chegada": "2025-01-10T12:05:00Z",
    "duracaoTotal": "03:10:00",
    "precoTotal": {
      "valor": 1320.50,
      "moeda": "USD"
    },
    "companhiaAereaCodigo": "LA",
    "companhiaAereaNome": "LATAM Airlines",
    "cabine": "Economy",
    "segmentos": [
      {
        "origemIata": "GRU",
        "destinoIata": "EZE",
        "partida": "2025-01-10T08:55:00Z",
        "chegada": "2025-01-10T12:05:00Z",
        "numeroVoo": "LA8000",
        "companhiaAereaCodigo": "LA",
        "duracao": "03:10:00"
      }
    ]
  }
]
```

### Status Codes

- `200 OK` – busca realizada com sucesso (lista pode estar vazia);
- `400 Bad Request` – parâmetros inválidos (ex.: origem/destino ausentes ou inválidos);
- `500 Internal Server Error` – erro inesperado na comunicação com Duffel ou na lógica interna.

### Implementação (conceito minimal API)

```csharp
app.MapGet("/api/voos/busca", async (
    [FromServices] IBuscarVoosUseCase useCase,
    [FromQuery] string origem,
    [FromQuery] string destino,
    [FromQuery] DateTime dataIda,
    [FromQuery] DateTime? dataVolta,
    [FromQuery] int passageiros,
    [FromQuery] string? cabine) =>
{
    var input = new BuscarVoosInput
    {
        OrigemIata = origem,
        DestinoIata = destino,
        DataIda = dataIda,
        DataVolta = dataVolta,
        Passageiros = passageiros <= 0 ? 1 : passageiros,
        Cabine = cabine
    };

    var offers = await useCase.ExecuteAsync(input);
    return Results.Ok(offers);
});
```

### Swagger

- Configurar Swagger para exibir:
  - Descrição da rota `/api/voos/busca`;
  - Parâmetros e exemplos;
  - Modelo de resposta.

---

# 80 - Integração MCP / IA (BuscaVoosTeste.McpServer)

## Objetivo

Expor um endpoint MCP (Tool) para que IA (como GitHub Copilot) possa buscar voos usando a lógica da POC.

## Projeto BuscaVoosTeste.McpServer

- Deve referenciar:
  - `BuscaVoosTeste.Application`;
  - e, se necessário, `BuscaVoosTeste.Infrastructure`.

## Tool principal: buscar_voos

### Nome

```text
buscar_voos
```

### Input JSON esperado

```json
{
  "origem": "GRU",
  "destino": "EZE",
  "data_ida": "2025-01-10",
  "data_volta": null,
  "passageiros": 1,
  "cabine": "Economy"
}
```

### Output

- Lista de `FlightOffer` em JSON, conforme retornado pelo use case.

### Esqueleto conceitual

```csharp
public sealed class BuscarVoosTool : ITool
{
    private readonly IBuscarVoosUseCase _useCase;

    public string Name => "buscar_voos";

    public async Task<object> ExecuteAsync(
        dynamic input,
        CancellationToken cancellationToken = default)
    {
        var dto = new BuscarVoosInput
        {
            OrigemIata = (string)input.origem,
            DestinoIata = (string)input.destino,
            DataIda = DateTime.Parse((string)input.data_ida),
            DataVolta = input.data_volta != null
                ? DateTime.Parse((string)input.data_volta)
                : null,
            Passageiros = input.passageiros != null
                ? (int)input.passageiros
                : 1,
            Cabine = input.cabine
        };

        var result = await _useCase.ExecuteAsync(dto, cancellationToken);
        return result; // MCP host faz a serialização em JSON
    }
}
```

## Exemplos de prompts para IA

- “Buscar voos de GRU para EZE no dia 10 de janeiro, 1 passageiro em classe econômica.”
- “Quero opções de voos de São Paulo para Buenos Aires na próxima sexta, ida e volta, 2 passageiros.”

A IA deve:

1. Interpretar o comando em linguagem natural;
2. Montar o JSON de input esperado por `buscar_voos`;
3. Invocar a Tool `buscar_voos` via MCP;
4. Exibir o JSON retornado para o usuário ou usar em outra ação.

---

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
