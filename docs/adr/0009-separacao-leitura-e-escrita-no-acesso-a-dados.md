# 0009 - Separação leitura/escrita no acesso a dados (CQRS leve)

## Status
Aceito

## Contexto
Os métodos de repositório precisam decidir o que recebem e o que retornam.
Leitura (exibir dados) e escrita (operar regra de negócio) têm necessidades
opostas: a escrita precisa da entidade com comportamento e invariantes; a
leitura precisa apenas de dados projetados na forma da tela, sem o peso de
carregar agregados completos.

A alternativa seria um modelo único (a entidade) servindo leitura e escrita —
o que leva ou a entidades carregadas gordas só para exibir, ou a projeções
que esbarram em `.Include` crescente a cada novo relacionamento.

## Decisão
Aplicar CQRS leve (sem buses, handlers ou bancos separados — apenas a
separação de modelos):

- **Escrita / operação com regra** → métodos retornam e recebem a **entidade**,
  passando pelo domínio. Ex: `GetByIdAsync → Product`, `UpdateAsync(Product)`.
- **Leitura para exibir** → métodos retornam **DTO projetado** via Expression,
  direto do banco, sem carregar a entidade nem usar `.Include`. Ex:
  `GetDetailByIdAsync → ProductDetailDto`, `SearchAsync(query) → PagedResult<...>`.

Os métodos seguem uma matriz de dois eixos — direção do dado (leitura/escrita)
× presença de regra de domínio:

| | Tem regra | Sem regra |
|---|---|---|
| **Escrita** | carrega entidade, opera no domínio | operação em lote no banco (bulk) |
| **Leitura** | consulta de validação | DTO projetado |

## Consequências

### Positivas
- Leitura performática: projeta só os campos da tela, sem `.Include` nem
  agregado gordo.
- Escrita protege a regra: o caminho de comando sempre passa pela entidade.
- Classificar um método novo é trivial — cai numa das quatro células.

### Negativas
- Dois caminhos para o "mesmo" dado (ex: `GetByIdAsync` entidade e
  `GetDetailByIdAsync` DTO). É separação por propósito, não duplicação — mas
  exige disciplina para não confundir qual usar.
- O lado de leitura não reusa a lógica de mapeamento do lado de escrita — as
  projeções (Expression) são mantidas à parte.

## Relacionadas
- ADR 0006 (mapeamento via Expression) — é o mecanismo que torna a projeção de
  leitura viável sem `.Include`.