# 0019 - Concorrência otimista nas transições de status

## Status
Aceito

## Contexto
As transições de status leem a entidade, validam em memória e gravam depois, sem confirmar que o status lido ainda vale. Em requisições simultâneas, ganha quem grava por último: dois cancelamentos do mesmo pedido devolvem o estoque em dobro, pagar e cancelar ao mesmo tempo apaga o pagamento ou envia um pedido cujo estoque já voltou, e cancelar dois pedidos da mesma compra grava um `TotalAmount` errado e pode deixar a compra Pending com todos os pedidos cancelados. A [0018](0018-concorrencia-na-atualizacao-de-estoque.md) deixou para cá a decisão de *se* o estoque deve ser devolvido.

## Opções consideradas
- **Lock pessimista (`SELECT ... FOR UPDATE`):** serializa tudo que mexe na compra, inclusive leituras de comando sem conflito real, e exige SQL escrito à mão.
- **Update condicional com o status no `WHERE`:** tira a regra de transição da entidade e não protege o `TotalAmount`.
- **Concorrência otimista:** a regra continua na entidade; a gravação falha se a linha mudou desde a leitura.

## Decisão
- Token `xmin` do PostgreSQL em `Order` e `Purchase`, como shadow property `Version` configurada só com `IsRowVersion()`: sem coluna nova e sem propriedade no domínio.
- `Order` e `Purchase` só mudam por entidade com tracking e `SaveChanges`, nunca por `ExecuteUpdate`. Sem `Update()`, que marcaria todas as colunas e geraria conflitos falsos.
- Todo fluxo cuja decisão depende dos pedidos de uma compra altera a Purchase. A versão só é checada quando algum valor da linha muda.
- `DbUpdateConcurrencyException` vira 409 no handler global, e nunca é capturada dentro da transação: a falha desfaz tudo, inclusive os incrementos de estoque.
- Sem retry automático e sem idempotência por enquanto.

## Consequências
- ➕ A regra de transição continua na entidade, e não há lock entre a leitura e a gravação.
- ➕ O handler também cobre gravações em linhas que já não existem: checkout em dobro e remoção dupla de item do carrinho respondem 409 em vez de 500.
- ➖ Dois cancelamentos legítimos de pedidos diferentes da mesma compra: o segundo recebe 409 por conflito na Purchase e precisa repetir.
- ➖ A mesma ação em dobro responde 409 se for simultânea, ou 422 se chegar depois do commit.
- ➖ `Product`, `Cart` e `ProductCategory` seguem sem token: edições simultâneas continuam com lost update.
- ➖ A migration contém um `AddColumn` de `xmin`, que não gera SQL: `xmin` é coluna de sistema.

## Gatilho de revisão
- 409 frequente em cancelamentos legítimos: avaliar retry automático.
- Integração com gateway de pagamento: avaliar idempotência, pois um pagamento repetido passaria a cobrar duas vezes.
