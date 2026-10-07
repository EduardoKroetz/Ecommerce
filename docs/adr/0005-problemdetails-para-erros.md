# 0005 - ProblemDetails para respostas de erro

## Status
Aceito

## Contexto
A API precisa de um formato consistente para respostas de erro. Duas opções: um DTO de erro próprio do sistema, ou o `ProblemDetails` nativo do ASP.NET Core (RFC 9457).

## Decisão
Usaremos `ProblemDetails` como formato padrão de erro.

A razão principal é padronização: `ProblemDetails` é um formato de erro especificado por RFC, com campos previsíveis (`type`, `title`, `status`, `detail`). Qualquer consumidor da API já sabe interpretá-lo sem precisar aprender um contrato próprio — um DTO customizado obrigaria cada cliente a conhecer o formato específico do sistema.

Como fator secundário, já vem pronto no framework, sem criar tipos novos. Vale registrar que eu não dominava o formato antes — optei por ele mesmo custando aprendizado, por ser a opção padronizada, em vez do DTO customizado que eu já conhecia bem.

## Consequências

### Positivas
- Interoperável: clientes parseiam o erro sem documentação adicional.
- Zero código de contrato de erro próprio para manter.
- Integração nativa com o ASP.NET Core: erros de validação de model (`[ApiController]`), `NotFound()` e `Problem(...)` já produzem `ProblemDetails`, e exceções não tratadas também, via `AddProblemDetails()` + `UseExceptionHandler()` no `Program.cs`.

### Negativas
- Menos flexível para erros de estrutura rica: dados extras (ex: lista de itens sem estoque) vão em `extensions`, menos limpo que um DTO desenhado para o caso.
- Acopla o contrato de erro a um padrão externo — se um dia precisar de formato muito específico, foge do padrão ou convive com os dois.
