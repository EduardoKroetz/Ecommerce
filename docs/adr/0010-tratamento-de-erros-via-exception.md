# 0010 - Tratamento de erros via Exception (sem Result Pattern)

## Status
Aceito

## Contexto
Os métodos de aplicação precisam sinalizar falhas — tanto falhas de recurso
(entidade não encontrada) quanto decisões de negócio (estoque insuficiente).
Duas abordagens:

- **Result Pattern:** o método retorna `Result<T>` (sucesso ou erro), e o
  chamador verifica explicitamente. O erro fica visível na assinatura.
- **Exception:** o método lança; o erro sobe até um middleware central que o
  converte em `ProblemDetails` (ver ADR 0005).

## Decisão
Usaremos Exception como mecanismo único de tratamento de erro. Não misturamos
os dois — um método ou lança (e propaga) ou conclui; nenhum retorna `Result` e
lança ao mesmo tempo.

Para evitar proliferação de tipos (uma exception por regra), a variação do erro
vive em dados, não em subclasses:

- `DomainException(code, message, details?)` — uma classe para todas as regras
  de negócio. O que distingue cada regra é o `Code` (ex: `INSUFFICIENT_STOCK`),
  e dados estruturados (ex: itens faltantes) vão em `Details`.
- `NotFoundException` — recurso que deveria existir e não existe.
- `ValidationException` — input malformado (formato, obrigatoriedade, regra de
  campo; ex: senha fraca no registro). Carrega lista de erros de campo
  (code + description). Distinta de `DomainException`: validação responde "você
  mandou errado" (→ 400); domínio responde "você mandou certo, mas o negócio
  não permite" (→ 422).
- `ConflictException` — operação conflita com o estado atual (ex: apagar produto que está em pedido) (→ 409).
- `UnauthorizedException` — credenciais inválidas no login (→ 401).
- O middleware central lê a exception e monta o `ProblemDetails`, com `Details`
  indo para `extensions`.

## Consequências

### Positivas
- Happy path limpo: o fluxo principal não carrega verificação de erro em cada
  chamada — o que dá errado sobe sozinho.
- Alinhado ao grão do ecossistema C#/BCL, construído sobre exceptions (ex:
  `int.Parse` lança). Result não tem suporte de linguagem — nada obriga a checá-lo.
- Poucas classes de exception: a granularidade da regra fica no `Code`, não na
  hierarquia de tipos.
- Integra com o middleware de `ProblemDetails` já existente (ADR 0005).

### Negativas
- Decisões de negócio trafegam no canal de exceção, que conceitualmente é para o
  excepcional — "sem estoque" não é raro nem é bug. É uma imprecisão semântica
  assumida.
- Sem `catch` tipado por regra: ramificar por regra exige ler o `Code`, não
  capturar um subtipo. Aceitável porque regra de negócio sobe para o middleware,
  não é capturada no meio do fluxo.
- Exception tem custo de performance (stack trace/unwinding). Irrelevante no uso
  atual (no máximo um throw por request), mas proíbe lançar dentro de loops de
  validação de muitos itens — nesses casos, acumular e lançar uma vez.
- Preferência de leitura pesou na escolha; não é um argumento de que Exception
  seja conceitualmente superior a Result.
- Erros do ASP.NET Core Identity são hoje agrupados em `ValidationException`,
  misturando validação de formato (senha fraca → 400) com conflito (email já
  existe → idealmente 409). Agrupamento aceito por simplicidade; separar se a
  distinção importar.

## Gatilho de revisão
A decisão assume que erros de negócio sempre propagam para o middleware. Se
algum fluxo (ex: checkout) precisar que o próprio service **reaja** a um erro de
negócio em vez de propagá-lo — ramificando a lógica — a exception vira controle
de fluxo disfarçado, e naquele ponto um retorno explícito passa a ser melhor.