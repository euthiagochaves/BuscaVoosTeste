namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// DTO de resposta de ofertas da API da Duffel.
/// Representa a estrutura de dados retornada pelo endpoint POST /air/offer_requests da Duffel
/// contendo as ofertas de voos disponíveis para os critérios de busca informados.
/// </summary>
/// <remarks>
/// Este DTO é específico da camada de infraestrutura e serve como modelo para desserialização
/// da resposta JSON da Duffel. Posteriormente, será mapeado para as entidades de domínio
/// (FlightOffer, FlightSegment, etc.) através de um mapper de infraestrutura.
/// </remarks>
public sealed class DuffelOfferResponseDto
{
    /// <summary>
    /// Lista de ofertas de voo retornadas pela Duffel.
    /// Cada oferta representa uma opção de voo disponível para os critérios de busca.
    /// </summary>
    public IReadOnlyCollection<OfertaDto> Ofertas { get; init; } = [];

    /// <summary>
    /// DTO que representa uma oferta individual de voo retornada pela Duffel.
    /// </summary>
    public sealed class OfertaDto
    {
        /// <summary>
        /// Identificador único da oferta retornado pela Duffel.
        /// Utilizado para referência em operações subsequentes (reserva, confirmação, etc.).
        /// </summary>
        public string Id { get; init; } = default!;

        /// <summary>
        /// Valor total da oferta de voo.
        /// Representa o preço total para todos os passageiros informados na busca.
        /// </summary>
        public decimal PrecoTotal { get; init; }

        /// <summary>
        /// Código da moeda do preço total (ex.: BRL, USD, EUR).
        /// Segue o padrão ISO 4217.
        /// </summary>
        public string Moeda { get; init; } = default!;

        /// <summary>
        /// Data e hora de expiração da oferta (UTC).
        /// Após este momento, a oferta não estará mais disponível para reserva.
        /// </summary>
        public DateTime? Expiracao { get; init; }

        /// <summary>
        /// Lista de trechos (slices) que compõem a oferta.
        /// Cada trecho representa uma parte do itinerário (ida ou volta).
        /// </summary>
        public IReadOnlyCollection<TrechoRespostaDto> Trechos { get; init; } = [];

        /// <summary>
        /// Lista de passageiros associados à oferta com suas respectivas tarifas.
        /// </summary>
        public IReadOnlyCollection<PassageiroRespostaDto> Passageiros { get; init; } = [];

        /// <summary>
        /// Código da companhia aérea proprietária da oferta (ex.: JJ, LA, G3).
        /// </summary>
        public string CompanhiaAereaCodigo { get; init; } = default!;

        /// <summary>
        /// Nome da companhia aérea proprietária da oferta (quando disponível).
        /// </summary>
        public string? CompanhiaAereaNome { get; init; }
    }

    /// <summary>
    /// DTO que representa um trecho (slice) na resposta da Duffel.
    /// Um trecho é uma parte do itinerário de viagem (ex.: ida ou volta).
    /// </summary>
    public sealed class TrechoRespostaDto
    {
        /// <summary>
        /// Identificador único do trecho retornado pela Duffel.
        /// </summary>
        public string Id { get; init; } = default!;

        /// <summary>
        /// Código IATA do aeroporto de origem do trecho (3 letras).
        /// </summary>
        public string OrigemIata { get; init; } = default!;

        /// <summary>
        /// Código IATA do aeroporto de destino do trecho (3 letras).
        /// </summary>
        public string DestinoIata { get; init; } = default!;

        /// <summary>
        /// Duração total do trecho no formato ISO 8601 (ex.: PT2H30M).
        /// Inclui tempo de voo e escalas/conexões.
        /// </summary>
        public string? Duracao { get; init; }

        /// <summary>
        /// Lista de segmentos que compõem o trecho.
        /// Cada segmento representa um voo direto entre dois aeroportos.
        /// </summary>
        public IReadOnlyCollection<SegmentoDto> Segmentos { get; init; } = [];
    }

    /// <summary>
    /// DTO que representa um segmento de voo na resposta da Duffel.
    /// Um segmento é um voo direto entre dois aeroportos, sem escalas intermediárias.
    /// </summary>
    public sealed class SegmentoDto
    {
        /// <summary>
        /// Identificador único do segmento retornado pela Duffel.
        /// </summary>
        public string Id { get; init; } = default!;

        /// <summary>
        /// Código IATA do aeroporto de origem do segmento (3 letras).
        /// </summary>
        public string OrigemIata { get; init; } = default!;

        /// <summary>
        /// Código IATA do aeroporto de destino do segmento (3 letras).
        /// </summary>
        public string DestinoIata { get; init; } = default!;

        /// <summary>
        /// Data e hora de partida do voo (UTC ou com timezone).
        /// </summary>
        public DateTime Partida { get; init; }

        /// <summary>
        /// Data e hora de chegada do voo (UTC ou com timezone).
        /// </summary>
        public DateTime Chegada { get; init; }

        /// <summary>
        /// Duração do segmento no formato ISO 8601 (ex.: PT1H45M).
        /// </summary>
        public string? Duracao { get; init; }

        /// <summary>
        /// Número do voo (ex.: JJ1234, LA3456).
        /// </summary>
        public string NumeroVoo { get; init; } = default!;

        /// <summary>
        /// Código da companhia aérea operadora do segmento (ex.: JJ, LA, G3).
        /// </summary>
        public string CompanhiaAereaCodigo { get; init; } = default!;

        /// <summary>
        /// Nome da companhia aérea operadora do segmento (quando disponível).
        /// </summary>
        public string? CompanhiaAereaNome { get; init; }

        /// <summary>
        /// Classe de cabine do segmento (ex.: economy, premium_economy, business, first).
        /// </summary>
        public string? ClasseCabine { get; init; }
    }

    /// <summary>
    /// DTO que representa um passageiro e sua tarifa na resposta da Duffel.
    /// </summary>
    public sealed class PassageiroRespostaDto
    {
        /// <summary>
        /// Identificador único do passageiro retornado pela Duffel.
        /// </summary>
        public string Id { get; init; } = default!;

        /// <summary>
        /// Tipo do passageiro conforme a API da Duffel.
        /// Valores possíveis: adult, child, infant_without_seat.
        /// </summary>
        public string Tipo { get; init; } = default!;

        /// <summary>
        /// Valor da tarifa individual do passageiro.
        /// </summary>
        public decimal? ValorTarifa { get; init; }

        /// <summary>
        /// Código da moeda da tarifa do passageiro.
        /// </summary>
        public string? MoedaTarifa { get; init; }
    }
}
