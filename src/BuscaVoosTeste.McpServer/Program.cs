// Ponto de entrada do servidor MCP para a solução BuscaVoosTeste.
// Este host expõe o protocolo MCP (Model Context Protocol) via STDIO,
// permitindo que clientes como o GitHub Copilot se conectem e utilizem
// os casos de uso da aplicação.
// Ver '80-mcp-integration.md' para detalhes do protocolo MCP.

using BuscaVoosTeste.Application;
using BuscaVoosTeste.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Configuração do host MCP.
// O host é configurado para comunicação via STDIO, conforme padrão esperado
// pelo GitHub Copilot e outros clientes MCP compatíveis.
// Ver '90-instrucoes-copilot-agent.md' para instruções de conexão do agente.
var builder = Host.CreateApplicationBuilder(args);

// Configura o logging com nível mínimo Information, conforme convenções do projeto.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Registro dos serviços da camada de aplicação (casos de uso).
// A camada Application contém a lógica de negócio da busca de voos.
builder.Services.AddApplication();

// Registro dos serviços da camada de infraestrutura (integração com Duffel).
// A camada Infrastructure implementa a comunicação HTTP com a API da Duffel.
builder.Services.AddInfrastructure(builder.Configuration);

// Configuração do servidor MCP via STDIO.
// O servidor MCP ficará disponível para receber conexões de clientes MCP,
// como o GitHub Copilot, através do protocolo de comunicação padrão.
builder.Services.AddMcpServer()
    .WithStdioServerTransport();

var host = builder.Build();

// Inicia o servidor MCP e mantém o processo em execução.
// O servidor permanecerá ativo enquanto houver clientes conectados
// ou até receber sinal de encerramento.
await host.RunAsync();
