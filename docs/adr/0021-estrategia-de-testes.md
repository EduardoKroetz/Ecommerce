# 0021 - Estratégia de testes: unitários no domínio, integração nos services

## Status
Aceito

## Contexto
O projeto não tem testes automatizados no repositório. As refatorações para domínio rico ([0014](0014-dominio-rico-em-entidades-com-invariantes.md), [0015](0015-ddd-tatico-leve-agregados-como-fronteira-de-consistencia.md)) foram validadas por scripts e2e fora do repositório, rodados manualmente: a verificação não é versionada nem roda no CI.

O comportamento crítico está em três lugares:
- **Domínio:** máquina de estados do `Order`, regras do `Cart`, criação da `Purchase`, cálculo de totais. São regras puras, sem dependências externas.
- **Banco e coordenação:** decremento condicional de estoque via `ExecuteUpdate` e `CHECK` ([0018](0018-concorrencia-na-atualizacao-de-estoque.md)), concorrência otimista com `xmin` ([0019](0019-concorrencia-otimista-nas-transicoes-de-status.md)), transações e `SaveChanges` controlados pelo service ([0020](0020-unit-of-work-como-ponto-unico-de-gravacao.md)). Esse comportamento só existe onde o SQL roda e onde a transação é controlada.
- **API:** autorização (`RequireSeller`, 401/403), mapeamento de exceções para status no `GlobalExceptionHandler` e validação dos DTOs.

## Opções consideradas
- **Services com repositórios e UoW mockados:** só verifica chamadas (`IncrementStockAsync` foi chamado?), não o resultado. Não enxerga transação, `xmin`, `CHECK` nem a semântica do `ExecuteUpdate`, e quebra a cada refatoração do service.
- **Services com mocks e testes de integração por repositório:** cobre o SQL, mas nunca a atomicidade, porque a fronteira da transação está no service. Duplica o esforço e acopla os testes à interface dos repositórios, que além disso são `internal`.
- **EF Core InMemory ou SQLite:** não reproduzem transação, `xmin`, `ExecuteUpdate` condicional nem `CHECK` do Postgres. Passariam justamente nos cenários de maior risco.
- **Domínio com testes unitários; services com testes de integração contra Postgres real.**

## Decisão
- **Critério:** cada comportamento é testado no nível mais barato em que pode ser observado. A integração não repete regra que o domínio já garante: prova só o que depende do banco, da transação ou da composição.
- **Domínio → testes unitários.** Entidades com regra são testadas pela API pública, sem banco e sem mocks.
- **Services → testes de integração contra Postgres real.** O banco é uma dependência gerenciada (só a aplicação o acessa): a interação com ele é detalhe de implementação, então usa-se a instância real, nunca mock.
- **Repositórios não têm testes dedicados:** são exercitados pelos testes dos services. O escopo por papel (`For{Papel}`), os filtros e a ordenação também são verificados pelos services, com cenários de outro usuário.
- **API → testes finos com `WebApplicationFactory`**, só para autorização, mapeamento de exceções para status e contrato (validação dos DTOs, serialização). Regra de negócio não é retestada na API.
- **Montagem:** o service é obtido pelo container de DI (`AddInfrastructure`), como em produção. As entradas do ambiente (usuário atual via `ICurrentUserProvider` e data via `TimeProvider`) são substituídas por implementações fixas de teste (stubs).
- **Mocks para verificar interação** só em dependências não gerenciadas, cuja interação é observada por sistemas externos (gateway de pagamento, mensageria, e-mail). Hoje não há nenhuma.
- **Postgres via Testcontainers:** um container por execução da suíte, igual localmente e no CI.
- **Isolamento:** dados limpos com Respawn antes de cada teste, preservando o schema. Os testes de integração não rodam em paralelo entre si.
- **Concorrência:** o conflito é provocado de forma determinística por um interceptor do EF, que grava uma alteração concorrente na linha antes do `SaveChanges` do service.
- **Projetos separados:** `Ecommerce.UnitTests` (domínio) e `Ecommerce.IntegrationTests` (services e API). Têm velocidades e dependências diferentes e podem rodar em etapas distintas do CI.
- **xUnit** como framework.
- **Sem `InternalsVisibleTo` para os projetos de teste:** os testes usam só a API pública. Precisar de um membro `internal` é sinal de problema no teste ou no design.
- As convenções de escrita (nomes, estrutura, dados de teste e asserções) ficam em `docs/Conventions.md`.

## Consequências
- ➕ Atomicidade e concorrência ficam verificáveis: o teste observa o estado final no banco, inclusive o rollback em conflito.
- ➕ Os testes de service resistem a refatoração: fundir ou renomear métodos de repositório não quebra nada.
- ➕ O mapeamento do EF (shadow property de `xmin`, navegações `internal`, configurações e migrations) é exercitado sem custo extra.
- ➕ Os testes unitários do domínio são rápidos e falham de forma específica.
- ➖ Os testes de integração são mais lentos e exigem Docker, localmente e no CI. O CI precisa de um runner Linux (`ubuntu-latest`) para essa etapa, porque runners Windows não executam containers Linux.
- ➖ Falhas na integração são menos específicas: a causa pode estar na consulta, no mapeamento ou no service.
- ➖ Sem paralelismo entre os testes de integração, a suíte fica mais lenta conforme cresce.
- ➖ O interceptor prova a reação ao conflito, mas não reproduz o timing real de duas requisições.
- ➖ Risco de explosão combinatória se cada ramo de decisão do service virar cenário de integração. Mitigação: manter a lógica de decisão no domínio e deixar ao service só a coordenação.

## Gatilho de revisão
- Suíte de integração lenta a ponto de não ser rodada a cada mudança: reavaliar a proporção entre os níveis, o paralelismo ou o isolamento.
- Entrada de uma dependência não gerenciada (pagamento, mensageria): definir onde ficam os mocks de fronteira.
- Services acumulando lógica de decisão que não cabe no domínio: reavaliar testes unitários de service.
