
# ADRs

**Mapeamento de DTOs: Método Tradicional VS Expression**

Decidi usar Expression para mapear os DTOs principalmente por causa de como o Entity Framework Core lida com o método .Select().

**O Problema**

Quando se usa um método comum do C# dentro do .Select() — tipo .Select(p => GetProduct.Map(p)) — e tenta aplicar qualquer outro método depois que use esse novo tipo (como .Where, .OrderBy ou .GroupBy), a aplicação dá erro.

Isso acontece porque o EF não consegue ler o código que está dentro do método C# para traduzir para SQL. Exemplo:

```
var products =await _dbContext.Products
    .Select(p => GetProduct.Map(p))
    .OrderBy(dto => dto.CreatedAt)// ERRO: O EF não sabe o que é 'CreatedAt' no banco
    .ToListAsync();
```

**Por que não apenas inverter a ordem?**

Daria para colocar o .OrderBy antes e o .Select depois para resolver o erro, mas isso traz problemas:

- **Tráfego de dados desnecessário:** O banco envia todas as colunas da tabela para o C#, mesmo as que não foram mapedas no DTO.
- **Exigência de .Include():** Se o DTO precisar de tabelas relacionadas (como Categorias), se torna obrigatório colocar o .Include() no começo da query, senão elas vêm vazias.

**Por que a Expression é melhor?**

Ao mudar o mapeamento para uma Expression, o EF consegue ler toda a estrutura e traduzir tudo direto para o banco de dados. As vantagens são:

- Você pode colocar filtros (Where) e ordenações (OrderBy) em qualquer ordem que funciona.
- O banco só retorna as colunas que realmente vão ser usadas no DTO (melhor performance).
- Não precisa de .Include(), já que o EF faz os JOINs automáticos no banco ao referenciar as tabelas relacionadas.