
## Nomenclatura de repositórios
- Dentro de `IXRepository`, métodos não repetem "X" (o tipo já diz): `GetById`, não `GetProductById`.
- Leitura de item: `GetDetailByIdAsync`. Leitura de lista com filtro: `SearchAsync` / `GetListAsync`.
- Escrita de domínio: `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` (recebem/retornam entidade).
- Validação de existência: `ExistsByNameAsync`.
- Operação em lote sem regra: prefixo `Bulk` + verbo de intenção (`BulkAssignCategoryAsync`).