namespace BuscaVoosTeste.Infrastructure;

/// <summary>
/// Representa as configurações de integração com a API da Duffel.
/// Esta classe é a representação tipada da seção "Duffel" no appsettings.json.
/// </summary>
public sealed class DuffelOptions
{
    /// <summary>
    /// URL base da API da Duffel (sandbox ou produção).
    /// Exemplo: "https://api.duffel.com"
    /// </summary>
    public string BaseUrl { get; init; } = default!;

    /// <summary>
    /// Token de acesso para autenticação na API da Duffel.
    /// Será usado no header HTTP: Authorization: Bearer {AccessToken}
    /// </summary>
    public string AccessToken { get; init; } = default!;
}
