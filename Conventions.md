
## Nomenclatura de repositórios
- Dentro de `IXRepository`, métodos não repetem "X" (o tipo já diz): `GetById`, não `GetProductById`.
- Leitura de item: `GetDetailByIdAsync`. Leitura de lista com filtro: `SearchAsync` / `GetListAsync`. 
- Escrita de domínio: `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` (recebem/retornam entidade).
- Validação de existência: `ExistsByNameAsync`.
- Operação em lote sem regra: prefixo `Bulk` + verbo de intenção (`BulkAssignCategoryAsync`).
- Operações que podem falhar por uma condição esperada (não por erro) usam
  prefixo `Try` e retornam `bool`: `TryDecrementStockAsync` → true se aplicou,
  false se a condição não foi satisfeita. Segue a convenção do `TryParse` da BCL.
- Ao compor filtros no nome, a chave primária vem primeiro, depois os
  qualificadores: `GetDetailByIdAndUserIdAsync`, não `...ByUserIdAndId`.