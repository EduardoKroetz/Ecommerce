# 0014 - Domínio rico em entidades com invariantes

## Status
Aceito — substitui a [0007](0007-dominio-anemico-na-v1.md)

## Contexto
A 0007 manteve o domínio anêmico na v1, de forma deliberada, como ponto de partida para a refatoração. Na v2, o domínio ganhou regras que dependem de estado e de atores: a máquina de estados do pedido, a compra que agrupa pedidos por vendedor e o carrinho com limites de quantidade. Com a lógica nos services, nada impede que essas regras sejam contornadas por uma atribuição direta, e elas só podem ser testadas por integração.

## Opções consideradas
- **Domínio rico em todas as entidades:** uniformidade, mas cerimônia sem ganho em entidades de CRUD puro.
- **Domínio rico só onde há invariante:** a entidade protege as regras que dependem do próprio estado; entidades de CRUD continuam anêmicas.
- **Mover a baixa de estoque para a entidade (`product.DecreaseStock(qty)`):** exigiria um token de concorrência (ex.: `xmin`) para não reintroduzir a venda duplicada apontada na 0007. O tratamento de concorrência está adiado.

## Decisão
- Entidades com invariantes são ricas: Order (incluindo OrderItem), Purchase, Cart (incluindo CartItem) e Product.
- Entidades de CRUD puro continuam anêmicas.
- Em entidades ricas, mudanças de estado acontecem apenas por métodos ou pelo construtor.
- A baixa de estoque continua no banco, pelo `UPDATE` condicional atômico (`TryDecrementStockAsync`). A invariante "estoque nunca negativo" não é responsabilidade da entidade.
- Regras que dependem de outros agregados ou de consultas (unicidade, regras entre compra e pedidos) ficam na Application.

## Consequências

### Positivas
- As invariantes passam a ser garantidas pela entidade, não pela disciplina de quem chama.
- As regras de domínio podem ser testadas unitariamente.
- A lógica de cada agregado fica num lugar só, em vez de espalhada pelos services.

### Negativas
- Montar testes exige levar a entidade ao estado desejado pelos próprios métodos (builders ou fábricas).
- O mapeamento do EF exige mais cuidado: backing fields em coleções e construtores privados.
- Nem toda invariante fica na entidade: a de estoque fica no banco e as que envolvem vários agregados ficam na Application. A regra de onde procurar uma invariante não é única.
- O modelo misto exige decidir, por entidade, se ela é rica ou anêmica.

## Gatilho de revisão
- Concorrência otimista implementada (ex.: `xmin`): reavaliar mover a baixa de estoque para a entidade.
- Uma entidade anêmica ganhar uma invariante: passa a ser rica.
