using BuscaVoosTeste.Domain.ValueObjects;

namespace BuscaVoosTeste.Domain.Entities;

/// <summary>
/// Entidade que representa uma oferta de voo retornada pelos provedores externos.
/// Contém todas as informações de uma oferta de voo, incluindo rota, preço, 
/// duração, companhia aérea e segmentos do itinerário.
/// </summary>
/// <remarks>
/// Esta entidade garante as seguintes invariantes de domínio:
/// - O identificador deve ser um GUID válido (não vazio);
/// - O código IATA de origem deve ser válido (3 letras maiúsculas);
/// - O código IATA de destino deve ser válido (3 letras maiúsculas);
/// - Origem e destino não podem ser iguais;
/// - O horário de chegada deve ser posterior ao horário de partida;
/// - O código da companhia aérea não pode ser nulo ou vazio;
/// - A lista de segmentos não pode ser nula ou vazia.
/// </remarks>
public sealed class FlightOffer
{
    /// <summary>
    /// Identificador único da oferta de voo (GUID interno da POC).
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Código IATA do aeroporto de origem (3 letras maiúsculas).
    /// </summary>
    public string OrigemIata { get; }

    /// <summary>
    /// Código IATA do aeroporto de destino (3 letras maiúsculas).
    /// </summary>
    public string DestinoIata { get; }

    /// <summary>
    /// Data e horário de partida do primeiro segmento do voo.
    /// </summary>
    public DateTime Partida { get; }

    /// <summary>
    /// Data e horário de chegada do último segmento do voo.
    /// </summary>
    public DateTime Chegada { get; }

    /// <summary>
    /// Duração total do voo, incluindo escalas e conexões.
    /// </summary>
    public Duration DuracaoTotal { get; }

    /// <summary>
    /// Preço total da oferta de voo.
    /// </summary>
    public Money PrecoTotal { get; }

    /// <summary>
    /// Código da companhia aérea principal operadora do voo (ex.: "JJ", "LA", "G3").
    /// </summary>
    public string CompanhiaAereaCodigo { get; }

    /// <summary>
    /// Nome da companhia aérea principal (quando disponível).
    /// </summary>
    public string CompanhiaAereaNome { get; }

    /// <summary>
    /// Classe de cabine da oferta (ex.: Economy, Business, First, PremiumEconomy).
    /// </summary>
    public string Cabine { get; }

    /// <summary>
    /// Lista de segmentos que compõem o itinerário do voo.
    /// Cada segmento representa um trecho direto entre dois aeroportos.
    /// </summary>
    public IReadOnlyCollection<FlightSegment> Segmentos { get; }

    /// <summary>
    /// Construtor privado da entidade FlightOffer.
    /// Use o método de fábrica <see cref="Criar"/> para criar instâncias.
    /// </summary>
    private FlightOffer(
        Guid id,
        string origemIata,
        string destinoIata,
        DateTime partida,
        DateTime chegada,
        Duration duracaoTotal,
        Money precoTotal,
        string companhiaAereaCodigo,
        string companhiaAereaNome,
        string cabine,
        IReadOnlyCollection<FlightSegment> segmentos)
    {
        Id = id;
        OrigemIata = origemIata;
        DestinoIata = destinoIata;
        Partida = partida;
        Chegada = chegada;
        DuracaoTotal = duracaoTotal;
        PrecoTotal = precoTotal;
        CompanhiaAereaCodigo = companhiaAereaCodigo;
        CompanhiaAereaNome = companhiaAereaNome;
        Cabine = cabine;
        Segmentos = segmentos;
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="FlightOffer"/> com validação das invariantes de domínio.
    /// </summary>
    /// <param name="id">Identificador único da oferta de voo (GUID).</param>
    /// <param name="origemIata">Código IATA do aeroporto de origem (3 letras).</param>
    /// <param name="destinoIata">Código IATA do aeroporto de destino (3 letras).</param>
    /// <param name="partida">Data e horário de partida do voo.</param>
    /// <param name="chegada">Data e horário de chegada do voo.</param>
    /// <param name="duracaoTotal">Duração total do voo.</param>
    /// <param name="precoTotal">Preço total da oferta.</param>
    /// <param name="companhiaAereaCodigo">Código da companhia aérea operadora.</param>
    /// <param name="companhiaAereaNome">Nome da companhia aérea (quando disponível).</param>
    /// <param name="cabine">Classe de cabine da oferta.</param>
    /// <param name="segmentos">Lista de segmentos que compõem o itinerário.</param>
    /// <returns>Uma nova instância de <see cref="FlightOffer"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando alguma das invariantes de domínio é violada.
    /// </exception>
    public static FlightOffer Criar(
        Guid id,
        string origemIata,
        string destinoIata,
        DateTime partida,
        DateTime chegada,
        Duration duracaoTotal,
        Money precoTotal,
        string companhiaAereaCodigo,
        string companhiaAereaNome,
        string cabine,
        IReadOnlyCollection<FlightSegment> segmentos)
    {
        ValidarId(id);
        ValidarCodigoIata(origemIata, nameof(origemIata));
        ValidarCodigoIata(destinoIata, nameof(destinoIata));

        var origemIataNormalizado = origemIata.ToUpperInvariant();
        var destinoIataNormalizado = destinoIata.ToUpperInvariant();
        var companhiaAereaCodigoNormalizado = companhiaAereaCodigo.ToUpperInvariant();

        ValidarOrigemDestinoDistintos(origemIataNormalizado, destinoIataNormalizado);
        ValidarHorarios(partida, chegada);
        ValidarCompanhiaAereaCodigo(companhiaAereaCodigo);
        ValidarSegmentos(segmentos);

        return new FlightOffer(
            id,
            origemIataNormalizado,
            destinoIataNormalizado,
            partida,
            chegada,
            duracaoTotal,
            precoTotal,
            companhiaAereaCodigoNormalizado,
            companhiaAereaNome ?? string.Empty,
            cabine ?? string.Empty,
            segmentos);
    }

    /// <summary>
    /// Valida se o identificador é válido (não é um GUID vazio).
    /// </summary>
    /// <param name="id">Identificador a ser validado.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o identificador é um GUID vazio.
    /// </exception>
    private static void ValidarId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da oferta não pode ser um GUID vazio.", nameof(id));
        }
    }

    /// <summary>
    /// Valida se o código IATA é válido (3 letras).
    /// </summary>
    /// <param name="codigoIata">Código IATA a ser validado.</param>
    /// <param name="nomeCampo">Nome do campo para mensagem de erro.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o código IATA é nulo, vazio ou não possui exatamente 3 letras.
    /// </exception>
    private static void ValidarCodigoIata(string codigoIata, string nomeCampo)
    {
        if (string.IsNullOrWhiteSpace(codigoIata))
        {
            throw new ArgumentException($"O código IATA de {nomeCampo} não pode ser nulo ou vazio.", nomeCampo);
        }

        if (codigoIata.Length != 3)
        {
            throw new ArgumentException($"O código IATA de {nomeCampo} deve ter exatamente 3 caracteres.", nomeCampo);
        }

        if (!codigoIata.All(char.IsLetter))
        {
            throw new ArgumentException($"O código IATA de {nomeCampo} deve conter apenas letras.", nomeCampo);
        }
    }

    /// <summary>
    /// Valida se origem e destino são diferentes.
    /// </summary>
    /// <param name="origemIata">Código IATA de origem.</param>
    /// <param name="destinoIata">Código IATA de destino.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando origem e destino são iguais.
    /// </exception>
    private static void ValidarOrigemDestinoDistintos(string origemIata, string destinoIata)
    {
        if (string.Equals(origemIata, destinoIata, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A origem e o destino não podem ser iguais.");
        }
    }

    /// <summary>
    /// Valida se o horário de chegada é posterior ao horário de partida.
    /// </summary>
    /// <param name="partida">Horário de partida.</param>
    /// <param name="chegada">Horário de chegada.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o horário de chegada não é posterior ao de partida.
    /// </exception>
    private static void ValidarHorarios(DateTime partida, DateTime chegada)
    {
        if (chegada <= partida)
        {
            throw new ArgumentException("O horário de chegada deve ser posterior ao horário de partida.", nameof(chegada));
        }
    }

    /// <summary>
    /// Valida se o código da companhia aérea é válido.
    /// </summary>
    /// <param name="companhiaAereaCodigo">Código da companhia aérea a ser validado.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o código da companhia aérea é nulo ou vazio.
    /// </exception>
    private static void ValidarCompanhiaAereaCodigo(string companhiaAereaCodigo)
    {
        if (string.IsNullOrWhiteSpace(companhiaAereaCodigo))
        {
            throw new ArgumentException("O código da companhia aérea não pode ser nulo ou vazio.", nameof(companhiaAereaCodigo));
        }
    }

    /// <summary>
    /// Valida se a lista de segmentos é válida (não nula e não vazia).
    /// </summary>
    /// <param name="segmentos">Lista de segmentos a ser validada.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando a lista de segmentos é nula ou vazia.
    /// </exception>
    private static void ValidarSegmentos(IReadOnlyCollection<FlightSegment> segmentos)
    {
        if (segmentos == null || segmentos.Count == 0)
        {
            throw new ArgumentException("A lista de segmentos não pode ser nula ou vazia.", nameof(segmentos));
        }
    }
}
