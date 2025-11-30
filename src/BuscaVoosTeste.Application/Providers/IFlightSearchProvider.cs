using BuscaVoosTeste.Application.UseCases.BuscarVoos;
using BuscaVoosTeste.Domain.Entities;

namespace BuscaVoosTeste.Application.Providers;

/// <summary>
/// Interface que define o contrato para provedores de busca de voos na camada de aplicação.
/// </summary>
/// <remarks>
/// Esta interface abstrai o mecanismo de busca de voos, permitindo que diferentes implementações
/// concretas (por exemplo, integração com a API Duffel) sejam utilizadas na camada de Infrastructure
/// sem acoplar a camada de Application a detalhes de implementação externa.
/// 
/// A interface segue o princípio de inversão de dependência (DIP) da Clean Architecture,
/// permitindo que os casos de uso dependam de abstrações em vez de implementações concretas.
/// </remarks>
public interface IFlightSearchProvider
{
    /// <summary>
    /// Realiza a busca de ofertas de voos com base nos critérios informados.
    /// </summary>
    /// <param name="input">
    /// Objeto contendo os critérios de busca de voos, incluindo:
    /// origem, destino, data de ida, data de volta (opcional),
    /// quantidade de passageiros e classe de cabine (opcional).
    /// </param>
    /// <param name="cancellationToken">
    /// Token para cancelamento da operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma coleção somente leitura de ofertas de voo (<see cref="FlightOffer"/>) 
    /// que atendem aos critérios de busca especificados.
    /// A coleção pode estar vazia caso nenhuma oferta seja encontrada.
    /// </returns>
    Task<IReadOnlyCollection<FlightOffer>> SearchAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default);
}
