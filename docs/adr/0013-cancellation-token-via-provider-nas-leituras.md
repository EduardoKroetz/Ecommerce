# 0013 - CancellationToken via provider, só nas leituras

## Status
Aceito — 2026-10-20

## Contexto
Os repositórios recebiam `CancellationToken` por parâmetro, mas nenhum controller ou service o repassava: tudo rodava com `default` e nada era cancelado. O repasse manual por todas as camadas é cerimônia que, na prática, não aconteceu.

## Decisão
- Um `ICancellationTokenProvider` (Infra) expõe o `HttpContext.RequestAborted`; os repositórios o injetam.
- Só leituras usam o token. Escritas (`Add`, `Update`, `Delete`, `SaveChanges`) não são canceladas: cancelar um comando no meio pode deixar estado inconsistente quando houver efeitos fora do banco (ex.: gateway de pagamento).
- `CancellationToken` sai das assinaturas de repositórios, services e controllers.
- Fora de uma requisição HTTP, o provider devolve `CancellationToken.None`.

## Alternativas consideradas
- **Token explícito, repassado de verdade.** É o padrão do .NET (BCL, EF, analisador CA2016), mas mantém a cerimônia em todas as camadas.
- **Provider também nas escritas.** Cancelaria comandos no meio quando o cliente desconecta.

## Consequências

### Positivas
- Leituras são canceladas quando o cliente aborta a requisição, sem depender de repasse.
- Assinaturas limpas em todas as camadas.

### Negativas
- Dependência escondida (ambient context): a assinatura não mostra que o método é cancelável.
- Desvio consciente do padrão do .NET de token explícito.

## Gatilho de revisão
- Surgirem jobs em background ou `HostedService` com token próprio de parada: o provider precisará aceitar um token definido por escopo (ex.: `AsyncLocal`).
