using System.Net;
using System.Text.Json;
using BuscaVoosTeste.Api.Dtos;

namespace BuscaVoosTeste.Api.Middlewares;

/// <summary>
/// Middleware centralizado para tratamento de exceções na pipeline HTTP.
/// Captura todas as exceções não tratadas e as converte em respostas HTTP padronizadas.
/// </summary>
/// <remarks>
/// Este middleware deve ser registrado no início da pipeline HTTP para garantir
/// que todas as exceções sejam capturadas e tratadas de forma consistente.
/// 
/// Mapeamento de exceções para códigos HTTP:
/// - ArgumentNullException → HTTP 400 (Bad Request) - Parâmetro obrigatório não informado
/// - ArgumentException → HTTP 400 (Bad Request) - Parâmetros de entrada inválidos
/// - KeyNotFoundException → HTTP 404 (Not Found) - Recurso não encontrado
/// - InvalidOperationException → HTTP 409 (Conflict) - Conflito de estado
/// - HttpRequestException → HTTP 502 (Bad Gateway) - Erro de comunicação externa
/// - TaskCanceledException (cancelada pelo cliente) → HTTP 400 (Bad Request)
/// - TaskCanceledException (timeout) → HTTP 504 (Gateway Timeout)
/// - Outras exceções → HTTP 500 (Internal Server Error) - Erro interno genérico
/// </remarks>
public class MiddlewareTratamentoExcecoes
{
    private readonly RequestDelegate _proximoMiddleware;
    private readonly ILogger<MiddlewareTratamentoExcecoes> _logger;

    // Códigos de erro padronizados
    private const string CodigoErroValidacao = "VALIDACAO";
    private const string CodigoErroNaoEncontrado = "NAO_ENCONTRADO";
    private const string CodigoErroConflito = "CONFLITO";
    private const string CodigoErroInterno = "ERRO_INTERNO";

    // Mensagens padronizadas
    private const string MensagemErroInterno = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.";

    /// <summary>
    /// Inicializa uma nova instância do middleware de tratamento de exceções.
    /// </summary>
    /// <param name="proximoMiddleware">Delegado para o próximo middleware na pipeline.</param>
    /// <param name="logger">Logger para registro de erros.</param>
    public MiddlewareTratamentoExcecoes(
        RequestDelegate proximoMiddleware,
        ILogger<MiddlewareTratamentoExcecoes> logger)
    {
        _proximoMiddleware = proximoMiddleware ?? throw new ArgumentNullException(nameof(proximoMiddleware));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processa a requisição HTTP e trata exceções que possam ocorrer.
    /// </summary>
    /// <param name="contexto">Contexto HTTP da requisição.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximoMiddleware(contexto);
        }
        catch (Exception excecao)
        {
            await TratarExcecaoAsync(contexto, excecao);
        }
    }

    /// <summary>
    /// Trata a exceção capturada e gera uma resposta HTTP padronizada.
    /// </summary>
    /// <param name="contexto">Contexto HTTP da requisição.</param>
    /// <param name="excecao">Exceção capturada.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    private async Task TratarExcecaoAsync(HttpContext contexto, Exception excecao)
    {
        var identificadorRequisicao = contexto.TraceIdentifier;

        var (codigoStatus, respostaErro) = MapearExcecaoParaResposta(excecao, identificadorRequisicao);

        RegistrarErro(excecao, codigoStatus, identificadorRequisicao);

        contexto.Response.ContentType = "application/json; charset=utf-8";
        contexto.Response.StatusCode = (int)codigoStatus;

        var opcoesJson = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var jsonResposta = JsonSerializer.Serialize(respostaErro, opcoesJson);
        await contexto.Response.WriteAsync(jsonResposta);
    }

    /// <summary>
    /// Mapeia a exceção para o código de status HTTP e resposta de erro apropriados.
    /// </summary>
    /// <param name="excecao">Exceção a ser mapeada.</param>
    /// <param name="identificadorRequisicao">Identificador único da requisição.</param>
    /// <returns>Tupla contendo o código de status HTTP e a resposta de erro.</returns>
    private static (HttpStatusCode CodigoStatus, RespostaErro Resposta) MapearExcecaoParaResposta(
        Exception excecao,
        string identificadorRequisicao)
    {
        return excecao switch
        {
            ArgumentNullException argumentNullException => (
                HttpStatusCode.BadRequest,
                RespostaErro.Criar(
                    CodigoErroValidacao,
                    "Parâmetro obrigatório não informado.",
                    argumentNullException.Message,
                    identificadorRequisicao)),

            ArgumentException argumentException => (
                HttpStatusCode.BadRequest,
                RespostaErro.Criar(
                    CodigoErroValidacao,
                    "Parâmetros de entrada inválidos.",
                    argumentException.Message,
                    identificadorRequisicao)),

            KeyNotFoundException keyNotFoundException => (
                HttpStatusCode.NotFound,
                RespostaErro.Criar(
                    CodigoErroNaoEncontrado,
                    "Recurso não encontrado.",
                    keyNotFoundException.Message,
                    identificadorRequisicao)),

            InvalidOperationException invalidOperationException => (
                HttpStatusCode.Conflict,
                RespostaErro.Criar(
                    CodigoErroConflito,
                    "Operação não permitida no estado atual.",
                    invalidOperationException.Message,
                    identificadorRequisicao)),

            // Nota: A mensagem original do HttpRequestException não é incluída na resposta
            // por razões de segurança, evitando vazamento de detalhes da infraestrutura.
            HttpRequestException => (
                HttpStatusCode.BadGateway,
                RespostaErro.Criar(
                    CodigoErroInterno,
                    "Erro na comunicação com serviço externo.",
                    null,
                    identificadorRequisicao)),

            TaskCanceledException taskCanceledException when taskCanceledException.CancellationToken.IsCancellationRequested => (
                HttpStatusCode.BadRequest,
                RespostaErro.Criar(
                    CodigoErroValidacao,
                    "Requisição cancelada pelo cliente.",
                    null,
                    identificadorRequisicao)),

            TaskCanceledException => (
                HttpStatusCode.GatewayTimeout,
                RespostaErro.Criar(
                    CodigoErroInterno,
                    "Tempo limite de comunicação com serviço externo excedido.",
                    null,
                    identificadorRequisicao)),

            _ => (
                HttpStatusCode.InternalServerError,
                RespostaErro.Criar(
                    CodigoErroInterno,
                    MensagemErroInterno,
                    null,
                    identificadorRequisicao))
        };
    }

    /// <summary>
    /// Registra o erro no sistema de logging.
    /// </summary>
    /// <param name="excecao">Exceção ocorrida.</param>
    /// <param name="codigoStatus">Código de status HTTP resultante.</param>
    /// <param name="identificadorRequisicao">Identificador único da requisição.</param>
    private void RegistrarErro(Exception excecao, HttpStatusCode codigoStatus, string identificadorRequisicao)
    {
        var nivelLog = codigoStatus switch
        {
            HttpStatusCode.BadRequest => LogLevel.Warning,
            HttpStatusCode.NotFound => LogLevel.Warning,
            HttpStatusCode.Conflict => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(
            nivelLog,
            excecao,
            "Erro ao processar requisição. IdentificadorRequisicao: {IdentificadorRequisicao}, " +
            "TipoExcecao: {TipoExcecao}, CodigoStatus: {CodigoStatus}, Mensagem: {Mensagem}",
            identificadorRequisicao,
            excecao.GetType().Name,
            (int)codigoStatus,
            excecao.Message);
    }
}

/// <summary>
/// Extensões para facilitar o registro do middleware de tratamento de exceções.
/// </summary>
public static class MiddlewareTratamentoExcecoesExtensions
{
    /// <summary>
    /// Adiciona o middleware de tratamento de exceções à pipeline HTTP.
    /// </summary>
    /// <param name="app">Builder da aplicação.</param>
    /// <returns>Builder da aplicação para encadeamento.</returns>
    public static IApplicationBuilder UseTratamentoExcecoes(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MiddlewareTratamentoExcecoes>();
    }
}
