## Nomenclatura de repositórios
- Dentro de `IXRepository`, métodos não repetem "X" (o tipo já diz): `GetById`, não `GetProductById`.
- Leitura de item: `GetDetailByIdAsync`. Leitura de lista com filtro: `SearchAsync` / `GetListAsync`. 
- Escrita de domínio: `GetByIdAsync`, `Add`, `AddRange`, `Delete` (recebem/retornam entidade). `Add`, `AddRange` e `Delete` são síncronos: só registram a alteração, sem I/O.
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

## Gravação e tracking
- Repositório não salva: o service chama `uow.SaveChangesAsync()` (ADR 0020). Única exceção: `UserManager.CreateAsync`, que salva por conta própria.
- Sem `Update()`/`UpdateRange()`: entidades carregadas com tracking são gravadas pelo `SaveChanges`, só nas colunas alteradas.
- Métodos que carregam entidade para alterar nunca usam `AsNoTracking`: a alteração não seria gravada e a versão de concorrência não seria checada (ADR 0019).
- `AsNoTracking` é para leitura: projeções e entidades só consultadas, como `ProductRepository.GetListByIdsAsync`.

## Entidades
- Todas herdam `BaseEntity` (`Id`). Rica ou anêmica conforme o ADR 0014. Critérios de agregado, de valores derivados e de onde cada regra mora: docs/domain-model.md.
- Rica (tem invariante): todas as propriedades com `private set`; mudanças só por construtor ou método.
  Entidades ricas têm um construtor private sem parâmetros, exclusivo do EF, marcado com o comentário `// EF`.
  Coleções expostas como `IReadOnlyCollection<T>` sobre um campo `private readonly List<T> _nome`.
- Anêmica (CRUD puro): setters públicos.
- Várias propriedades sem regra própria mudam juntas por um único `Update(...)`. Não criar um método por
  propriedade (`UpdateName`, `UpdateDescription`) só para validar; método dedicado só quando a mudança
  tem regra própria (`IncreaseStock`, `Cancel`).
- Transição de status recusada lança `Invalid{Entidade}Status`, com o status atual em `details` (`currentStatus`).
  Código próprio `{Entidade}Already{Ação}` só quando a mesma ação já foi aplicada (`OrderAlreadyCancelled`,
  `PurchaseAlreadyPaid`): o cliente pode tratá-lo como sucesso numa nova tentativa.

## Services
- Operação repetida entre métodos públicos do mesmo service (ex.: a mesma validação em criar e
  atualizar) vai para um método privado reutilizado, nunca duplicada. Ex.: `AuthService.CreateUserAsync`.
- Método de service com mais de 3 parâmetros recebe um objeto de entrada no lugar deles.
  Até 3, parâmetros diretos (`CreateAsync(userId)`).
- Data atual vem do `TimeProvider` injetado, via `timeProvider.GetUtcDateTime()` (extensão em
  `Application/Extensions`). Nunca `DateTime.UtcNow`. O service passa a data pronta para a entidade.

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

## Testes
Estratégia (o que testar em cada nível): ADR 0021.

### Organização
- `Ecommerce.UnitTests` testa o domínio; `Ecommerce.IntegrationTests` testa services e API.
- Uma classe `{Entidade}Tests` por agregado. Itens do agregado são testados na classe da raiz
  (`OrderItem` em `OrderTests`), com o prefixo do item no nome do teste (`CreateOrderItem_...`).
- Pastas espelham o que é testado (`Entities/`). As classes de dados de teste ficam em `TestData/`.

### Nomes
- Padrão `{Método}_{Condição}_{Resultado}`. O método vai sem o sufixo `Async`, que é assinatura e não comportamento: `Create_...`, nunca `CreateAsync_...`.
- Condição: `With...` para a entrada (`Create_WithoutItems`), `When...` para o estado (`MarkAsPaid_WhenAwaitingPayment`).
  Omitida quando o cenário é o caso válido padrão (`Create_StartsAwaitingPayment`).
- Resultado: `IsRejected` para rejeição do domínio, `MovesTo{Status}` para transição de status.
  Nos demais casos, descreve o que é verificado (`Create_TotalIsSumOfItemPriceTimesQuantity`).

### Estrutura
- Preparação, ação e verificação separadas por linha em branco, sem comentários `// Arrange`.
- Um comportamento por teste. Variações da mesma entrada usam `[Theory]` com `[InlineData]`.

### TestData
- Uma classe `{Entidade}TestData` com `Create(...)`: todos os parâmetros opcionais, com valores padrão válidos.
- O teste informa por nome só o parâmetro relevante ao cenário (`OrderTestData.Create(items: [])`).
- Ids explícitos quando a regra depende deles. O `Id` de entidade, gerado pelo banco, é definido com `WithId(...)`,
  extensão de teste sobre `BaseEntity` (`Extensions/`). Fora isso, nunca reflection para alterar estado.

### Asserções
- Rejeição do domínio: `Assert.Throws<DomainException>` e conferir o `Code`, nunca a mensagem.
- Erro de programação (`InvalidOperationException`, `ArgumentOutOfRangeException`) é verificado pelo tipo.

### Integração
- O service é obtido por `Services.Get<T>(customerId, sellerId)`, do container de DI, com `ICurrentUserProvider` e
  `TimeProvider` substituídos por stubs.
- Cenários são montados por `TestData` (`TestData.SellerAsync()`, `TestData.ProductAsync(...)`), com parâmetros opcionais
  e valores padrão válidos, também no Act. Nada de métodos privados de montagem dentro da classe de teste.
- Verificação: por padrão, pela operação pública de leitura correspondente (`GetByIdAsync` do service). `QueryAsync`
  (sem tracking, em escopo novo) só para efeitos que nenhuma operação de leitura expõe: versão de concorrência e
  estado interno (ex.: `SellerId` do produto).
- Os dados são limpos com Respawn antes de cada teste.
- Conflito de concorrência é provocado pelo interceptor do EF, nunca por chamadas paralelas.
