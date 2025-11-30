# BuscaVoosTeste.sln

## Visão geral da solução

**BuscaVoosTeste** é uma POC (Proof of Concept) em **.NET 10 + C# 14** para realizar **busca de voos** utilizando exclusivamente a **API da Duffel**.

O projeto segue os princípios de **Clean Architecture** e está preparado para integração com **IA / GitHub Copilot / MCP Server**, permitindo buscas de voos em linguagem natural.

### Objetivo

- Realizar **busca de voos** usando **exclusivamente a API da Duffel**;
- Expor uma **API HTTP** para busca de voos (estilo Decolar.com, apenas listagem);
- Integrar com **IA / GitHub Copilot / MCP Server**, permitindo que a IA faça buscas de voos em linguagem natural.

Para mais detalhes sobre o objetivo e escopo do projeto, consulte o arquivo [.github/intructions/00-visao-geral-e-objetivos.md](.github/intructions/00-visao-geral-e-objetivos.md).

### Projetos da solução

A solução **BuscaVoosTeste.sln** utiliza Clean Architecture com os seguintes projetos:

| Projeto | Descrição | Dependências |
|---------|-----------|--------------|
| **BuscaVoosTeste.Domain** | Contém as regras de negócio puras e modelos de domínio. | Nenhuma |
| **BuscaVoosTeste.Application** | Implementa os casos de uso da aplicação (ex.: `BuscarVoosUseCase`). | Domain |
| **BuscaVoosTeste.Infrastructure** | Implementa a integração com a API da Duffel (provider de voos). | Domain, Application |
| **BuscaVoosTeste.Api** | Expõe a API HTTP pública para busca de voos. | Application, Infrastructure |
| **BuscaVoosTeste.McpServer** | Servidor MCP para integração com IA. | Application, Infrastructure |

Para detalhes sobre a arquitetura, camadas e regras de dependência, consulte o arquivo [.github/intructions/10-arquitetura-clean.md](.github/intructions/10-arquitetura-clean.md).

### Estrutura de pastas

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

### Referências às instructions

Os arquivos de instructions com as definições detalhadas do projeto estão disponíveis em `.github/intructions/`:

- [00-visao-geral-e-objetivos.md](.github/intructions/00-visao-geral-e-objetivos.md) - Visão geral e objetivos do projeto
- [10-arquitetura-clean.md](.github/intructions/10-arquitetura-clean.md) - Arquitetura Clean e estrutura da solution
- [15-convencoes-de-codigo.md](.github/intructions/15-convencoes-de-codigo.md) - Convenções de código
- [30-dependencias-e-configuracoes.md](.github/intructions/30-dependencias-e-configuracoes.md) - Dependências e configurações
- [70-api-endpoints.md](.github/intructions/70-api-endpoints.md) - Endpoints da API HTTP
- [80-mcp-integration.md](.github/intructions/80-mcp-integration.md) - Integração MCP / IA

---

## Pré-requisitos

Antes de executar a solução, certifique-se de ter os seguintes pré-requisitos instalados e configurados:

### SDK e Runtime

- **.NET 10 SDK** ou superior
- **C# 14**

### Ferramentas necessárias

- **Git** para clonar o repositório
- **Editor/IDE** de sua preferência (ex.: Visual Studio 2022, VS Code, Rider)

### Configurações externas

- **Duffel API Access Token**: É necessário obter um token de acesso da Duffel para executar a aplicação. A Duffel é a única API de voos utilizada neste projeto.
  - Para obter o token, consulte a documentação oficial: [https://duffel.com/docs/api/overview/welcome](https://duffel.com/docs/api/overview/welcome)

Para mais detalhes sobre dependências e configurações, consulte o arquivo [.github/intructions/30-dependencias-e-configuracoes.md](.github/intructions/30-dependencias-e-configuracoes.md).

---

## Build da solução

### Restaurar dependências

Para restaurar todas as dependências dos projetos, execute na raiz do repositório:

```bash
dotnet restore
```

### Compilar a solução

Para compilar todos os projetos da solução:

```bash
dotnet build
```

O comando acima irá compilar todos os projetos respeitando as dependências:
1. `BuscaVoosTeste.Domain`
2. `BuscaVoosTeste.Application`
3. `BuscaVoosTeste.Infrastructure`
4. `BuscaVoosTeste.Api`
5. `BuscaVoosTeste.McpServer`

### Build de projeto específico

Se necessário, você pode compilar um projeto específico:

```bash
# Compilar apenas a API
dotnet build src/BuscaVoosTeste.Api

# Compilar apenas o McpServer
dotnet build src/BuscaVoosTeste.McpServer
```

---

## Executando a API (BuscaVoosTeste.Api)

A API HTTP é o projeto **BuscaVoosTeste.Api** e expõe o endpoint de busca de voos para consumo via HTTP.

### Configuração prévia

Antes de executar a API, configure o token da Duffel conforme descrito na seção [Configurações e variáveis de ambiente](#configurações-e-variáveis-de-ambiente).

### Executar a API

Para executar a API em ambiente de desenvolvimento local:

```bash
dotnet run --project src/BuscaVoosTeste.Api
```

Por padrão, a API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Swagger / OpenAPI

A documentação interativa da API está disponível via Swagger em:

```
https://localhost:5001/swagger
```

### Endpoint disponível

O endpoint principal de busca de voos é:

```http
GET /api/voos/busca
```

**Parâmetros de query:**

| Parâmetro | Obrigatório | Descrição | Formato |
|-----------|-------------|-----------|---------|
| `origem` | Sim | Código IATA do aeroporto de origem | String (3 caracteres) |
| `destino` | Sim | Código IATA do aeroporto de destino | String (3 caracteres) |
| `dataIda` | Sim | Data de ida | `yyyy-MM-dd` |
| `dataVolta` | Não | Data de volta | `yyyy-MM-dd` |
| `passageiros` | Não | Quantidade de passageiros (padrão: 1) | Inteiro >= 1 |
| `cabine` | Não | Classe de cabine | `Economy`, `Business`, etc. |

**Exemplo de requisição:**

```http
GET /api/voos/busca?origem=GRU&destino=EZE&dataIda=2025-01-10&passageiros=1&cabine=Economy
```

Para documentação completa dos endpoints, status codes e exemplos de response, consulte o arquivo [.github/intructions/70-api-endpoints.md](.github/intructions/70-api-endpoints.md).

---

## Executando o host MCP (BuscaVoosTeste.McpServer)

O projeto **BuscaVoosTeste.McpServer** é o servidor MCP (Model Context Protocol) que permite a integração com IA, como o GitHub Copilot.

### Configuração prévia

Antes de executar o McpServer, configure o token da Duffel conforme descrito na seção [Configurações e variáveis de ambiente](#configurações-e-variáveis-de-ambiente).

### Relação com a API

O **BuscaVoosTeste.McpServer** é um projeto independente que utiliza diretamente o caso de uso `BuscarVoosUseCase` da camada Application. Ele **não depende da API HTTP estar em execução**, pois possui sua própria configuração de DI (injeção de dependência) e acesso direto ao provider da Duffel.

### Executar o McpServer

Para executar o host MCP em ambiente de desenvolvimento local:

```bash
dotnet run --project src/BuscaVoosTeste.McpServer
```

### Tool MCP disponível

O McpServer expõe a tool principal:

**Nome:** `buscar_voos`

**Input JSON esperado:**

```json
{
  "origem": "GRU",
  "destino": "EZE",
  "data_ida": "2025-01-10",
  "data_volta": null,
  "passageiros": 1,
  "cabine": "Economy"
}
```

**Output:** Lista de `FlightOffer` em JSON, conforme retornado pelo caso de uso.

### Validando a execução

Ao iniciar o McpServer, você deverá ver mensagens de log indicando que o servidor foi iniciado com sucesso. O servidor MCP utiliza comunicação via stdio (entrada/saída padrão) para interagir com o cliente MCP (ex.: GitHub Copilot).

Para detalhes completos sobre a integração MCP, estrutura da tool e exemplos de uso com IA, consulte o arquivo [.github/intructions/80-mcp-integration.md](.github/intructions/80-mcp-integration.md).

---

## Configurações e variáveis de ambiente

### Arquivo de configuração

A configuração da Duffel API é feita nos arquivos `appsettings.json` dos projetos `BuscaVoosTeste.Api` e `BuscaVoosTeste.McpServer`:

```json
{
  "Duffel": {
    "BaseUrl": "https://api.duffel.com",
    "AccessToken": "YOUR_DUFFEL_ACCESS_TOKEN_HERE"
  }
}
```

### Variáveis principais

| Variável | Descrição | Onde configurar |
|----------|-----------|-----------------|
| `Duffel:BaseUrl` | URL base da API da Duffel | `appsettings.json` |
| `Duffel:AccessToken` | Token de acesso da Duffel | `appsettings.Development.json`, User Secrets ou variável de ambiente |

### Boas práticas de segurança

- **Nunca** commitar o token de acesso diretamente no `appsettings.json`;
- Para desenvolvimento local, utilize `appsettings.Development.json` ou **User Secrets** do .NET:

```bash
dotnet user-secrets set "Duffel:AccessToken" "seu_token_aqui" --project src/BuscaVoosTeste.Api
dotnet user-secrets set "Duffel:AccessToken" "seu_token_aqui" --project src/BuscaVoosTeste.McpServer
```

- Em produção, utilize variáveis de ambiente, Azure Key Vault ou outro gerenciador de segredos.

Para detalhes completos sobre configurações e dependências, consulte os arquivos:
- [.github/intructions/30-dependencias-e-configuracoes.md](.github/intructions/30-dependencias-e-configuracoes.md)
- [.github/intructions/80-mcp-integration.md](.github/intructions/80-mcp-integration.md)

---

## Tecnologias utilizadas

- **.NET 10**
- **C# 14**
- **ASP.NET Core** (para a API HTTP)
- **Clean Architecture**
- **Duffel API** (provider exclusivo para busca de voos)
- **Swagger / Swashbuckle** (documentação da API)
- **HttpClientFactory** (consumo da Duffel API)
- **IOptions\<T\>** (configurações da Duffel)

---

## Licença

Este projeto é uma POC para fins de demonstração.
