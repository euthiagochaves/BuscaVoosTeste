using BuscaVoosTeste.Application.UseCases.BuscarVoos;
using Microsoft.Extensions.DependencyInjection;

namespace BuscaVoosTeste.Application;

/// <summary>
/// Extensões para registro de serviços da camada de aplicação no container de injeção de dependência.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços da camada de aplicação ao container de injeção de dependência.
    /// </summary>
    /// <param name="services">Coleção de serviços do container de DI.</param>
    /// <returns>A coleção de serviços atualizada.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBuscarVoosUseCase, BuscarVoosUseCase>();

        return services;
    }
}
