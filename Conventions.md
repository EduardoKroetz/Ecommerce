## Terminologia
- **User**: identidade autenticada. `UserId` só aparece na borda (Identity, JWT, `ICurrentUserProvider`).
- **Customer**: papel de comprador. Todo usuário tem um.
- **Seller**: papel de vendedor. Opcional.
- Nunca usar `UserId` para representar um papel; nunca usar o termo "Buyer".

## Nomenclatura de repositórios
- Dentro de `IXRepository`, métodos não repetem "X" (o tipo já diz): `GetById`, não `GetProductById`.
- Leitura de item: `GetDetailByIdAsync`. Leitura de lista com filtro: `SearchAsync` / `GetListAsync`. 
- Escrita de domínio: `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` (recebem/retornam entidade).
- Validação de existência: `ExistsByNameAsync`.
- Operação em lote sem regra: prefixo `Bulk` + verbo de intenção (`BulkAssignCategoryAsync`).
- Operações que podem falhar por uma condição esperada (não por erro) usam
  prefixo `Try` e retornam `bool`: `TryDecrementStockAsync` → true se aplicou,
  false se a condição não foi satisfeita. Segue a convenção do `TryParse` da BCL.
- Filtros de dados usam `By`: a chave primária vem primeiro, depois os
  qualificadores, unidos por `And`: `GetByIdAsync`, `ExistsByNameAsync`.
- Escopo de acesso por papel usa `For{Papel}` e vem depois dos filtros:
  `GetByIdForCustomerAsync`, `SearchForSellerAsync`,
  `GetDetailByIdForCustomerOrSellerAsync`. O parâmetro do papel é obrigatório.
- Escopo de acesso nunca é expresso como filtro de coluna (`ByIdAndUserId`).

## Services
- Operação repetida entre métodos públicos do mesmo service (ex.: a mesma validação em criar e
  atualizar) vai para um método privado reutilizado, nunca duplicada. Ex.: `AuthService.CreateUserAsync`.

## Nomenclatura de DTOs

**Entrada é nomeada pela ação. Representação de recurso, pelo dado. Resultado de operação, pela operação.**
Teste: se a rota mudar e o nome de uma representação precisar mudar junto, ela foi nomeada pelo endpoint.

| Tipo | Padrão | Exemplos |
|---|---|---|
| Entrada (body) | `{Ação}{Recurso}Request` | `CreateProductRequest`, `AddCartItemRequest`, `LoginRequest` |
| Entrada (query string) | `{Ação}{Recurso}Query` / `{Conceito}Query` | `SearchProductsQuery`, `PaginationQuery` |
| Representação | `{Recurso}Response` | `ProductResponse`, `OrderResponse` |
| Item de agregado | `{Agregado}{Item}Response` | `OrderItemResponse`, `CartItemResponse` |
| Listagem enxuta | `{Recurso}SummaryResponse` | `ProductSummaryResponse` |
| Visão por audiência | `{Audiência}{Recurso}Response` | `SellerOrderResponse` |
| Resultado de operação | `{Ação}{Recurso}Response` | `LoginResponse`, `RegisterSellerResponse` |
| Envelope | `PagedResult<T>` | `PagedResult<ProductResponse>` |

- Representação ou resultado? Se o dado pode ser lido por outro caminho (um GET), é representação.
  Se só existe como desfecho da ação (token emitido, ids gerados), é resultado de operação.
- Ação é o verbo de negócio, não o HTTP: `CreateProductRequest`, nunca `PostProductRequest`.
- Entradas são separadas por operação, mesmo com campos idênticos (criar ≠ atualizar). Nada de `Upsert`
  para duas operações; ação sem dados (`POST /purchases/{id}/pay`) não tem DTO.
- Resultados de operação não são reusados entre operações, e não usam campos anuláveis que dependem
  da operação. Representações são reusadas por dado — a assimetria é intencional.
- `{Recurso}Response` é a representação completa, para detalhe e listagem. `Summary` só nasce quando a
  listagem diverge (nunca por antecipação). Não existe variante `Detail`.
- Variante por audiência só quando o formato muda, não o filtro.
- Aninhados reusam a representação do recurso (`PurchaseResponse.Orders` é `List<OrderResponse>`).
- Proibido: verbo de leitura em representação (`GetProduct`) e sufixo `Dto`.
- Pastas: uma por agregado, no plural (`Auth/`, `Carts/`, `Orders/`, `Products/`, `ProductCategories/`,
  `Purchases/`), com os DTOs de itens na pasta do agregado. Tipos genéricos (`PagedResult`,
  `PaginationQuery`) ficam na raiz de `DTOs/`; não criar `Common/` ou `Shared/`.

## Regras de validação compartilhadas

| Tipo | Exemplo | Onde | Quem referencia |
|---|---|---|---|
| Invariante de negócio | quantidade máxima por item | `const` na entidade dona (Domain) | DTO |
| Limite de campo / persistência | tamanho do nome do produto | `const` na entidade dona (Domain) | DTO, EF |
| Política da aplicação | senha, tamanho de página | `Application/Policies/`, um arquivo por conceito | DTO, Identity |

- Na dúvida: se mudar a regra exige conversar com o "negócio", é Domain.
- Nunca repetir o valor literal; camadas externas referenciam a constante.
- Constantes são `const` (exigência de argumentos de atributo), em PascalCase (`MaxQuantity`).
- Toda invariante declarada no domínio é aplicada pela entidade. O DTO antecipa (400); a entidade garante (422).
- Credenciais têm uma única fonte: `IdentityOptions` vêm de `PasswordPolicy`, e erros do
  `IdentityResult` viram `ValidationException` (400).
