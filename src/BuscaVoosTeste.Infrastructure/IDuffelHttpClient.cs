namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// Interface que define o contrato de comunicação HTTP com a API da Duffel.
/// Encapsula as chamadas HTTP de baixo nível para operações de busca de ofertas de voo.
/// </summary>
/// <remarks>
/// Esta interface é a porta de entrada padronizada para chamadas HTTP à API da Duffel
/// dentro da camada de infraestrutura. Será utilizada por providers como o
/// DuffelFlightSearchProvider para realizar buscas de ofertas de voo.
/// </remarks>
public interface IDuffelHttpClient
{
    /// <summary>
    /// Cria uma requisição de ofertas de voo na API da Duffel.
    /// Envia uma requisição POST para o endpoint /air/offer_requests da Duffel
    /// e retorna as ofertas de voo disponíveis.
    /// </summary>
    /// <param name="request">DTO contendo os parâmetros da busca de ofertas (trechos, passageiros, classe de cabine).</param>
    /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
    /// <returns>
    /// DTO contendo as ofertas de voo retornadas pela API da Duffel,
    /// incluindo detalhes de preços, trechos, segmentos e passageiros.
    /// </returns>
    Task<DuffelOfferResponseDto> CreateOfferRequestAsync(
        DuffelOfferRequestDto request,
        CancellationToken cancellationToken);
}
