using BuscaVoosTeste.Api.Dtos;
using BuscaVoosTeste.Api.Middlewares;
using BuscaVoosTeste.Application;
using BuscaVoosTeste.Application.UseCases.BuscarVoos;
using BuscaVoosTeste.Domain.Entities;
using BuscaVoosTeste.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// Configuração de serviços
// =============================================================================

// Registro dos serviços da camada Application (casos de uso)
builder.Services.AddApplication();

// Registro dos serviços da camada Infrastructure (integração com Duffel)
builder.Services.AddInfrastructure(builder.Configuration);

// Configuração do OpenAPI/Swagger
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "BuscaVoosTeste API",
            Version = "v1",
            Description = "API para busca de voos utilizando a integração com Duffel. " +
                          "Esta POC permite pesquisar ofertas de voos a partir de parâmetros " +
                          "como origem, destino, datas, quantidade de passageiros e classe de cabine."
        };
        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        if (context.Description.RelativePath == "api/voos/busca")
        {
            operation.Summary = "Busca ofertas de voos";
            operation.Description = "Busca ofertas de voos disponíveis com base nos parâmetros informados. " +
                                    "Utiliza a API da Duffel para obter as ofertas de voos e retorna uma lista " +
                                    "de ofertas com informações de preço, duração, companhia aérea e segmentos do itinerário.";

            if (operation.Parameters is not null)
            {
                foreach (var param in operation.Parameters)
                {
                    param.Description = param.Name switch
                    {
                        "origem" => "Código IATA do aeroporto ou cidade de origem (3 letras). Obrigatório.",
                        "destino" => "Código IATA do aeroporto ou cidade de destino (3 letras). Obrigatório.",
                        "dataIda" => "Data de ida da viagem no formato yyyy-MM-dd. Obrigatório.",
                        "dataVolta" => "Data de volta da viagem no formato yyyy-MM-dd. Opcional, se informada considera busca de ida e volta.",
                        "passageiros" => "Quantidade de passageiros. Valor padrão é 1 se não informado ou menor que 1.",
                        "cabine" => "Classe de cabine desejada (Economy, PremiumEconomy, Business, First). Opcional.",
                        _ => param.Description
                    };
                }
            }
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// =============================================================================
// Configuração do pipeline HTTP
// =============================================================================

// Middleware de tratamento de exceções (deve ser registrado primeiro na pipeline)
app.UseTratamentoExcecoes();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "BuscaVoosTeste API v1");
    });
}

app.UseHttpsRedirection();

// =============================================================================
// Endpoint de busca de voos
// =============================================================================

app.MapGet("/api/voos/busca", async (
    [FromServices] IBuscarVoosUseCase useCase,
    [FromQuery(Name = "origem")] string origem,
    [FromQuery(Name = "destino")] string destino,
    [FromQuery(Name = "dataIda")] DateTime dataIda,
    [FromQuery(Name = "dataVolta")] DateTime? dataVolta,
    [FromQuery(Name = "passageiros")] int passageiros,
    [FromQuery(Name = "cabine")] string? cabine,
    CancellationToken cancellationToken) =>
{
    var input = new BuscarVoosInput
    {
        OrigemIata = origem,
        DestinoIata = destino,
        DataIda = dataIda,
        DataVolta = dataVolta,
        Passageiros = passageiros <= 0 ? 1 : passageiros,
        Cabine = cabine
    };

    var ofertas = await useCase.ExecuteAsync(input, cancellationToken);
    return Results.Ok(ofertas);
})
.WithName("BuscarVoos")
.Produces<IReadOnlyCollection<FlightOffer>>(StatusCodes.Status200OK, "application/json")
.Produces<RespostaErro>(StatusCodes.Status400BadRequest, "application/json")
.Produces<RespostaErro>(StatusCodes.Status500InternalServerError, "application/json");

app.Run();
