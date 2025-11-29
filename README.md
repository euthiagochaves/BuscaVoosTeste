# BuscaVoosTeste.sln

## Visão Geral

**BuscaVoosTeste** é uma POC (Proof of Concept) em **.NET 10 + C# 14** para realizar **busca de voos** utilizando exclusivamente a **API da Duffel**.

O projeto segue os princípios de **Clean Architecture** e está preparado para integração futura com **IA / GitHub Copilot / MCP Server**, permitindo buscas de voos em linguagem natural.

## Arquitetura

O projeto utiliza Clean Architecture com as seguintes camadas:

- **Domain**: Contém as regras de negócio puras e modelos de domínio. Não possui dependências externas.
- **Application**: Implementa os casos de uso da aplicação. Depende apenas do Domain.
- **Infrastructure**: Implementa a integração com a API da Duffel (provider de voos). Depende do Domain e Application.
- **Api**: Expõe a API HTTP pública para busca de voos. Depende de Application e Infrastructure.
- **McpServer**: Servidor MCP para integração com IA. Depende de Application e Infrastructure.

## Estrutura de Pastas

```text
/BuscaVoosTeste.sln
├── README.md
├── .gitignore
├── .editorconfig
└── src/
    ├── BuscaVoosTeste.Domain/
    ├── BuscaVoosTeste.Application/
    ├── BuscaVoosTeste.Infrastructure/
    ├── BuscaVoosTeste.Api/
    └── BuscaVoosTeste.McpServer/
```

## Tecnologias Utilizadas

- **.NET 10**
- **C# 14**
- **ASP.NET Core** (para a API HTTP)
- **Clean Architecture**
- **Duffel API** (provider exclusivo para busca de voos)

## Como Executar

1. Clone o repositório
2. Navegue até a pasta raiz do projeto
3. Execute o comando:

```bash
dotnet build
```

Para executar a API:

```bash
dotnet run --project src/BuscaVoosTeste.Api
```

## Licença

Este projeto é uma POC para fins de demonstração.
