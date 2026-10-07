# 0002 - Persistência e modelagem do carrinho

## Status
Aceito

## Contexto
Duas decisões resolvidas em conjunto:
1. **Onde o carrinho mora** — memória ou banco de dados.
2. **Como é modelado** — `CartItem` com `UserId`, ou entidade própria `Cart` com relação 1:N para os itens.

## Opções consideradas

### Persistência
- Memória (ex: `IMemoryCache`).
- Banco de dados.

### Modelagem
- `CartItem` com `UserId` direto, sem entidade de carrinho.
- Entidade `Cart` (1 por usuário) com `CartItem` em relação 1:N.

## Decisão

### 1 — Persistência: banco de dados
Descartamos memória. Além de perder dados no restart, o projeto caminha para rodar em múltiplas instâncias (v2), onde carrinho em memória quebra — cada instância teria seu próprio estado. Memória não é opção inferior, é incompatível com a arquitetura de destino.

### 2 — Modelagem: entidade própria (`Cart`)
A razão principal é capacidade futura: uma entidade com identidade própria ancora comportamentos que o e-commerce tende a exigir — `CreatedAt`/`UpdatedAt` (carrinho abandonado), convidado sem usuário, múltiplos carrinhos. Com `CartItem + UserId` solto, cada um vira refatoração; com `Cart`, a mudança fica concentrada numa entidade só.

O custo não é zero em todos os casos: `CreatedAt`/`UpdatedAt` são de fato só colunas, mas convidado e múltiplos carrinhos exigem mexer nas restrições atuais — `UserId` é obrigatório e tem índice único em `Carts` — e definir qual carrinho é o ativo.

Como reforço, alinha o modelo à linguagem de domínio.

## Consequências

### Positivas
- Itens sobrevivem ao restart.
- Compatível com escala horizontal.
- `Cart` abre espaço para evolução futura a baixo custo.

### Negativas
- Tabela e JOIN a mais nas consultas.
- Complexidade paga antecipadamente: na v1 nenhuma capacidade futura existe ainda — o retorno é uma aposta.
