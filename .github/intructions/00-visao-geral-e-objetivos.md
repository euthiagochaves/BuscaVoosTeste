# 00 - Visão Geral e Objetivos

## Nome da solução

A solution deverá se chamar **BuscaVoosTeste.sln**.

## Objetivo do projeto

Criar uma **POC (Proof of Concept)** em **.NET 10 + C# 14**, com **Clean Architecture**, para:

- Realizar **busca de voos** usando **exclusivamente a API da Duffel**;
- Expor uma **API HTTP** para busca de voos (estilo Decolar.com, apenas listagem);
- Estar preparada para integração futura com **IA / GitHub Copilot / MCP Server**, permitindo que a IA faça buscas de voos em linguagem natural.

## Escopo desta POC

### O que o projeto DEVE fazer

- Receber parâmetros básicos de busca:
  - Origem (código IATA);
  - Destino (código IATA);
  - Data de ida;
  - Data de volta (opcional);
  - Quantidade de passageiros;
  - Classe de cabine (opcional).
- Invocar **somente a Duffel API** para buscar ofertas de voo;
- Converter o response da Duffel em um **modelo de domínio próprio** (`FlightOffer`, etc.);
- Retornar os resultados via endpoint HTTP claro, tipado, documentado via Swagger;
- Implementar **Clean Architecture** com as camadas/projetos:
  - `BuscaVoosTeste.Domain`;
  - `BuscaVoosTeste.Application`;
  - `BuscaVoosTeste.Infrastructure`;
  - `BuscaVoosTeste.Api`;
  - `BuscaVoosTeste.McpServer` (IA / MCP).

### O que o projeto NÃO fará (por enquanto)

- Não haverá:
  - Compra de voo;
  - Reserva/booking;
  - Emissão de bilhete;
  - Pagamento;
  - Login ou autenticação de usuário final;
  - Persistência em banco de dados;
  - Integração com outras APIs de voos além da Duffel;
  - Projetos/camadas de testes (unitários ou integração) nesta POC.

## Objetivo para IA (futuro)

- Permitir que um **MCP Server** (projeto `BuscaVoosTeste.McpServer`) utilize esta POC para:
  - Interpretar intenções em linguagem natural (ex.: “quero voos de GRU para EZE amanhã de manhã”);
  - Converter essas intenções em chamadas estruturadas ao caso de uso `BuscarVoosUseCase`;
  - Retornar resultados em JSON pronto para ser consumido pela IA, front-end ou qualquer outro consumidor.

---
