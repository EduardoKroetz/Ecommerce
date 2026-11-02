# 0018 - Concorrência na atualização de estoque

## Status
Aceito

## Contexto
O checkout decrementa o estoque de vários produtos, e o cancelamento devolve. O fluxo "ler o produto, calcular em memória, gravar o valor" sofre lost update: com estoque 10, duas compras simultâneas de 2 e 3 unidades leem 10 e gravam 8 e 7, em vez de 5. Duas compras do último item seriam ambas aprovadas. O estoque é um dos dados mais disputados do sistema, e a regra é simples: o saldo não pode ficar negativo. A [0014](0014-dominio-rico-em-entidades-com-invariantes.md) manteve essa regra no banco; esta ADR detalha como.

## Opções consideradas
- **Lock pessimista com a regra na entidade:** preserva o domínio rico, mas trava a linha desde a leitura até o commit e exige SQL escrito à mão.
- **Concorrência otimista (versão no Product):** no cenário mais disputado, quase todas as tentativas falhariam e precisariam de retry.
- **Lock na aplicação (`lock`, `SemaphoreSlim`):** não protege com mais de uma instância nem contra outros escritores.
- **Update condicional atômico:** o banco decide e grava no mesmo comando.

## Decisão
O estoque só muda por comandos atômicos relativos. Nenhum fluxo grava um saldo lido antes; a edição do produto não altera o estoque.
- `TryDecrementStockAsync`: `SET StockBalance = StockBalance - @q WHERE Id = @id AND StockBalance >= @q`. Zero linhas afetadas resulta em `InsufficientStock`.
- `IncrementStockAsync` (cancelamento): relativo e sem condição. A decisão de *se* deve devolver pertence à concorrência nas transições de status.
- `TryAdjustStockAsync` (vendedor): relativo, com a condição `StockBalance + @q >= 0`.
- Os decrementos de uma compra ficam na mesma transação: a falha de qualquer item desfaz os anteriores.
- Dentro de uma transação, decrementos e incrementos seguem a ordem crescente de `ProductId`, considerando todos os itens da compra, para evitar deadlock. O ajuste do vendedor altera uma linha por comando e não precisa de ordem.
- A validação de estoque em memória serve só para responder rápido e listar os itens sem saldo. A garantia é o `UPDATE`.
- `CHECK ("StockBalance" >= 0)` como rede de segurança.

## Consequências
- ➕ Não há intervalo entre decidir e gravar, então não há janela para outra requisição.
- ➕ Sem retries: no Read Committed (padrão do PostgreSQL), quem chega depois espera o lock da linha e é avaliado sobre o saldo atualizado.
- ➖ A regra "saldo não negativo" fica na Infra, fora da entidade `Product`.
- ➖ Passa por fora do `SaveChanges`: um `Product` em memória fica desatualizado, e auditoria ou eventos não rodam.
- ➖ "Zero linhas" não distingue produto inexistente de estoque insuficiente: a mensagem é enganosa se o produto for excluído entre a carga e o decremento.
- ➖ O lock no produto dura até o commit, depois dos inserts de Purchase, Order e Cart.
- ➖ O incremento atômico não impede devolver em dobro: isso depende da concorrência otimista nas transições de status ([0019](0019-concorrencia-otimista-nas-transicoes-de-status.md)).

## Gatilho de revisão
- A regra de estoque deixar de caber em SQL (reserva com expiração, kits): migrar para lock pessimista.
- Disputa extrema num único produto (lançamentos): dividir o estoque em várias linhas ou serializar por fila.
