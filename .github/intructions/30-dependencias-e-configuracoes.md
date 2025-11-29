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
    "AccessToken": "DUFFEL_ACCESS_TOKEN"
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

### Biblioteca Dufferl oficial (se disponível)
- se necessário, consultar a biblioteca oficial da Duffel https://duffel.com/docs/api/overview/welcome
