# Ecommerce.IntegrationTests

Testes dos services contra um PostgreSQL real, com a mesma composição de DI de produção. O porquê está na [ADR 0021](../docs/adr/0021-estrategia-de-testes.md); as regras de escrita, no [Conventions.md](../docs/Conventions.md#testes).

Para rodar, o Docker precisa estar ativo: `dotnet test Ecommerce.IntegrationTests`.

## As peças

| Peça | Problema que resolve |
|---|---|
| `DatabaseFixture` | Subir o Postgres (Testcontainers), montar o DI e aplicar as migrations é caro: acontece uma vez por suíte. |
| `DatabaseCollection` | Compartilha a fixture entre as classes e faz os testes rodarem um de cada vez, já que o banco é único. |
| `Respawner` | Apaga as linhas de todas as tabelas sem recriar o schema. |
| `TestTimeProvider` | Único `TimeProvider` registrado; por trás dele, cada teste ganha um `FakeTimeProvider` novo. |
| `TestCurrentUserProvider` | Substitui o usuário do JWT: o teste define quem está chamando. |
| `IntegrationTestBase` | Liga a classe à collection, reseta banco e relógio, cria os usuários padrão e expõe `Services`, `TestData`, `Customer`, `Seller`, `Time`, `BeforeNextWrite` e `QueryAsync`. |
| `ServiceResolver` | Imita uma requisição: um escopo novo por chamada, como o usuário escolhido. |
| `TestData` | Monta cenários pelos services públicos, com valores padrão válidos. |
| `DefaultTestUsers` | Customer e seller padrão do teste: usados por `Services.Get` e `TestData` quando o teste não informa outro. |
| `TestUser` | Quem o service representa. Os retornos de cadastro convertem para ele, e `null` significa anônimo. |
| `ConcurrentRequestInterceptor` | Roda a requisição do `BeforeNextWrite` no primeiro ponto de escrita do service testado. |

## Ciclo de vida

```mermaid
flowchart TD
    A["Suíte começa<br/>sobe o Postgres e aplica as migrations"] --> B

    subgraph Each["Para cada teste"]
        B["InitializeAsync<br/>Respawn limpa o banco, relógio volta a Now,<br/>customer e seller padrão são criados"] --> C["Arrange e Act<br/>TestData e Services.Get"]
        C --> D["Assert<br/>leitura pública, ou QueryAsync"]
        D --> E["DisposeAsync<br/>fecha os escopos abertos"]
    end

    E -->|próximo teste| B
    E --> F["Suíte termina<br/>container parado e removido"]
```

O reset vem antes do teste, e não depois: se um teste falhar, os dados dele ficam no banco para inspeção.

## Exemplo

```csharp
public class ProductServiceTests(DatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Create_PersistsProductForCurrentSeller()
    {
        var created = await TestData.ProductAsync(Seller, stockBalance: 5);

        var product = await Services.Get<ProductService>().GetByIdAsync(created.Id);
        Assert.Equal(5, product.StockBalance);
    }
}
```

`Services.Get<T>()` resolve o service como o customer padrão; `Services.Get<T>(Seller)` ou `Services.Get<T>(otherCustomer)`, como aquele usuário; `Services.Get<T>(null)`, anônimo. O `TestData` segue a mesma regra: sem customer ou seller, usa os padrão.

## Armadilhas

- **O relógio não volta:** o `FakeTimeProvider` recusa voltar no tempo, por isso a troca a cada teste fica no `TestTimeProvider`. Não registre um `FakeTimeProvider` direto.
- **Sem argumento é o padrão, não um desconhecido:** `TestData.PurchaseAsync()` é uma compra do customer padrão, e dois `ProductAsync()` são do mesmo seller (um pedido só). Para outro usuário, crie um explícito (`TestData.CustomerAsync()`).
- **Escopos vivem até o fim do teste:** o service usa o `DbContext` do escopo; descartá-lo antes quebra o service.
- **Toda classe de teste herda `IntegrationTestBase`:** é ela que coloca a classe na collection e faz o reset.
