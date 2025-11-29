# 20 - Regras de Negócio da Busca de Voos

## Parâmetros de entrada obrigatórios

1. **Origem**  
   - Código IATA de 3 letras (ex.: `GRU`, `BHZ`, `GIG`);
   - Obrigatório.

2. **Destino**  
   - Código IATA de 3 letras;
   - Obrigatório.

3. **Data de ida**  
   - Data no formato ISO (`yyyy-MM-dd`) ou `DateTime`;
   - Obrigatório.

4. **Quantidade de passageiros**  
   - Inteiro >= 1;
   - Se não informado, valor padrão = 1.

## Parâmetros de entrada opcionais

5. **Data de volta**  
   - Opcional;
   - Se informada, considerar busca de ida e volta (round-trip).

6. **Classe de cabine**  
   - Opcional;
   - Valores possíveis (internamente normalizados):
     - `Economy`;
     - `PremiumEconomy`;
     - `Business`;
     - `First`.

7. **Filtros adicionais (futuro, não obrigatório na POC)**  
   - Número máximo de escalas;
   - Companhia aérea preferida, etc.
   - A POC deve estar preparada no modelo de domínio para suportar futuras extensões.

## Regras de negócio de validação

- Origem e destino não podem ser nulos ou vazios;
- Origem e destino não podem ser iguais;
- Quantidade de passageiros deve ser >= 1;
- Em ambiente real, a data de ida deve ser maior ou igual à data atual (na POC pode ser flexibilizado, mas a regra deve estar documentada aqui).

## Regras relacionadas à Duffel

- O caso de uso **não** conhece a Duffel diretamente;
- Apenas a implementação de `IFlightSearchProvider` em Infraestrutura sabe:
  - como montar o request da Duffel;
  - como interpretar o response da Duffel.

## Regras de tradução para o domínio

- Toda resposta da Duffel deve ser convertida para o modelo de domínio `FlightOffer`, contendo pelo menos:
  - `Id` (GUID interno da POC);
  - `OrigemIata`;
  - `DestinoIata`;
  - `Partida`;
  - `Chegada`;
  - `DuracaoTotal` (`Duration`);
  - `PrecoTotal` (`Money`);
  - `CompanhiaAereaCodigo`;
  - `CompanhiaAereaNome` (quando disponível);
  - `Cabine`;
  - `Segmentos` (lista de `FlightSegment`).

- Caso não haja ofertas, o provider deve retornar uma lista vazia, **não** nula.

## Regras de erro

- Erros de autenticação/credencial inválida na Duffel (HTTP 401/403) devem ser logados e mapeados para um erro genérico de infraestrutura para a API;
- Erros de validação de parâmetros (400 Duffel) podem ser tratados como erro 400 na API, quando fizer sentido;
- Erros de rede (timeout, DNS, etc.) devem ser tratados como erro 502/503/500 na API, conforme política que você escolher.
