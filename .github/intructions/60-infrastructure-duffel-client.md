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
