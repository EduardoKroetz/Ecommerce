# 0016 - Navegações entre agregados como `internal` e projeções na Infra

## Status
Aceito

## Contexto
A [0015](0015-ddd-tatico-leve-agregados-como-fronteira-de-consistencia.md) proíbe comandos de navegar entre agregados. Removidas as navegações, as projeções de leitura passaram a receber `IQueryable` por parâmetro e repassá-lo entre níveis. Manter as navegações só com convenção não protege: o relationship fixup do EF as preenche sem `Include`, e o código gerado com IA usa qualquer navegação disponível.

## Opções consideradas
- **Navegações públicas + convenção:** violações funcionam silenciosamente.
- **Sem navegações entre agregados:** `IQueryable` atravessando as projeções.
- **Navegações `internal` + projeções na Infra:** fronteira garantida pelo compilador.
- **Inserir compra e pedidos num único `SaveChanges` via navegação:** acoplaria os construtores dos dois agregados.

## Decisão
- Navegações dentro do agregado: públicas. Entre agregados: `internal`, com `InternalsVisibleTo` só para a Infra.
- Navegações entre agregados são usadas nos métodos de consulta dos repositórios, incluindo projeções e filtros de escopo (ex.: `o.Purchase.CustomerId`).
- Projeções em `Infra/Projections/`, uma classe por agregado, aninhadas por injeção.
- DTOs de saída são só dados, com propriedades `required`.
- Comandos e métodos de entidade não usam navegações entre agregados.

## Consequências
- ➕ A Application não compila se cruzar a fronteira; projeções usam o EF nativamente.
- ➕ Campo esquecido numa projeção é erro de compilação.
- ➖ Adicionar um campo exige alterar DTO e projeção.
- ➖ O `internal` não protege o Domain dele mesmo.
- ➖ O checkout grava a compra antes dos pedidos.

## Gatilho de revisão
- Entidade navegando entre agregados: teste de arquitetura.
- Proliferação de leituras nos repositórios: separar em `IXQueries`.
