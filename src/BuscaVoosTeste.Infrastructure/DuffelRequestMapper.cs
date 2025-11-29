using BuscaVoosTeste.Application.UseCases.BuscarVoos;

namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// Classe responsável por mapear os dados de entrada da aplicação (BuscarVoosInput)
/// para o formato de requisição esperado pela API da Duffel (DuffelOfferRequestDto).
/// </summary>
/// <remarks>
/// Esta classe encapsula toda a lógica de transformação entre o modelo de entrada
/// da camada de Application e o DTO específico da integração com a Duffel,
/// garantindo o correto mapeamento de:
/// - Origem e destino (códigos IATA);
/// - Datas de ida e volta (formato ISO 8601 yyyy-MM-dd);
/// - Quantidade e tipos de passageiros;
/// - Classe de cabine.
/// </remarks>
public static class DuffelRequestMapper
{
    /// <summary>
    /// Formato de data esperado pela API da Duffel (ISO 8601: yyyy-MM-dd).
    /// </summary>
    private const string FormatoDataDuffel = "yyyy-MM-dd";

    /// <summary>
    /// Tipo de passageiro adulto na API da Duffel.
    /// </summary>
    private const string TipoPassageiroAdulto = "adult";

    /// <summary>
    /// Mapeia os dados de entrada da busca de voos (BuscarVoosInput) para o DTO
    /// de requisição de ofertas da Duffel (DuffelOfferRequestDto).
    /// </summary>
    /// <param name="input">
    /// Objeto contendo os critérios de busca de voos da camada de Application,
    /// incluindo origem, destino, data de ida, data de volta (opcional),
    /// quantidade de passageiros e classe de cabine (opcional).
    /// </param>
    /// <returns>
    /// DTO de requisição no formato esperado pela API da Duffel,
    /// contendo trechos (slices), passageiros e classe de cabine.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o parâmetro <paramref name="input"/> é nulo.
    /// </exception>
    /// <remarks>
    /// O mapeamento segue as seguintes regras de negócio:
    /// - Origem e destino são mapeados diretamente usando códigos IATA;
    /// - Data de ida é obrigatória e formatada como yyyy-MM-dd;
    /// - Se data de volta for informada, um segundo trecho (volta) é criado;
    /// - Passageiros são mapeados como adultos (type: "adult") conforme quantidade informada;
    /// - Classe de cabine é convertida para o formato esperado pela Duffel
    ///   (economy, premium_economy, business, first).
    /// </remarks>
    public static DuffelOfferRequestDto MapearParaDuffelOfferRequest(BuscarVoosInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var trechos = CriarTrechos(input);
        var passageiros = CriarPassageiros(input.Passageiros);
        var classeCabine = ConverterClasseCabine(input.Cabine);

        return new DuffelOfferRequestDto
        {
            Trechos = trechos,
            Passageiros = passageiros,
            ClasseCabine = classeCabine
        };
    }

    /// <summary>
    /// Cria a lista de trechos (slices) da viagem com base nos dados de entrada.
    /// </summary>
    /// <param name="input">Dados de entrada da busca de voos.</param>
    /// <returns>
    /// Lista contendo um trecho (somente ida) ou dois trechos (ida e volta),
    /// dependendo se a data de volta foi informada.
    /// </returns>
    private static IReadOnlyCollection<DuffelOfferRequestDto.TrechoDto> CriarTrechos(BuscarVoosInput input)
    {
        var trechos = new List<DuffelOfferRequestDto.TrechoDto>
        {
            new()
            {
                OrigemIata = input.OrigemIata,
                DestinoIata = input.DestinoIata,
                DataPartida = input.DataIda.ToString(FormatoDataDuffel)
            }
        };

        if (input.DataVolta.HasValue)
        {
            trechos.Add(new DuffelOfferRequestDto.TrechoDto
            {
                OrigemIata = input.DestinoIata,
                DestinoIata = input.OrigemIata,
                DataPartida = input.DataVolta.Value.ToString(FormatoDataDuffel)
            });
        }

        return trechos;
    }

    /// <summary>
    /// Cria a lista de passageiros com base na quantidade informada.
    /// </summary>
    /// <param name="quantidadePassageiros">Quantidade de passageiros para a busca.</param>
    /// <returns>
    /// Lista de passageiros do tipo adulto, conforme quantidade informada.
    /// </returns>
    /// <remarks>
    /// Na POC atual, todos os passageiros são mapeados como adultos.
    /// Futuras versões podem suportar diferentes tipos de passageiros
    /// (crianças, bebês) conforme a API da Duffel.
    /// </remarks>
    private static IReadOnlyCollection<DuffelOfferRequestDto.PassageiroDto> CriarPassageiros(int quantidadePassageiros)
    {
        var quantidade = quantidadePassageiros >= 1 ? quantidadePassageiros : 1;

        return Enumerable
            .Range(0, quantidade)
            .Select(_ => new DuffelOfferRequestDto.PassageiroDto
            {
                Tipo = TipoPassageiroAdulto
            })
            .ToList();
    }

    /// <summary>
    /// Converte a classe de cabine do formato interno da aplicação para o formato
    /// esperado pela API da Duffel.
    /// </summary>
    /// <param name="cabineInterna">
    /// Classe de cabine no formato interno (Economy, PremiumEconomy, Business, First).
    /// Pode ser nula.
    /// </param>
    /// <returns>
    /// Classe de cabine no formato da Duffel (economy, premium_economy, business, first),
    /// ou null se não informada.
    /// </returns>
    /// <remarks>
    /// Mapeamento de valores:
    /// - Economy -> economy
    /// - PremiumEconomy -> premium_economy
    /// - Business -> business
    /// - First -> first
    /// Valores não reconhecidos são ignorados (retorna null).
    /// </remarks>
    private static string? ConverterClasseCabine(string? cabineInterna)
    {
        if (string.IsNullOrWhiteSpace(cabineInterna))
        {
            return null;
        }

        return cabineInterna.ToLowerInvariant() switch
        {
            "economy" => "economy",
            "premiumeconomy" => "premium_economy",
            "business" => "business",
            "first" => "first",
            _ => null
        };
    }
}
