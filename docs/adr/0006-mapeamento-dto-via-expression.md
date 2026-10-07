# 0006 - Mapeamento de DTOs via Expression em vez de método tradicional

## Status
Aceito

## Contexto
As queries de leitura projetam entidades em DTOs. 
O mapeamento pode ser feito por um método C# tradicional (`.Select(p => GetProduct.Map(p))`) ou por uma `Expression` que o EF Core traduz para SQL.

## Decisão
Usaremos `Expression` para o mapeamento das projeções.

O método C# tradicional dentro do `.Select()` não é traduzível para SQL — o EF não consegue ler o corpo do método. Isso quebra ao encadear operações sobre o tipo projetado:

```csharp
_dbContext.Products
    .Select(p => GetProduct.Map(p))
    .OrderBy(dto => dto.CreatedAt) // erro: EF não traduz o DTO para SQL
```

Inverter a ordem (`OrderBy` antes do `Select`) resolve o erro, 
mas força o banco a trazer todas as colunas da entidade e exige `Include()` explícito para relacionamentos. 

Com `Expression`, o EF traduz o mapeamento inteiro para SQL: a ordem das operações deixa de importar, 
o banco retorna só as colunas projetadas, e a projeção dispensa `Include` (ao referenciar um campo relacionado, o EF gera o JOIN apenas para aquele campo).

## Consequências

### Positivas
- Projeções traduzidas no servidor: menos dados trafegados (só as colunas do DTO).
- `Where`/`OrderBy` funcionam em qualquer ordem sobre a projeção.
- Dispensa `Include` para dados relacionados que entram no DTO.

### Negativas
- A `Expression` de mapeamento só vale dentro de queries `IQueryable`. Diferente de um método comum, não é reutilizável para mapear um objeto já em memória sem compilá-la (`.Compile()`) ou manter um mapeamento separado.
- Mapeamento mais verboso e menos legível que um método direto.