using System.Net.Http.Headers;
using BuscaVoosTeste.Application.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// Extensões para registro de serviços da camada de infraestrutura no container de injeção de dependência.
/// </summary>
/// <remarks>
/// Esta classe centraliza o registro de todos os serviços de infraestrutura, incluindo:
/// - Configurações da Duffel (DuffelOptions);
/// - Cliente HTTP da Duffel (IDuffelHttpClient);
/// - Provedor de busca de voos da Duffel (IFlightSearchProvider).
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços da camada de infraestrutura ao container de injeção de dependência.
    /// </summary>
    /// <param name="services">Coleção de serviços do container de DI.</param>
    /// <param name="configuration">Configuração da aplicação para acesso às seções de configuração.</param>
    /// <returns>A coleção de serviços atualizada.</returns>
    /// <remarks>
    /// Este método registra:
    /// - DuffelOptions: configurações da API da Duffel (BaseUrl, AccessToken);
    /// - IDuffelHttpClient: cliente HTTP tipado para a Duffel com autenticação Bearer;
    /// - IFlightSearchProvider: provedor de busca de voos implementado pelo DuffelFlightSearchProvider.
    /// 
    /// As configurações são carregadas da seção "Duffel" do appsettings.json.
    /// </remarks>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registrar configurações da Duffel
        services.Configure<DuffelOptions>(configuration.GetSection("Duffel"));

        // Registrar cliente HTTP da Duffel com configuração de BaseAddress e Authorization
        services.AddHttpClient<IDuffelHttpClient, DuffelHttpClient>((serviceProvider, httpClient) =>
        {
            var opcoesDuffel = serviceProvider.GetRequiredService<IOptions<DuffelOptions>>().Value;

            httpClient.BaseAddress = new Uri(opcoesDuffel.BaseUrl);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", opcoesDuffel.AccessToken);
            httpClient.DefaultRequestHeaders.Add("Duffel-Version", "v2");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Registrar provedor de busca de voos da Duffel
        services.AddScoped<IFlightSearchProvider, DuffelFlightSearchProvider>();

        return services;
    }
}
