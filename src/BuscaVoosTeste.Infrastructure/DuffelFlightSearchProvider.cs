using BuscaVoosTeste.Application.Providers;
using BuscaVoosTeste.Application.UseCases.BuscarVoos;
using BuscaVoosTeste.Domain.Entities;

namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// Implementação concreta do provedor de busca de voos utilizando a API da Duffel.
/// Responsável por orquestrar a comunicação com a API externa e mapear dados
/// entre as camadas de aplicação e infraestrutura.
/// </summary>
/// <remarks>
/// Esta classe implementa a interface <see cref="IFlightSearchProvider"/> definida na camada
/// de aplicação, permitindo que os casos de uso consumam a busca de voos sem conhecer
/// detalhes da integração com a Duffel.
/// 
/// O fluxo de execução é:
/// 1. Receber o DTO de entrada (BuscarVoosInput) da camada de aplicação;
/// 2. Mapear para o formato de requisição da Duffel (DuffelOfferRequestDto);
/// 3. Chamar a API da Duffel via cliente HTTP encapsulado (IDuffelHttpClient);
/// 4. Mapear a resposta da Duffel para entidades de domínio (FlightOffer);
/// 5. Retornar as ofertas de voo para a camada de aplicação.
/// </remarks>
public sealed class DuffelFlightSearchProvider : IFlightSearchProvider
{
    private readonly IDuffelHttpClient _clienteHttpDuffel;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DuffelFlightSearchProvider"/>.
    /// </summary>
    /// <param name="clienteHttpDuffel">
    /// Cliente HTTP encapsulado para comunicação com a API da Duffel.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o cliente HTTP da Duffel é nulo.
    /// </exception>
    public DuffelFlightSearchProvider(IDuffelHttpClient clienteHttpDuffel)
    {
        _clienteHttpDuffel = clienteHttpDuffel ?? throw new ArgumentNullException(nameof(clienteHttpDuffel));
    }

    /// <inheritdoc />
    /// <summary>
    /// Realiza a busca de ofertas de voos na API da Duffel com base nos critérios informados.
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
    /// <exception cref="ArgumentNullException">
    /// Lançada quando o parâmetro de entrada é nulo.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Lançada quando ocorre um erro na comunicação com a API da Duffel.
    /// </exception>
    /// <remarks>
    /// Este método executa as seguintes etapas:
    /// 1. Mapeia os parâmetros de busca (BuscarVoosInput) para o formato da Duffel;
    /// 2. Envia a requisição para a API da Duffel via cliente HTTP;
    /// 3. Mapeia a resposta da Duffel para entidades de domínio (FlightOffer);
    /// 4. Retorna a lista de ofertas (nunca nula, pode ser vazia).
    /// </remarks>
    public async Task<IReadOnlyCollection<FlightOffer>> SearchAsync(
        BuscarVoosInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // 1. Mapear parâmetros de entrada para o formato da Duffel
        var requisicaoDuffel = DuffelRequestMapper.MapearParaDuffelOfferRequest(input);

        // 2. Chamar a API da Duffel via cliente HTTP encapsulado
        var respostaDuffel = await _clienteHttpDuffel.CreateOfferRequestAsync(requisicaoDuffel, cancellationToken);

        // 3. Mapear a resposta da Duffel para entidades de domínio
        var ofertas = DuffelResponseMapper.MapearParaDominio(respostaDuffel);

        // 4. Retornar lista de ofertas (nunca nula)
        return ofertas;
    }
}
