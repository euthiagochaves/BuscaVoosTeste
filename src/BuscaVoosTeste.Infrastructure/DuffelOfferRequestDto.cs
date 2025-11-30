namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// DTO de requisição de ofertas para a API da Duffel.
/// Representa o payload enviado ao endpoint POST /air/offer_requests da Duffel
/// para buscar ofertas de voos com base nos critérios de busca.
/// </summary>
/// <remarks>
/// Este DTO é específico da camada de infraestrutura e serve como modelo de transporte
/// entre a aplicação e o cliente HTTP da Duffel. O mapeamento dos dados de negócio
/// (BuscarVoosInput) para este DTO será feito por um mapper de infraestrutura.
/// </remarks>
public sealed class DuffelOfferRequestDto
{
    /// <summary>
    /// Lista de trechos (slices) da viagem.
    /// Cada trecho representa uma parte do itinerário (ida ou volta).
    /// </summary>
    /// <remarks>
    /// Para viagens somente ida, a lista contém um único trecho.
    /// Para viagens de ida e volta, a lista contém dois trechos.
    /// </remarks>
    public IReadOnlyCollection<TrechoDto> Trechos { get; init; } = [];

    /// <summary>
    /// Lista de passageiros para a busca de ofertas.
    /// Cada passageiro deve ter seu tipo especificado.
    /// </summary>
    public IReadOnlyCollection<PassageiroDto> Passageiros { get; init; } = [];

    /// <summary>
    /// Classe de cabine desejada para a busca de ofertas.
    /// Valores possíveis: economy, premium_economy, business, first.
    /// Parâmetro opcional.
    /// </summary>
    public string? ClasseCabine { get; init; }

    /// <summary>
    /// DTO que representa um trecho (slice) da viagem.
    /// </summary>
    public sealed class TrechoDto
    {
        /// <summary>
        /// Código IATA do aeroporto de origem (3 letras).
        /// </summary>
        /// <example>GRU, CNF, GIG</example>
        public string OrigemIata { get; init; } = default!;

        /// <summary>
        /// Código IATA do aeroporto de destino (3 letras).
        /// </summary>
        /// <example>GRU, CNF, GIG</example>
        public string DestinoIata { get; init; } = default!;

        /// <summary>
        /// Data de partida do trecho no formato ISO 8601 (yyyy-MM-dd).
        /// </summary>
        public string DataPartida { get; init; } = default!;
    }

    /// <summary>
    /// DTO que representa um passageiro na requisição de ofertas.
    /// </summary>
    public sealed class PassageiroDto
    {
        /// <summary>
        /// Tipo do passageiro conforme esperado pela API da Duffel.
        /// Valores possíveis: adult, child, infant_without_seat.
        /// </summary>
        public string Tipo { get; init; } = default!;
    }
}
