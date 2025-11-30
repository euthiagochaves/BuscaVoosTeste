using BuscaVoosTeste.Application.Providers;
using BuscaVoosTeste.Domain.Entities;

namespace BuscaVoosTeste.Application.UseCases.BuscarVoos;

/// <summary>
/// Implementação do caso de uso de busca de voos.
/// Orquestra a validação dos dados de entrada e a chamada ao provedor de busca de voos.
/// </summary>
/// <remarks>
/// Este caso de uso é responsável por:
/// - Validar os parâmetros de entrada conforme as regras de negócio;
/// - Delegar a busca ao provedor de voos (IFlightSearchProvider);
/// - Retornar as ofertas de voo encontradas ou uma lista vazia caso não haja resultados.
/// </remarks>
public sealed class BuscarVoosUseCase : IBuscarVoosUseCase
{
    private readonly IFlightSearchProvider _provedorBuscaVoos;

    /// <summary>
    /// Inicializa uma nova instância do caso de uso de busca de voos.
    /// </summary>
    /// <param name="provedorBuscaVoos">Provedor de busca de voos (abstração da infraestrutura).</param>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o provedor de busca de voos é nulo.
    /// </exception>
    public BuscarVoosUseCase(IFlightSearchProvider provedorBuscaVoos)
    {
        _provedorBuscaVoos = provedorBuscaVoos ?? throw new ArgumentNullException(nameof(provedorBuscaVoos));
    }

    /// <summary>
    /// Executa a busca de ofertas de voos com base nos critérios informados.
    /// </summary>
    /// <param name="input">Objeto contendo os parâmetros de busca de voos.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
    /// <returns>
    /// Uma coleção somente leitura de ofertas de voo que atendem aos critérios de busca.
    /// Retorna uma coleção vazia caso nenhuma oferta seja encontrada.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o parâmetro de entrada é nulo.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Lançada quando alguma regra de validação é violada (origem/destino vazios, 
    /// origem igual ao destino, passageiros inválidos, etc.).
    /// </exception>
    public async Task<IReadOnlyCollection<FlightOffer>> ExecuteAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default)
    {
        ValidarEntrada(input);

        var ofertas = await _provedorBuscaVoos.SearchAsync(input, cancellationToken);

        return ofertas;
    }

    /// <summary>
    /// Valida os dados de entrada conforme as regras de negócio da busca de voos.
    /// </summary>
    /// <param name="input">Objeto de entrada a ser validado.</param>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o parâmetro de entrada é nulo.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Lançada quando alguma regra de validação é violada.
    /// </exception>
    private static void ValidarEntrada(BuscarVoosInput input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input), "Os parâmetros de busca não podem ser nulos.");
        }

        ValidarOrigemIata(input.OrigemIata);
        ValidarDestinoIata(input.DestinoIata);
        ValidarOrigemDestinoDistintos(input.OrigemIata, input.DestinoIata);
        ValidarPassageiros(input.Passageiros);
    }

    /// <summary>
    /// Valida o código IATA de origem.
    /// </summary>
    /// <param name="origemIata">Código IATA de origem.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o código IATA de origem é nulo ou vazio.
    /// </exception>
    private static void ValidarOrigemIata(string origemIata)
    {
        if (string.IsNullOrWhiteSpace(origemIata))
        {
            throw new ArgumentException("O código IATA de origem não pode ser nulo ou vazio.", nameof(origemIata));
        }
    }

    /// <summary>
    /// Valida o código IATA de destino.
    /// </summary>
    /// <param name="destinoIata">Código IATA de destino.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o código IATA de destino é nulo ou vazio.
    /// </exception>
    private static void ValidarDestinoIata(string destinoIata)
    {
        if (string.IsNullOrWhiteSpace(destinoIata))
        {
            throw new ArgumentException("O código IATA de destino não pode ser nulo ou vazio.", nameof(destinoIata));
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
    /// Valida a quantidade de passageiros.
    /// </summary>
    /// <param name="passageiros">Quantidade de passageiros.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando a quantidade de passageiros é menor que 1.
    /// </exception>
    private static void ValidarPassageiros(int passageiros)
    {
        if (passageiros < 1)
        {
            throw new ArgumentException("A quantidade de passageiros deve ser maior ou igual a 1.", nameof(passageiros));
        }
    }
}
