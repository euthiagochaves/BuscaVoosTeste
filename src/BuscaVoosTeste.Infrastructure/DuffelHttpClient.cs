using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// Implementação concreta do cliente HTTP para comunicação com a API da Duffel.
/// Responsável por realizar as chamadas HTTP de baixo nível para operações de busca de ofertas de voo.
/// </summary>
/// <remarks>
/// Esta classe utiliza HttpClient injetado via DI, com configurações de BaseAddress e
/// Authorization header definidas no registro do serviço em Startup/Program.
/// </remarks>
public sealed class DuffelHttpClient : IDuffelHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _opcoesSerializacao;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DuffelHttpClient"/>.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP configurado com BaseAddress e headers de autenticação.</param>
    public DuffelHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _opcoesSerializacao = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public async Task<DuffelOfferResponseDto> CreateOfferRequestAsync(
        DuffelOfferRequestDto request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var corpoDaRequisicao = MontarCorpoDaRequisicao(request);

        var resposta = await _httpClient.PostAsJsonAsync(
            "air/offer_requests",
            corpoDaRequisicao,
            _opcoesSerializacao,
            cancellationToken);

        await ValidarRespostaHttpAsync(resposta, cancellationToken);

        var respostaJson = await resposta.Content.ReadFromJsonAsync<RespostaDuffelWrapper>(
            _opcoesSerializacao,
            cancellationToken);

        return MapearParaDto(respostaJson);
    }

    /// <summary>
    /// Monta o corpo da requisição no formato esperado pela API da Duffel.
    /// </summary>
    private static object MontarCorpoDaRequisicao(DuffelOfferRequestDto request)
    {
        var trechos = request.Trechos.Select(t => new
        {
            origin = t.OrigemIata,
            destination = t.DestinoIata,
            departure_date = t.DataPartida
        }).ToList();

        var passageiros = request.Passageiros.Select(p => new
        {
            type = p.Tipo
        }).ToList();

        var dados = new Dictionary<string, object>
        {
            ["slices"] = trechos,
            ["passengers"] = passageiros
        };

        if (!string.IsNullOrWhiteSpace(request.ClasseCabine))
        {
            dados["cabin_class"] = request.ClasseCabine;
        }

        return new { data = dados };
    }

    /// <summary>
    /// Valida a resposta HTTP e lança exceção em caso de erro.
    /// </summary>
    private static async Task ValidarRespostaHttpAsync(
        HttpResponseMessage resposta,
        CancellationToken cancellationToken)
    {
        if (resposta.IsSuccessStatusCode)
        {
            return;
        }

        var conteudoErro = await resposta.Content.ReadAsStringAsync(cancellationToken);
        var codigoStatus = (int)resposta.StatusCode;

        var mensagemErro = codigoStatus switch
        {
            400 => $"Requisição inválida para a API da Duffel: {conteudoErro}",
            401 => "Falha na autenticação com a API da Duffel. Verifique o token de acesso.",
            403 => "Acesso negado à API da Duffel. Verifique as permissões do token.",
            404 => "Recurso não encontrado na API da Duffel.",
            429 => "Limite de requisições excedido na API da Duffel. Tente novamente mais tarde.",
            >= 500 => $"Erro interno na API da Duffel (HTTP {codigoStatus}). Tente novamente mais tarde.",
            _ => $"Erro na comunicação com a API da Duffel (HTTP {codigoStatus}): {conteudoErro}"
        };

        throw new HttpRequestException(mensagemErro, null, resposta.StatusCode);
    }

    /// <summary>
    /// Mapeia a resposta da API da Duffel para o DTO interno.
    /// </summary>
    private static DuffelOfferResponseDto MapearParaDto(RespostaDuffelWrapper? respostaWrapper)
    {
        if (respostaWrapper?.Data?.Offers == null)
        {
            return new DuffelOfferResponseDto { Ofertas = [] };
        }

        var ofertas = respostaWrapper.Data.Offers.Select(oferta => new DuffelOfferResponseDto.OfertaDto
        {
            Id = oferta.Id ?? string.Empty,
            PrecoTotal = decimal.TryParse(oferta.TotalAmount, out var preco) ? preco : 0m,
            Moeda = oferta.TotalCurrency ?? string.Empty,
            Expiracao = oferta.ExpiresAt,
            CompanhiaAereaCodigo = oferta.Owner?.IataCode ?? string.Empty,
            CompanhiaAereaNome = oferta.Owner?.Name,
            Trechos = MapearTrechos(oferta.Slices),
            Passageiros = MapearPassageiros(oferta.Passengers)
        }).ToList();

        return new DuffelOfferResponseDto { Ofertas = ofertas };
    }

    /// <summary>
    /// Mapeia os trechos (slices) da resposta da Duffel.
    /// </summary>
    private static IReadOnlyCollection<DuffelOfferResponseDto.TrechoRespostaDto> MapearTrechos(
        IReadOnlyCollection<SliceResponse>? slices)
    {
        if (slices == null || slices.Count == 0)
        {
            return [];
        }

        return slices.Select(slice => new DuffelOfferResponseDto.TrechoRespostaDto
        {
            Id = slice.Id ?? string.Empty,
            OrigemIata = slice.Origin?.IataCode ?? string.Empty,
            DestinoIata = slice.Destination?.IataCode ?? string.Empty,
            Duracao = slice.Duration,
            Segmentos = MapearSegmentos(slice.Segments)
        }).ToList();
    }

    /// <summary>
    /// Mapeia os segmentos de voo da resposta da Duffel.
    /// </summary>
    private static IReadOnlyCollection<DuffelOfferResponseDto.SegmentoDto> MapearSegmentos(
        IReadOnlyCollection<SegmentResponse>? segments)
    {
        if (segments == null || segments.Count == 0)
        {
            return [];
        }

        return segments.Select(segment => new DuffelOfferResponseDto.SegmentoDto
        {
            Id = segment.Id ?? string.Empty,
            OrigemIata = segment.Origin?.IataCode ?? string.Empty,
            DestinoIata = segment.Destination?.IataCode ?? string.Empty,
            Partida = segment.DepartingAt,
            Chegada = segment.ArrivingAt,
            Duracao = segment.Duration,
            NumeroVoo = $"{segment.MarketingCarrier?.IataCode ?? string.Empty}{segment.MarketingCarrierFlightNumber ?? string.Empty}",
            CompanhiaAereaCodigo = segment.OperatingCarrier?.IataCode ?? segment.MarketingCarrier?.IataCode ?? string.Empty,
            CompanhiaAereaNome = segment.OperatingCarrier?.Name ?? segment.MarketingCarrier?.Name,
            ClasseCabine = segment.Passengers?.FirstOrDefault()?.CabinClass
        }).ToList();
    }

    /// <summary>
    /// Mapeia os passageiros da resposta da Duffel.
    /// </summary>
    private static IReadOnlyCollection<DuffelOfferResponseDto.PassageiroRespostaDto> MapearPassageiros(
        IReadOnlyCollection<PassengerResponse>? passengers)
    {
        if (passengers == null || passengers.Count == 0)
        {
            return [];
        }

        return passengers.Select(passenger => new DuffelOfferResponseDto.PassageiroRespostaDto
        {
            Id = passenger.Id ?? string.Empty,
            Tipo = passenger.Type ?? string.Empty,
            ValorTarifa = null,
            MoedaTarifa = null
        }).ToList();
    }

    #region Classes internas para deserialização da resposta da Duffel

    /// <summary>
    /// Wrapper da resposta da API da Duffel.
    /// </summary>
    private sealed class RespostaDuffelWrapper
    {
        [JsonPropertyName("data")]
        public OfferRequestResponse? Data { get; init; }
    }

    /// <summary>
    /// Resposta de uma requisição de ofertas da Duffel.
    /// </summary>
    private sealed class OfferRequestResponse
    {
        [JsonPropertyName("offers")]
        public IReadOnlyCollection<OfferResponse>? Offers { get; init; }
    }

    /// <summary>
    /// Oferta de voo retornada pela Duffel.
    /// </summary>
    private sealed class OfferResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("total_amount")]
        public string? TotalAmount { get; init; }

        [JsonPropertyName("total_currency")]
        public string? TotalCurrency { get; init; }

        [JsonPropertyName("expires_at")]
        public DateTime? ExpiresAt { get; init; }

        [JsonPropertyName("owner")]
        public AirlineResponse? Owner { get; init; }

        [JsonPropertyName("slices")]
        public IReadOnlyCollection<SliceResponse>? Slices { get; init; }

        [JsonPropertyName("passengers")]
        public IReadOnlyCollection<PassengerResponse>? Passengers { get; init; }
    }

    /// <summary>
    /// Trecho (slice) de uma oferta da Duffel.
    /// </summary>
    private sealed class SliceResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("origin")]
        public AirportResponse? Origin { get; init; }

        [JsonPropertyName("destination")]
        public AirportResponse? Destination { get; init; }

        [JsonPropertyName("duration")]
        public string? Duration { get; init; }

        [JsonPropertyName("segments")]
        public IReadOnlyCollection<SegmentResponse>? Segments { get; init; }
    }

    /// <summary>
    /// Segmento de voo de uma oferta da Duffel.
    /// </summary>
    private sealed class SegmentResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("origin")]
        public AirportResponse? Origin { get; init; }

        [JsonPropertyName("destination")]
        public AirportResponse? Destination { get; init; }

        [JsonPropertyName("departing_at")]
        public DateTime DepartingAt { get; init; }

        [JsonPropertyName("arriving_at")]
        public DateTime ArrivingAt { get; init; }

        [JsonPropertyName("duration")]
        public string? Duration { get; init; }

        [JsonPropertyName("marketing_carrier")]
        public AirlineResponse? MarketingCarrier { get; init; }

        [JsonPropertyName("marketing_carrier_flight_number")]
        public string? MarketingCarrierFlightNumber { get; init; }

        [JsonPropertyName("operating_carrier")]
        public AirlineResponse? OperatingCarrier { get; init; }

        [JsonPropertyName("passengers")]
        public IReadOnlyCollection<SegmentPassengerResponse>? Passengers { get; init; }
    }

    /// <summary>
    /// Passageiro da requisição de ofertas da Duffel.
    /// </summary>
    private sealed class PassengerResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }

    /// <summary>
    /// Passageiro em um segmento da oferta da Duffel.
    /// </summary>
    private sealed class SegmentPassengerResponse
    {
        [JsonPropertyName("cabin_class")]
        public string? CabinClass { get; init; }
    }

    /// <summary>
    /// Aeroporto da resposta da Duffel.
    /// </summary>
    private sealed class AirportResponse
    {
        [JsonPropertyName("iata_code")]
        public string? IataCode { get; init; }
    }

    /// <summary>
    /// Companhia aérea da resposta da Duffel.
    /// </summary>
    private sealed class AirlineResponse
    {
        [JsonPropertyName("iata_code")]
        public string? IataCode { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    #endregion
}
