# 0015 - DDD tático leve: agregados como fronteira de consistência

## Status
Aceito — complementa a [0014](0014-dominio-rico-em-entidades-com-invariantes.md)

## Contexto
A 0014 definiu onde fica o comportamento, mas não a fronteira entre entidades. Na refatoração, comandos passaram a navegar livremente: o cancelamento carregava `order.Purchase.Orders` e devolvia estoque por `item.Product`, sobrescrevendo baixas concorrentes do checkout.

## Opções consideradas
- **Só domínio rico, sem agregados:** regras entre entidades ficam sem dono.
- **Purchase contendo os Orders:** o vendedor teria que carregar a compra inteira para enviar o próprio pedido.
- **Serviço de domínio** para regras entre agregados: nenhum caso hoje.

## Decisão
DDD tático leve: entidades ricas, agregados, repositório por agregado e linguagem ubíqua.

- Agregado é o que precisa estar consistente ao final de uma transação. Critério, em ordem: regra que nunca pode ser violada → mesmo ciclo de vida → ator independente (este decide se divergirem).
- Só a raiz tem repositório; entre agregados, referência pelo id.
- Uma transação altera um agregado; exceções listadas no `docs/domain-model.md`.
- Regra que decide o estado de um agregado com dados de outros é método dele, recebendo os dados (`purchase.RecalculateTotal(orders)`). Coordenação de vários agregados fica na Application. Refina o último item da 0014.
- Fora por enquanto: bounded contexts, serviços e eventos de domínio.

## Consequências
- ➕ Regras entre entidades têm dono; cada comando carrega e salva só o que altera.
- ➖ Dados de outro agregado são carregados explicitamente pelo repositório.
- ➖ Leituras entre agregados precisam de mecanismo próprio (0016).
- ➖ Checkout, pagamento e cancelamentos ainda alteram mais de um agregado por transação.

## Gatilho de revisão
- Mensageria: trocar as exceções por eventos de domínio.
- Regra pura sem agregado dono, usada por vários serviços: serviço de domínio.