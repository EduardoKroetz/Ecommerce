# 0004 - Checkout apenas via carrinho na v1 (sem compra avulsa, sem OrderService)

## Status
Aceito

## Contexto
A criação de pedido pode nascer de dois caminhos: checkout do carrinho ou compra avulsa (comprar um produto direto, sem carrinho). 
Também é preciso decidir onde mora a lógica de montar o pedido (calcular total, criar itens): no controller ou numa camada de serviço.

## Decisão
A v1 tem apenas checkout via carrinho. 
A lógica de criação do pedido fica no próprio controller, sem `OrderService`.

**Sem compra avulsa:** escopo de v1 — o fluxo tradicional basta e não há necessidade comprovada do caminho avulso.

**Sem OrderService:** com um único caminho de criação, extrair um serviço seria abstração preventiva — um tipo técnico a mais, usado num só lugar, sem a duplicação que justificaria sua existência.

## Gatilho de revisão
Quando a compra avulsa for implementada, haverá dois caminhos montando pedido. A regra de total e de criação não pode viver duplicada — **esse** é o momento de extrair o `OrderService`, não antes.

## Consequências

### Positivas
- Menos código e menos indireção na v1.
- Evita over-engineering: a camada de serviço só nasce quando houver duplicação real.

### Negativas
- A lógica de criação fica no controller; quando o segundo caminho chegar, haverá uma refatoração para extraí-la.
- Decisão de adiar exige disciplina de revisitar — se o gatilho passar despercebido, a lógica acaba duplicada entre controllers.