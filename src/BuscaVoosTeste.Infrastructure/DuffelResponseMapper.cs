using BuscaVoosTeste.Domain.Entities;
using BuscaVoosTeste.Domain.ValueObjects;

namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// Classe responsável por mapear as respostas da API da Duffel (DuffelOfferResponseDto)
/// para as entidades de domínio (FlightOffer, FlightSegment, etc.).
/// </summary>
/// <remarks>
/// Esta classe encapsula toda a lógica de transformação entre o DTO específico da integração
/// com a Duffel e o modelo de domínio definido em BuscaVoosTeste.Domain, garantindo:
/// - Conversão correta de ofertas, segmentos, durações e preços;
/// - Normalização de dados (códigos IATA, moedas, classes de cabine);
/// - Tratamento de campos opcionais com valores padrão adequados;
/// - Geração de identificadores internos (GUID) para cada oferta.
/// 
/// A classe é utilizada pelo provider de busca de voos da Duffel (DuffelFlightSearchProvider)
/// para transformar as respostas JSON da API externa em objetos de domínio.
/// </remarks>
public static class DuffelResponseMapper
{
    /// <summary>
    /// Classe de cabine padrão quando não informada na resposta da Duffel.
    /// </summary>
    private const string ClasseCabinePadrao = "Economy";

    /// <summary>
    /// Nome padrão da companhia aérea quando não disponível na resposta da Duffel.
    /// </summary>
    private const string CompanhiaAereaNomePadrao = "";

    /// <summary>
    /// Duração padrão em formato ISO 8601 quando não disponível na resposta.
    /// Representa 0 minutos.
    /// </summary>
    private const string DuracaoPadraoIso8601 = "PT0M";

    /// <summary>
    /// Mapeia a resposta de ofertas da Duffel (DuffelOfferResponseDto) para uma coleção
    /// de entidades de domínio FlightOffer.
    /// </summary>
    /// <param name="resposta">
    /// DTO de resposta da Duffel contendo a lista de ofertas de voo.
    /// </param>
    /// <returns>
    /// Coleção de ofertas de voo mapeadas para o modelo de domínio.
    /// Retorna uma coleção vazia (nunca nula) se não houver ofertas.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o parâmetro <paramref name="resposta"/> é nulo.
    /// </exception>
    /// <remarks>
    /// Este método é o ponto de entrada principal para o mapeamento de respostas da Duffel.
    /// Ele itera sobre todas as ofertas retornadas pela API e converte cada uma para
    /// a entidade de domínio FlightOffer, utilizando os métodos auxiliares para
    /// converter segmentos, durações, preços e demais informações.
    /// 
    /// Regras de negócio aplicadas:
    /// - Cada oferta recebe um novo GUID interno (não usa o ID da Duffel diretamente);
    /// - Origem e destino são extraídos do primeiro trecho da oferta;
    /// - Partida é a data/hora do primeiro segmento do primeiro trecho;
    /// - Chegada é a data/hora do último segmento do último trecho;
    /// - A duração total é calculada a partir da duração do primeiro trecho;
    /// - Ofertas inválidas (sem trechos ou segmentos) são ignoradas.
    /// </remarks>
    public static IReadOnlyCollection<FlightOffer> MapearParaDominio(DuffelOfferResponseDto resposta)
    {
        ArgumentNullException.ThrowIfNull(resposta);

        if (resposta.Ofertas == null || resposta.Ofertas.Count == 0)
        {
            return [];
        }

        var ofertasDominio = new List<FlightOffer>();

        foreach (var ofertaDto in resposta.Ofertas)
        {
            var ofertaDominio = MapearOferta(ofertaDto);
            if (ofertaDominio != null)
            {
                ofertasDominio.Add(ofertaDominio);
            }
        }

        return ofertasDominio;
    }

    /// <summary>
    /// Mapeia uma oferta individual da Duffel para a entidade de domínio FlightOffer.
    /// </summary>
    /// <param name="ofertaDto">DTO da oferta individual da Duffel.</param>
    /// <returns>
    /// Instância de FlightOffer mapeada, ou null se a oferta for inválida
    /// (sem trechos ou segmentos).
    /// </returns>
    /// <remarks>
    /// Este método realiza as seguintes transformações:
    /// - Gera um novo GUID para o Id da oferta;
    /// - Extrai origem e destino do primeiro trecho (ida);
    /// - Extrai partida do primeiro segmento e chegada do último segmento do trecho;
    /// - Converte duração total a partir do primeiro trecho;
    /// - Converte preço total para o value object Money;
    /// - Converte a classe de cabine para o formato interno;
    /// - Mapeia todos os segmentos do primeiro trecho (ida).
    /// 
    /// Nota: Cada trecho na resposta da Duffel representa uma parte do itinerário
    /// (ida ou volta). Este mapeamento considera apenas o primeiro trecho (ida)
    /// para criar uma FlightOffer. Se houver trecho de volta, ele seria mapeado
    /// separadamente em uma implementação de viagem de ida e volta.
    /// </remarks>
    private static FlightOffer? MapearOferta(DuffelOfferResponseDto.OfertaDto ofertaDto)
    {
        // Validar se a oferta possui trechos
        if (ofertaDto.Trechos == null || ofertaDto.Trechos.Count == 0)
        {
            return null;
        }

        // Obter o primeiro trecho (ida) para extrair informações principais
        var primeiroTrecho = ofertaDto.Trechos.First();

        // Validar se o trecho possui segmentos
        if (primeiroTrecho.Segmentos == null || primeiroTrecho.Segmentos.Count == 0)
        {
            return null;
        }

        // Extrair dados do primeiro e último segmento do trecho de ida
        var primeiroSegmento = primeiroTrecho.Segmentos.First();
        var ultimoSegmentoDoTrecho = primeiroTrecho.Segmentos.Last();

        // Mapear segmentos do trecho de ida para entidades de domínio
        var segmentosDominio = MapearSegmentos(primeiroTrecho.Segmentos);

        // Se não houver segmentos válidos, retornar null
        if (segmentosDominio.Count == 0)
        {
            return null;
        }

        // Mapear duração total
        var duracaoTotal = MapearDuracao(primeiroTrecho.Duracao);

        // Mapear preço total
        var precoTotal = MapearPreco(ofertaDto.PrecoTotal, ofertaDto.Moeda);

        // Mapear classe de cabine (usar do primeiro segmento ou padrão)
        var cabine = MapearClasseCabine(primeiroSegmento.ClasseCabine);

        // Criar a entidade de domínio FlightOffer
        return FlightOffer.Criar(
            id: Guid.NewGuid(),
            origemIata: primeiroTrecho.OrigemIata,
            destinoIata: primeiroTrecho.DestinoIata,
            partida: primeiroSegmento.Partida,
            chegada: ultimoSegmentoDoTrecho.Chegada,
            duracaoTotal: duracaoTotal,
            precoTotal: precoTotal,
            companhiaAereaCodigo: ofertaDto.CompanhiaAereaCodigo,
            companhiaAereaNome: ofertaDto.CompanhiaAereaNome ?? CompanhiaAereaNomePadrao,
            cabine: cabine,
            segmentos: segmentosDominio);
    }

    /// <summary>
    /// Mapeia uma coleção de segmentos da Duffel para entidades de domínio FlightSegment.
    /// </summary>
    /// <param name="segmentosDto">Coleção de DTOs de segmentos da Duffel.</param>
    /// <returns>
    /// Lista de segmentos de domínio mapeados.
    /// Segmentos inválidos são ignorados.
    /// </returns>
    private static IReadOnlyCollection<FlightSegment> MapearSegmentos(
        IReadOnlyCollection<DuffelOfferResponseDto.SegmentoDto> segmentosDto)
    {
        var segmentosDominio = new List<FlightSegment>();

        foreach (var segmentoDto in segmentosDto)
        {
            var segmentoDominio = MapearSegmento(segmentoDto);
            if (segmentoDominio != null)
            {
                segmentosDominio.Add(segmentoDominio);
            }
        }

        return segmentosDominio;
    }

    /// <summary>
    /// Mapeia um segmento individual da Duffel para a entidade de domínio FlightSegment.
    /// </summary>
    /// <param name="segmentoDto">DTO do segmento da Duffel.</param>
    /// <returns>
    /// Instância de FlightSegment mapeada, ou null se o segmento for inválido.
    /// </returns>
    /// <remarks>
    /// Este método converte:
    /// - Códigos IATA de origem e destino;
    /// - Horários de partida e chegada;
    /// - Número do voo;
    /// - Código da companhia aérea;
    /// - Duração do segmento (formato ISO 8601).
    /// </remarks>
    private static FlightSegment? MapearSegmento(DuffelOfferResponseDto.SegmentoDto segmentoDto)
    {
        try
        {
            var duracao = MapearDuracao(segmentoDto.Duracao);

            return FlightSegment.Criar(
                origemIata: segmentoDto.OrigemIata,
                destinoIata: segmentoDto.DestinoIata,
                partida: segmentoDto.Partida,
                chegada: segmentoDto.Chegada,
                numeroVoo: segmentoDto.NumeroVoo,
                companhiaAereaCodigo: segmentoDto.CompanhiaAereaCodigo,
                duracao: duracao);
        }
        catch (ArgumentException)
        {
            // Se houver erro de validação no domínio, ignorar o segmento
            return null;
        }
    }

    /// <summary>
    /// Mapeia uma string de duração no formato ISO 8601 para o value object Duration.
    /// </summary>
    /// <param name="duracaoIso8601">
    /// String de duração no formato ISO 8601 (ex.: PT2H30M).
    /// Pode ser nula ou vazia.
    /// </param>
    /// <returns>
    /// Value object Duration correspondente.
    /// Se a string for nula ou vazia, retorna uma duração de 0 minutos.
    /// Se o formato for inválido, retorna uma duração de 0 minutos.
    /// </returns>
    /// <remarks>
    /// O formato ISO 8601 para duração é: PT[n]H[n]M
    /// Onde n representa a quantidade de horas ou minutos.
    /// Exemplos: PT2H30M (2h 30min), PT1H (1h), PT45M (45min).
    /// </remarks>
    private static Duration MapearDuracao(string? duracaoIso8601)
    {
        if (string.IsNullOrWhiteSpace(duracaoIso8601))
        {
            return Duration.CriarAPartirDeIso8601(DuracaoPadraoIso8601);
        }

        try
        {
            return Duration.CriarAPartirDeIso8601(duracaoIso8601);
        }
        catch (ArgumentException)
        {
            // Se o formato for inválido, usar duração padrão
            return Duration.CriarAPartirDeIso8601(DuracaoPadraoIso8601);
        }
    }

    /// <summary>
    /// Mapeia o valor e moeda do preço para o value object Money.
    /// </summary>
    /// <param name="valor">Valor numérico do preço.</param>
    /// <param name="moeda">Código da moeda (ex.: BRL, USD).</param>
    /// <returns>
    /// Value object Money com o valor e moeda mapeados.
    /// </returns>
    /// <remarks>
    /// Este método aplica as seguintes normalizações defensivas:
    /// - Valores negativos são tratados como 0 (não deveria ocorrer em uma API válida);
    /// - Moedas vazias ou nulas são substituídas por "BRL" como padrão.
    /// 
    /// O value object Money garante que:
    /// - O valor não seja negativo (validado no construtor);
    /// - A moeda seja normalizada para maiúsculas.
    /// </remarks>
    private static Money MapearPreco(decimal valor, string moeda)
    {
        // Normalização defensiva: valores negativos não são esperados da API da Duffel,
        // mas tratamos como 0 para evitar exceções no construtor de Money.
        var valorNormalizado = valor >= 0 ? valor : 0;

        // Normalização defensiva: usar BRL como moeda padrão se não informada
        var moedaNormalizada = string.IsNullOrWhiteSpace(moeda) ? "BRL" : moeda;

        return new Money(valorNormalizado, moedaNormalizada);
    }

    /// <summary>
    /// Mapeia a classe de cabine do formato da Duffel para o formato interno do domínio.
    /// </summary>
    /// <param name="classeCabineDuffel">
    /// Classe de cabine no formato da Duffel (economy, premium_economy, business, first).
    /// Pode ser nula.
    /// </param>
    /// <returns>
    /// Classe de cabine no formato interno (Economy, PremiumEconomy, Business, First).
    /// Retorna "Economy" como padrão se não informada ou não reconhecida.
    /// </returns>
    /// <remarks>
    /// Mapeamento de valores:
    /// - economy -> Economy
    /// - premium_economy -> PremiumEconomy
    /// - business -> Business
    /// - first -> First
    /// - Qualquer outro valor ou nulo -> Economy
    /// </remarks>
    private static string MapearClasseCabine(string? classeCabineDuffel)
    {
        if (string.IsNullOrWhiteSpace(classeCabineDuffel))
        {
            return ClasseCabinePadrao;
        }

        return classeCabineDuffel.ToLowerInvariant() switch
        {
            "economy" => "Economy",
            "premium_economy" => "PremiumEconomy",
            "business" => "Business",
            "first" => "First",
            _ => ClasseCabinePadrao
        };
    }
}
