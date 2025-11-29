using BuscaVoosTeste.Application;
using BuscaVoosTeste.Application.UseCases.BuscarVoos;
using BuscaVoosTeste.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// Configuração de serviços
// =============================================================================

// Registro dos serviços da camada Application (casos de uso)
builder.Services.AddApplication();

// Registro dos serviços da camada Infrastructure (integração com Duffel)
builder.Services.AddInfrastructure(builder.Configuration);

// Configuração do OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// =============================================================================
// Configuração do pipeline HTTP
// =============================================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// =============================================================================
// Endpoint de busca de voos
// =============================================================================

app.MapGet("/api/voos/busca", async (
    [FromServices] IBuscarVoosUseCase useCase,
    [FromQuery] string origem,
    [FromQuery] string destino,
    [FromQuery] DateTime dataIda,
    [FromQuery] DateTime? dataVolta,
    [FromQuery] int passageiros,
    [FromQuery] string? cabine,
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
});

app.Run();
