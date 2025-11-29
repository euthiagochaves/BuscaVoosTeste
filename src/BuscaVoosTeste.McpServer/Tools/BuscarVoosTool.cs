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
        ValidarParametrosObrigatorios(origem, destino, dataIda);

        var dataIdaParsed = ParsearData(dataIda, "data de ida");
        var dataVoltaParsed = !string.IsNullOrEmpty(dataVolta)
            ? ParsearData(dataVolta, "data de volta")
            : (DateTime?)null;

        var input = new BuscarVoosInput
        {
            OrigemIata = origem,
            DestinoIata = destino,
            DataIda = dataIdaParsed,
            DataVolta = dataVoltaParsed,
            Passageiros = passageiros,
            Cabine = cabine
        };

        var ofertas = await _buscarVoosUseCase.ExecuteAsync(input, cancellationToken);

        return ofertas;
    }

    /// <summary>
    /// Valida os parâmetros obrigatórios da busca de voos.
    /// </summary>
    /// <param name="origem">Código IATA de origem.</param>
    /// <param name="destino">Código IATA de destino.</param>
    /// <param name="dataIda">Data de ida.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando algum parâmetro obrigatório é nulo ou vazio.
    /// </exception>
    private static void ValidarParametrosObrigatorios(string origem, string destino, string dataIda)
    {
        if (string.IsNullOrWhiteSpace(origem))
        {
            throw new ArgumentException("O código IATA de origem é obrigatório.", nameof(origem));
        }

        if (string.IsNullOrWhiteSpace(destino))
        {
            throw new ArgumentException("O código IATA de destino é obrigatório.", nameof(destino));
        }

        if (string.IsNullOrWhiteSpace(dataIda))
        {
            throw new ArgumentException("A data de ida é obrigatória.", nameof(dataIda));
        }
    }

    /// <summary>
    /// Converte uma string de data no formato ISO (yyyy-MM-dd) para DateTime.
    /// </summary>
    /// <param name="data">String com a data no formato ISO.</param>
    /// <param name="nomeCampo">Nome do campo para mensagem de erro.</param>
    /// <returns>O DateTime correspondente à data informada.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando a data não está no formato esperado (yyyy-MM-dd).
    /// </exception>
    private static DateTime ParsearData(string data, string nomeCampo)
    {
        if (!DateTime.TryParseExact(
            data,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var resultado))
        {
            throw new ArgumentException(
                $"A {nomeCampo} '{data}' não está no formato esperado (yyyy-MM-dd).",
                nomeCampo);
        }

        return resultado;
    }
}
