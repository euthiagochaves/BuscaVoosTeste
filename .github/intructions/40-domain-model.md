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
