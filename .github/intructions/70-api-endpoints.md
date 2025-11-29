# 70 - API HTTP (BuscaVoosTeste.Api)

## Endpoint principal de busca de voos

### Rota

```http
GET /api/voos/busca
```

### Parâmetros de query

- `origem` (obrigatório; IATA);
- `destino` (obrigatório; IATA);
- `dataIda` (obrigatório; `yyyy-MM-dd`);
- `dataVolta` (opcional; `yyyy-MM-dd`);
- `passageiros` (opcional; inteiro >= 1; padrão 1);
- `cabine` (opcional; `Economy`, `Business`, etc.).

### Exemplo de request

```http
GET /api/voos/busca?origem=GRU&destino=EZE&dataIda=2025-01-10&passageiros=1&cabine=Economy
```

### Exemplo de response (200 OK)

```json
[
  {
    "id": "4c76d3c2-7853-4b6f-9b8f-9ad4d3efb3ab",
    "origemIata": "GRU",
    "destinoIata": "EZE",
    "partida": "2025-01-10T08:55:00Z",
    "chegada": "2025-01-10T12:05:00Z",
    "duracaoTotal": "03:10:00",
    "precoTotal": {
      "valor": 1320.50,
      "moeda": "USD"
    },
    "companhiaAereaCodigo": "LA",
    "companhiaAereaNome": "LATAM Airlines",
    "cabine": "Economy",
    "segmentos": [
      {
        "origemIata": "GRU",
        "destinoIata": "EZE",
        "partida": "2025-01-10T08:55:00Z",
        "chegada": "2025-01-10T12:05:00Z",
        "numeroVoo": "LA8000",
        "companhiaAereaCodigo": "LA",
        "duracao": "03:10:00"
      }
    ]
  }
]
```

### Status Codes

- `200 OK` – busca realizada com sucesso (lista pode estar vazia);
- `400 Bad Request` – parâmetros inválidos (ex.: origem/destino ausentes ou inválidos);
- `500 Internal Server Error` – erro inesperado na comunicação com Duffel ou na lógica interna.

### Implementação (conceito minimal API)

```csharp
app.MapGet("/api/voos/busca", async (
    [FromServices] IBuscarVoosUseCase useCase,
    [FromQuery] string origem,
    [FromQuery] string destino,
    [FromQuery] DateTime dataIda,
    [FromQuery] DateTime? dataVolta,
    [FromQuery] int passageiros,
    [FromQuery] string? cabine) =>
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

    var offers = await useCase.ExecuteAsync(input);
    return Results.Ok(offers);
});
```

### Swagger

- Configurar Swagger para exibir:
  - Descrição da rota `/api/voos/busca`;
  - Parâmetros e exemplos;
  - Modelo de resposta.
