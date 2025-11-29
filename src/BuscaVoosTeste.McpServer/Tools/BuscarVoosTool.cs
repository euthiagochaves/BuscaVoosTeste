using System.ComponentModel;
using BuscaVoosTeste.Application.UseCases.BuscarVoos;
using BuscaVoosTeste.Domain.Entities;
using ModelContextProtocol.Server;

namespace BuscaVoosTeste.McpServer.Tools;

/// <summary>
/// Tool MCP para busca de voos.
/// Esta Tool expõe o caso de uso de busca de voos para clientes MCP como o GitHub Copilot.
/// </summary>
/// <remarks>
/// A Tool recebe os parâmetros de busca do agente (origem, destino, datas, passageiros, cabine)
/// e retorna uma lista de ofertas de voo disponíveis.
/// 
/// Exemplo de uso via agente:
/// - "Buscar voos de GRU para EZE no dia 10 de janeiro, 1 passageiro em classe econômica."
/// - "Quero opções de voos de São Paulo para Buenos Aires, ida e volta, 2 passageiros."
/// </remarks>
public sealed class BuscarVoosTool
{
    private readonly IBuscarVoosUseCase _buscarVoosUseCase;

    /// <summary>
    /// Inicializa uma nova instância da Tool de busca de voos.
    /// </summary>
    /// <param name="buscarVoosUseCase">Caso de uso de busca de voos.</param>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o caso de uso é nulo.
    /// </exception>
    public BuscarVoosTool(IBuscarVoosUseCase buscarVoosUseCase)
    {
        _buscarVoosUseCase = buscarVoosUseCase ?? throw new ArgumentNullException(nameof(buscarVoosUseCase));
    }

    /// <summary>
    /// Busca ofertas de voos com base nos critérios informados.
    /// </summary>
    /// <param name="origem">Código IATA do aeroporto de origem (3 letras, ex.: GRU, CNF, GIG).</param>
    /// <param name="destino">Código IATA do aeroporto de destino (3 letras, ex.: GRU, CNF, GIG).</param>
    /// <param name="dataIda">Data de ida no formato ISO (yyyy-MM-dd).</param>
    /// <param name="dataVolta">Data de volta no formato ISO (yyyy-MM-dd). Opcional, para viagens de ida e volta.</param>
    /// <param name="passageiros">Quantidade de passageiros. Valor padrão é 1.</param>
    /// <param name="cabine">Classe de cabine desejada (Economy, PremiumEconomy, Business, First). Opcional.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    /// <returns>Lista de ofertas de voo disponíveis conforme os critérios informados.</returns>
    [McpServerTool(Name = "buscar_voos")]
    [Description("Busca ofertas de voos disponíveis com base na origem, destino, datas, quantidade de passageiros e classe de cabine. " +
                 "Retorna uma lista de ofertas contendo informações como preço, duração, companhia aérea e segmentos do itinerário.")]
    public async Task<IReadOnlyCollection<FlightOffer>> BuscarVoosAsync(
        [Description("Código IATA do aeroporto de origem (3 letras, ex.: GRU, CNF, GIG)")] string origem,
        [Description("Código IATA do aeroporto de destino (3 letras, ex.: GRU, CNF, GIG)")] string destino,
        [Description("Data de ida no formato ISO (yyyy-MM-dd)")] string dataIda,
        [Description("Data de volta no formato ISO (yyyy-MM-dd). Opcional, para viagens de ida e volta")] string? dataVolta = null,
        [Description("Quantidade de passageiros (padrão: 1)")] int passageiros = 1,
        [Description("Classe de cabine (Economy, PremiumEconomy, Business, First). Opcional")] string? cabine = null,
        CancellationToken cancellationToken = default)
    {
        var input = new BuscarVoosInput
        {
            OrigemIata = origem,
            DestinoIata = destino,
            DataIda = DateTime.Parse(dataIda),
            DataVolta = !string.IsNullOrEmpty(dataVolta) ? DateTime.Parse(dataVolta) : null,
            Passageiros = passageiros,
            Cabine = cabine
        };

        var ofertas = await _buscarVoosUseCase.ExecuteAsync(input, cancellationToken);

        return ofertas;
    }
}
