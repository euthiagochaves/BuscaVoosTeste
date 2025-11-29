using BuscaVoosTeste.Domain.ValueObjects;

namespace BuscaVoosTeste.Domain.Entities;

/// <summary>
/// Entidade que representa um trecho individual de voo dentro de uma oferta de voo.
/// Um segmento de voo é um trecho direto entre dois aeroportos, sem escalas intermediárias.
/// </summary>
/// <remarks>
/// Esta entidade garante as seguintes invariantes de domínio:
/// - O código IATA de origem deve ser válido (3 letras maiúsculas);
/// - O código IATA de destino deve ser válido (3 letras maiúsculas);
/// - Origem e destino não podem ser iguais;
/// - O horário de chegada deve ser posterior ao horário de partida;
/// - O número do voo não pode ser nulo ou vazio;
/// - O código da companhia aérea não pode ser nulo ou vazio.
/// </remarks>
public sealed class FlightSegment
{
    /// <summary>
    /// Código IATA do aeroporto de origem (3 letras maiúsculas).
    /// </summary>
    public string OrigemIata { get; }

    /// <summary>
    /// Código IATA do aeroporto de destino (3 letras maiúsculas).
    /// </summary>
    public string DestinoIata { get; }

    /// <summary>
    /// Data e horário de partida do voo.
    /// </summary>
    public DateTime Partida { get; }

    /// <summary>
    /// Data e horário de chegada do voo.
    /// </summary>
    public DateTime Chegada { get; }

    /// <summary>
    /// Número do voo (ex.: "JJ1234", "LA3456").
    /// </summary>
    public string NumeroVoo { get; }

    /// <summary>
    /// Código da companhia aérea operadora do voo (ex.: "JJ", "LA", "G3").
    /// </summary>
    public string CompanhiaAereaCodigo { get; }

    /// <summary>
    /// Duração do trecho de voo.
    /// </summary>
    public Duration Duracao { get; }

    /// <summary>
    /// Construtor privado da entidade FlightSegment.
    /// Use o método de fábrica <see cref="Criar"/> para criar instâncias.
    /// </summary>
    private FlightSegment(
        string origemIata,
        string destinoIata,
        DateTime partida,
        DateTime chegada,
        string numeroVoo,
        string companhiaAereaCodigo,
        Duration duracao)
    {
        OrigemIata = origemIata;
        DestinoIata = destinoIata;
        Partida = partida;
        Chegada = chegada;
        NumeroVoo = numeroVoo;
        CompanhiaAereaCodigo = companhiaAereaCodigo;
        Duracao = duracao;
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="FlightSegment"/> com validação das invariantes de domínio.
    /// </summary>
    /// <param name="origemIata">Código IATA do aeroporto de origem (3 letras).</param>
    /// <param name="destinoIata">Código IATA do aeroporto de destino (3 letras).</param>
    /// <param name="partida">Data e horário de partida do voo.</param>
    /// <param name="chegada">Data e horário de chegada do voo.</param>
    /// <param name="numeroVoo">Número do voo.</param>
    /// <param name="companhiaAereaCodigo">Código da companhia aérea operadora.</param>
    /// <param name="duracao">Duração do trecho de voo.</param>
    /// <returns>Uma nova instância de <see cref="FlightSegment"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando alguma das invariantes de domínio é violada.
    /// </exception>
    public static FlightSegment Criar(
        string origemIata,
        string destinoIata,
        DateTime partida,
        DateTime chegada,
        string numeroVoo,
        string companhiaAereaCodigo,
        Duration duracao)
    {
        ValidarCodigoIata(origemIata, nameof(origemIata));
        ValidarCodigoIata(destinoIata, nameof(destinoIata));
        ValidarOrigemDestinoDistintos(origemIata, destinoIata);
        ValidarHorarios(partida, chegada);
        ValidarNumeroVoo(numeroVoo);
        ValidarCompanhiaAereaCodigo(companhiaAereaCodigo);

        return new FlightSegment(
            origemIata.ToUpperInvariant(),
            destinoIata.ToUpperInvariant(),
            partida,
            chegada,
            numeroVoo,
            companhiaAereaCodigo.ToUpperInvariant(),
            duracao);
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
    /// Valida se o número do voo é válido.
    /// </summary>
    /// <param name="numeroVoo">Número do voo a ser validado.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o número do voo é nulo ou vazio.
    /// </exception>
    private static void ValidarNumeroVoo(string numeroVoo)
    {
        if (string.IsNullOrWhiteSpace(numeroVoo))
        {
            throw new ArgumentException("O número do voo não pode ser nulo ou vazio.", nameof(numeroVoo));
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
}
