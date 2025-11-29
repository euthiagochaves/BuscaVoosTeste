using BuscaVoosTeste.Application;
using BuscaVoosTeste.Infrastructure;

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

app.Run();
