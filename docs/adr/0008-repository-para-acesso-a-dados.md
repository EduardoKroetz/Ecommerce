# 00XX - Repository para acesso a dados na Application

## Status
Aceito

## Contexto
A camada Application precisa acessar dados persistidos. O `DbContext` do EF
mora na Infra (onde é "certo" ficar, por ser infraestrutura), e a Application
não referencia a Infra — a dependência aponta para dentro (Infra → Application).

Duas opções reais:

- **EF direto:** usar o `DbContext` na Application. Como ele está na Infra,
  exigiria ou expor um `IApplicationDbContext` na Application (interface que
  ainda vaza tipos do EF, como `DbSet<T>`), ou mover o `DbContext` para a
  Application (acoplando a camada ao EF). Ambos acoplam a Application ao EF.
- **Repository:** interfaces (`IProductRepository`) definidas na Application e
  implementadas na Infra. A Application não conhece o EF.

(O `IApplicationDbContext` foi considerado apenas como parte do caminho EF
direto e descartado junto com ele — não desacopla de fato, pois expõe tipos do
EF na Application.)

## Decisão
Usaremos o padrão Repository. As interfaces ficam na Application; as
implementações (com EF) na Infra.

A razão principal não é superioridade técnica — para entidades de leitura
variada (catálogo) o Repository é até pior que `IQueryable` direto. É manter a
Application testável e livre de EF, e exercitar a inversão de dependência na
prática.

## Consequências

### Positivas
- Application desacoplada do EF: regra de orquestração testável sem infra.
- Métodos nomeados (`GetActiveByCategory`) leem melhor que queries soltas.
- Inversão de dependência aplicada de verdade — a interface na Application
  quebra o ciclo que apareceria se a Application referenciasse a Infra.

### Negativas
- Proliferação de métodos: cada variação de busca tende a virar um método no
  repositório. Em entidades de query variada (catálogo), isso enrijece.
- Alternativa à proliferação seria vazar `IQueryable` pela interface — mas isso
  anula o desacoplamento (a query volta a montar no service). Não há meio-termo
  grátis.
- Teste unitário de service com repositório mockado cobre só a orquestração,
  não a query real — esta ainda exige teste de integração.
- Camada/indireção a mais para buscas que seriam uma linha de EF.

## Gatilho de revisão
Decisão tomada em parte para observar, na prática, se a proliferação de métodos
se concretiza conforme o sistema cresce. Se os repositórios de entidades de
catálogo (ex: `Product`) acumularem muitos métodos de filtro específicos,
reconsiderar: Repository só para agregados (`Order`, `Cart`) + EF direto ou
query objects para leituras de catálogo.