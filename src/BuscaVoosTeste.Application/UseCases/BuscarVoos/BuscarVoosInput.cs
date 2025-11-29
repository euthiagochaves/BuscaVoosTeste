namespace BuscaVoosTeste.Application.UseCases.BuscarVoos;

/// <summary>
/// DTO de entrada para o caso de uso de busca de voos.
/// Representa os parâmetros necessários para realizar uma pesquisa de voos.
/// </summary>
/// <remarks>
/// Este DTO contém os seguintes parâmetros:
/// - Origem e destino (códigos IATA obrigatórios);
/// - Data de ida (obrigatória);
/// - Data de volta (opcional, para viagens de ida e volta);
/// - Quantidade de passageiros (padrão = 1);
/// - Classe de cabine (opcional: Economy, PremiumEconomy, Business, First).
/// </remarks>
public sealed class BuscarVoosInput
{
    /// <summary>
    /// Código IATA do aeroporto ou cidade de origem (3 letras).
    /// Parâmetro obrigatório.
    /// </summary>
    /// <example>GRU, BHZ, GIG</example>
    public string OrigemIata { get; init; } = default!;

    /// <summary>
    /// Código IATA do aeroporto ou cidade de destino (3 letras).
    /// Parâmetro obrigatório.
    /// </summary>
    /// <example>GRU, BHZ, GIG</example>
    public string DestinoIata { get; init; } = default!;

    /// <summary>
    /// Data de ida da viagem.
    /// Parâmetro obrigatório.
    /// </summary>
    public DateTime DataIda { get; init; }

    /// <summary>
    /// Data de volta da viagem.
    /// Parâmetro opcional. Se informada, considera busca de ida e volta (round-trip).
    /// </summary>
    public DateTime? DataVolta { get; init; }

    /// <summary>
    /// Quantidade de passageiros para a busca.
    /// Valor padrão é 1. Deve ser maior ou igual a 1.
    /// </summary>
    public int Passageiros { get; init; } = 1;

    /// <summary>
    /// Classe de cabine desejada para o voo.
    /// Parâmetro opcional.
    /// Valores possíveis: Economy, PremiumEconomy, Business, First.
    /// </summary>
    public string? Cabine { get; init; }
}
