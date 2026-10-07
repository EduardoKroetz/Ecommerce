# 0003 - Preço vivo do carrinho vs congelado do pedido

## Status
Aceito

## Contexto
O item do carrinho e o item do pedido tratam preço de formas opostas. É preciso definir, em cada um, se o preço acompanha o produto (vivo) ou é fixado (congelado).

## Opções consideradas

### Item do carrinho
- Vivo: reflete sempre o preço atual do produto.
- Congelado: fixa o preço no momento em que o item é adicionado.

### Item do pedido
- Vivo: recalculado a partir do preço atual do produto.
- Congelado: fixado no momento da criação do pedido.

## Decisão
Carrinho mantém preço **vivo**; pedido **congela** o snapshot no momento da compra.

O pedido é o registro do que foi efetivamente cobrado — tem que ser auditável e imutável. O carrinho é uma intenção de compra temporária, que deve mostrar o preço atual. O congelamento vale para o que identifica o item na compra (preço e nome), não só o número — senão renomear um produto reescreveria o histórico de pedidos antigos.

Pedido com preço vivo tornaria impossível saber quanto foi cobrado; carrinho congelado faria o cliente pagar um preço desatualizado. As alternativas rejeitadas são incorretas, não apenas piores.

## Consequências

### Positivas
- O pedido reflete o preço real da compra, auditável e imutável.
- O preço do carrinho nunca fica desatualizado em relação ao catálogo.

### Negativas
- Congelar exige duplicar dados: `OrderItem` copia preço (`UnitPrice`) e nome (`ProductName`), além de manter a referência a `Product`.
- A referência a `Product` usa `DeleteBehavior.Restrict`: produto que já está em algum pedido não pode ser apagado (a API responde 409).
- Preço vivo no carrinho pode mudar entre adicionar e o checkout. Na v1 essa surpresa não é tratada: o checkout cobra o preço atual sem avisar o cliente.

## Pendências (v1)
- O carrinho (`GetCartItem`) ainda não retorna preço nem subtotal — o "preço vivo" existe no modelo, mas não chega ao cliente.
- Detectar e avisar mudança de preço entre a adição ao carrinho e o checkout.
