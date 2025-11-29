# 80 - Integração MCP / IA (BuscaVoosTeste.McpServer)

## Objetivo

Expor um endpoint MCP (Tool) para que IA (como GitHub Copilot) possa buscar voos usando a lógica da POC.

## Projeto BuscaVoosTeste.McpServer

- Deve referenciar:
  - `BuscaVoosTeste.Application`;
  - e, se necessário, `BuscaVoosTeste.Infrastructure`.

## Tool principal: buscar_voos

### Nome

```text
buscar_voos
```

### Input JSON esperado

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

### Output

- Lista de `FlightOffer` em JSON, conforme retornado pelo use case.

### Esqueleto conceitual

```csharp
public sealed class BuscarVoosTool : ITool
{
    private readonly IBuscarVoosUseCase _useCase;

    public string Name => "buscar_voos";

    public async Task<object> ExecuteAsync(
        dynamic input,
        CancellationToken cancellationToken = default)
    {
        var dto = new BuscarVoosInput
        {
            OrigemIata = (string)input.origem,
            DestinoIata = (string)input.destino,
            DataIda = DateTime.Parse((string)input.data_ida),
            DataVolta = input.data_volta != null
                ? DateTime.Parse((string)input.data_volta)
                : null,
            Passageiros = input.passageiros != null
                ? (int)input.passageiros
                : 1,
            Cabine = input.cabine
        };

        var result = await _useCase.ExecuteAsync(dto, cancellationToken);
        return result; // MCP host faz a serialização em JSON
    }
}
```

## Exemplos de prompts para IA

- “Buscar voos de GRU para EZE no dia 10 de janeiro, 1 passageiro em classe econômica.”
- “Quero opções de voos de São Paulo para Buenos Aires na próxima sexta, ida e volta, 2 passageiros.”

A IA deve:

1. Interpretar o comando em linguagem natural;
2. Montar o JSON de input esperado por `buscar_voos`;
3. Invocar a Tool `buscar_voos` via MCP;
4. Exibir o JSON retornado para o usuário ou usar em outra ação.
