# 0007 - Domínio anêmico na v1 (lógica de negócio no controller)

## Status
Aceito

## Contexto
O cálculo do total do pedido e a baixa de estoque são regras de negócio. 
Podem viver em métodos das entidades (domínio rico) ou no controller/serviço (domínio anêmico). Minha preferência de design é por domínio rico.

## Opções consideradas
- **Domínio rico:** entidades com comportamento (ex: `order.AddItem(...)`, `product.DecreaseStock(qty)`), protegendo as próprias invariantes.
- **Domínio anêmico:** entidades só com dados; a regra fica no controller (ou num serviço).

## Decisão
Na v1, a lógica fica no controller e as entidades são anêmicas — contra a preferência por domínio rico, de forma deliberada.

A v1 é o ponto de partida cru da refatoração planejada para a v2 (monolito → camadas, com domínio de verdade para Pedido e Estoque). 
Começar anêmico e migrar para rico na v2 é intencional: faz parte de sentir a diferença entre as duas abordagens na prática, em vez de já começar no destino.

## Consequências

### Positivas
- v1 mais simples e rápida de entregar.
- A refatoração da v2 passa a ter um "antes" concreto para comparar — o próprio objetivo de aprendizado do projeto.

### Negativas
- Regra de negócio exposta no controller é mais difícil de testar isoladamente — na v1 só dá para testar via integração, não unitário (o que a v2 corrige ao mover a lógica para o domínio).
- Risco de a lógica se espalhar/duplicar entre controllers se a v2 demorar.

## Atenção para a v2
Hoje a baixa de estoque no checkout é um `UPDATE` condicional atômico executado no banco (`StockBalance >= quantidade`), que impede vender a mesma unidade duas vezes em checkouts simultâneos. Ao mover essa regra para a entidade (`product.DecreaseStock(qty)`), a checagem passa a acontecer em memória sobre um valor lido antes — sem um token de concorrência (ex: `xmin` do PostgreSQL), a venda duplicada volta.
