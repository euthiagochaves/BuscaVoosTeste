namespace BuscaVoosTeste.Api.Dtos;

/// <summary>
/// DTO que representa uma resposta de erro padronizada da API.
/// Este contrato é utilizado para todas as respostas de erro, garantindo
/// consistência e previsibilidade para os consumidores da API.
/// </summary>
/// <remarks>
/// O formato de erro segue o padrão definido nas convenções do projeto,
/// com todos os campos e mensagens em português do Brasil.
/// </remarks>
public sealed class RespostaErro
{
    /// <summary>
    /// Código identificador do tipo de erro (por exemplo: "VALIDACAO", "NAO_ENCONTRADO", "ERRO_INTERNO").
    /// </summary>
    public string Codigo { get; init; } = default!;

    /// <summary>
    /// Mensagem amigável descrevendo o erro ocorrido.
    /// </summary>
    public string Mensagem { get; init; } = default!;

    /// <summary>
    /// Detalhes adicionais sobre o erro (opcional).
    /// Pode conter informações específicas sobre campos inválidos ou contexto do erro.
    /// </summary>
    public string? Detalhes { get; init; }

    /// <summary>
    /// Identificador único da requisição para rastreamento (correlação de logs).
    /// </summary>
    public string? IdentificadorRequisicao { get; init; }

    /// <summary>
    /// Data e hora em que o erro ocorreu (formato ISO 8601).
    /// </summary>
    public DateTime DataHora { get; init; }

    /// <summary>
    /// Cria uma nova instância de resposta de erro.
    /// </summary>
    /// <param name="codigo">Código identificador do tipo de erro.</param>
    /// <param name="mensagem">Mensagem amigável descrevendo o erro.</param>
    /// <param name="detalhes">Detalhes adicionais sobre o erro (opcional).</param>
    /// <param name="identificadorRequisicao">Identificador único da requisição (opcional).</param>
    /// <returns>Uma nova instância de <see cref="RespostaErro"/>.</returns>
    public static RespostaErro Criar(
        string codigo,
        string mensagem,
        string? detalhes = null,
        string? identificadorRequisicao = null)
    {
        return new RespostaErro
        {
            Codigo = codigo,
            Mensagem = mensagem,
            Detalhes = detalhes,
            IdentificadorRequisicao = identificadorRequisicao,
            DataHora = DateTime.UtcNow
        };
    }
}
