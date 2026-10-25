# 0017 - Relação entre Purchase e Order

## Status
Aceito

## Contexto
Purchase e Order são agregados separados ([0015](0015-ddd-tatico-leve-agregados-como-fronteira-de-consistencia.md)): o Order é alterado pelo vendedor de forma independente. Mas a Purchase só existe a partir dos seus pedidos, e os totais dela são derivados deles. O cálculo dos totais estava no service, e o construtor do Order exigia o id da Purchase, enquanto a Purchase precisaria dos Orders para se construir: um ciclo na criação.

## Opções consideradas
- **Purchase contendo os Orders:** o vendedor teria que carregar a compra inteira para enviar o próprio pedido.
- **Purchase criada sem pedidos e preenchida depois (`RegisterOrders`):** a compra existiria, e seria gravada, sem pedidos e sem total.
- **Total da compra derivado na leitura:** a regra se repetiria em cada projeção.
- **Orders criados primeiro, Purchase construída a partir deles.**

## Decisão
- Referência: o Order conhece a Purchase pelo id. A Purchase nunca guarda os Orders. A navegação `internal` `Purchase.Orders` ([0016](0016-navegacoes-entre-agregados-internal-e-projecoes-na-infra.md)) existe só para leitura na Infra: nunca é preenchida pela entidade nem usada por comandos.
- Derivação: a Purchase calcula seus valores a partir dos Orders recebidos por parâmetro.
- Criação: os Orders são criados primeiro; a Purchase é construída a partir deles e o vínculo é feito depois (`order.AssignToPurchase(id)`, uma única vez).
- Invariantes de criação no construtor da Purchase: pelo menos um pedido, um pedido por vendedor.
- Valores:
  - `OriginalTotalAmount`: congelado na criação, registro do valor original da compra.
  - `TotalAmount`: desnormalizado, recalculado por `RecalculateTotal(orders)` nos pontos de escrita listados no `docs/domain-model.md`.
- As influências entre os dois agregados (cancelamento de pedido altera a compra; pagamento da compra altera os pedidos) passam pela Application.

## Consequências
- ➕ Os totais e as regras de criação da compra ficam na Purchase, não no service.
- ➕ "Um pedido por vendedor" passa a ser garantido pelo domínio.
- ➖ O Order existe brevemente sem compra; o vínculo é garantido pelo `AssignToPurchase` e pela FK obrigatória.
- ➖ A Purchase confia nos Orders recebidos; a correspondência com o carrinho é responsabilidade do service.
- ➖ Sem concorrência otimista na Purchase, cancelamentos simultâneos de pedidos da mesma compra podem gravar um `TotalAmount` incorreto.

## Gatilho de revisão
- Concorrência otimista implementada: aplicar na Purchase.
- Criação da compra ganhar mais regras (cupom, frete): extrair uma factory no domínio.
