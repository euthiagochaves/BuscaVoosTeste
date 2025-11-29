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
