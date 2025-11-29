using BuscaVoosTeste.Domain.Entities;

namespace BuscaVoosTeste.Application.UseCases.BuscarVoos;

/// <summary>
/// Interface que define o contrato do caso de uso de busca de voos.
/// </summary>
/// <remarks>
/// Esta interface representa o ponto de entrada principal para a funcionalidade de busca de voos
/// na camada de aplicação, seguindo o padrão Clean Architecture.
/// 
/// A implementação deste caso de uso é responsável por:
/// - Validar os parâmetros de entrada recebidos;
/// - Orquestrar a chamada ao provedor de busca de voos (IFlightSearchProvider);
/// - Aplicar regras de negócio adicionais, se necessário;
/// - Retornar as ofertas de voo encontradas.
/// </remarks>
public interface IBuscarVoosUseCase
{
    /// <summary>
    /// Executa a busca de ofertas de voos com base nos critérios informados.
    /// </summary>
    /// <param name="input">
    /// Objeto contendo os parâmetros de busca de voos, incluindo:
    /// origem (código IATA), destino (código IATA), data de ida, 
    /// data de volta (opcional), quantidade de passageiros e classe de cabine (opcional).
    /// </param>
    /// <param name="cancellationToken">
    /// Token para cancelamento da operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma coleção somente leitura de ofertas de voo (<see cref="FlightOffer"/>) 
    /// que atendem aos critérios de busca especificados.
    /// A coleção pode estar vazia caso nenhuma oferta seja encontrada.
    /// </returns>
    Task<IReadOnlyCollection<FlightOffer>> ExecuteAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default);
}
