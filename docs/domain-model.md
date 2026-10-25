# Modelo de domínio

Guia de **como modelar** o domínio: termos, agregados e onde cada regra mora.
Para **como escrever** o código, ver `CONVENTIONS.md`. Para **por que** cada decisão foi tomada, ver os ADRs.

Este documento é refinado conforme a refatoração avança.

---

## Glossário

| Termo | Significado |
|---|---|
| **User** | Identidade autenticada. `UserId` só aparece na borda (Identity, JWT, `ICurrentUserProvider`). |
| **Customer** | Papel de comprador. Todo usuário tem exatamente um. |
| **Seller** | Papel de vendedor. Opcional. |
| **Cart** | Carrinho de um Customer. Preço vivo: reflete o preço atual do produto. |
| **Purchase** (compra) | O que o Customer faz num checkout. Agrupa os pedidos gerados e concentra o pagamento. |
| **Order** (pedido) | A parte de uma compra que pertence a um único vendedor. Preço congelado. |

- Nunca usar `UserId` para representar um papel.
- Nunca usar o termo "Buyer". O papel de comprador é sempre Customer.

---

## Nível de DDD

DDD tático leve.

- **Adotado:** entidades ricas onde há invariante (ADR 0014), agregados, repositório por agregado e linguagem ubíqua (o glossário acima).
- **Fora por enquanto:** bounded contexts, serviços de domínio e eventos de domínio (previstos para quando houver mensageria).

---

## Agregados

### O que define um agregado

Três perguntas, nesta ordem:

1. **Existe uma regra entre esses objetos que nunca pode ser violada, nem por um instante?** Se sim, ficam no mesmo agregado.
2. **Eles nascem e morrem juntos?** Se o ciclo de vida é o mesmo (um `OrderItem` não existe sem o `Order`), ficam juntos.
3. **Algum ator altera uma parte de forma independente?** Se sim, são agregados separados (o vendedor envia o Order dele sem tocar na Purchase do comprador).

As perguntas são lidas em ordem. Quando apontam em sentidos diferentes, a terceira (ator independente) decide. Ex.: Purchase e Order nascem juntos, mas o Order é alterado pelo vendedor de forma independente, então são agregados separados.

### Regras

- Só a raiz do agregado tem repositório.
- Entre agregados, a referência é feita pelo id. Comandos não navegam de um agregado para outro.
- Uma transação altera um agregado. Exceções são listadas em [Exceções conhecidas](#exceções-conhecidas).
- Navegações entre agregados existem no modelo como `internal` e são usadas apenas nos métodos de consulta dos repositórios da Infra, incluindo as projeções (`Infra/Projections/`). Nem comandos da Application nem métodos de entidade navegam entre agregados.
- Navegações dentro de um agregado (`Order.Items`, `Cart.Items`) são públicas e podem ser usadas por comandos.

### Referências por navegação (exceções)

Referências entre agregados que continuam como navegação pública, em vez de `internal`.

| Referência | Motivo |
|---|---|
| `Product.Categories` (`ProductCategory`) | Vínculo muitos-para-muitos sem regra própria; o vínculo por id exigiria uma entidade de associação sem ganho. É pública e usada pelos métodos `AddCategory` e `RemoveCategory`: exceção às duas regras acima, por enquanto. |

### Mapa atual

| Agregado (raiz) | Contém | Invariante que justifica a fronteira |
|---|---|---|
| `Order` | `OrderItem` | Máquina de estados; total = soma dos itens |
| `Cart` | `CartItem` | Limite de quantidade por item; um item por produto |
| `Purchase` | — | Status de pagamento; na criação, pelo menos um pedido e um pedido por vendedor (ADR 0017) |
| `Product` | — | Estoque nunca negativo, garantido no banco (ADR 0014) |
| `Customer` | — | — |
| `Seller` | — | — |
| `ProductCategory` | — | — |

---

## Valores: congelado, mantido, derivado ou desnormalizado

| Pergunta | Tipo | Como funciona | Exemplo |
|---|---|---|---|
| O valor precisa refletir **um momento passado**, mesmo que a origem mude? | **Congelado** | Copiado na criação, nunca mais alterado | `OrderItem.UnitPrice` |
| É derivado **do mesmo agregado**? | **Mantido pela entidade** | Calculado na hora ou armazenado e atualizado pelos métodos da própria entidade | `Order.TotalAmount` |
| É derivado **de outros agregados**? | **Derivado na leitura** (padrão) | Calculado na projeção, não armazenado | — |
| Idem, mas com justificativa para armazenar | **Desnormalizado** | Armazenado e recalculado em pontos de escrita listados | `Purchase.TotalAmount` |

Regras para valores desnormalizados:
- Todo ponto de escrita que altera a origem precisa recalcular. A lista fica abaixo e deve ser atualizada a cada novo ponto.
- Só são seguros com concorrência otimista na raiz que os armazena.

### Desnormalizados atuais

| Valor | Calculado por | Pontos de escrita | Pendência |
|---|---|---|---|
| `Purchase.TotalAmount` (soma dos pedidos não cancelados) | Construtor da `Purchase` (checkout, também congela o `OriginalTotalAmount`) e `Purchase.RecalculateTotal(orders)` | Checkout, cancelamento de pedido, cancelamento de compra | Sem concorrência otimista na Purchase, cancelamentos simultâneos de pedidos da mesma compra podem gravar um total incorreto |

---

## Onde fica cada regra

**Quem muda de estado é dono da regra.**

| A regra... | Fica em | Exemplo |
|---|---|---|
| Envolve só o próprio agregado | Método da entidade | `order.Ship()`, `order.Cancel()` |
| Decide o estado de **um** agregado usando dados de outros | Método desse agregado, recebendo os dados por parâmetro. A Application carrega os dados pelo repositório, nunca por navegação. | `purchase.RecalculateTotal(orders)` |
| Coordena **vários** agregados ou precisa de infraestrutura (repositório, consulta, tempo) | Serviço de aplicação, que carrega, chama os métodos de cada agregado e salva | `OrderService.CancelAsync` |
| Depende de concorrência ou do estado atual no banco | Repositório, com operação atômica | `TryDecrementStockAsync` (ADR 0014) |

**Serviço de domínio:** não adotado. Gatilho: uma regra pura que não pertença a nenhum agregado e seja usada por mais de um serviço de aplicação.

---

## Exceções conhecidas

Operações que alteram mais de um agregado numa mesma transação. Aceitas no monolito; candidatas a eventos de domínio quando houver mensageria.

| Operação | Agregados alterados |
|---|---|
| Checkout | Purchase, Orders, estoque dos Products |
| Pagamento | Purchase, Orders |
| Cancelamento de pedido | Order, estoque dos Products, Purchase |
| Cancelamento de compra | Purchase, Orders, estoque dos Products |
