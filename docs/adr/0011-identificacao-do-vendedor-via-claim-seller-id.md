# 0011 - Identificação do vendedor via claim `seller_id` no JWT + policy

## Status
Aceito

## Contexto
O sistema passa a operar como marketplace: entidade `Seller` vinculada 1:1 a `User` (índice único em `UserId`), com `SellerId` obrigatório em `Product` e `Order`. Pedidos são single-seller.

Surgem duas perguntas de autorização distintas:

1. O usuário é vendedor? Define quem pode gerenciar produtos.
2. O vendedor é dono deste recurso? Define quem pode editar este produto ou enviar este pedido.

Comprador não é perfil: qualquer usuário autenticado pode comprar, inclusive vendedores. O papel de comprador é definido pela compra (`Purchase.UserId`), não por role.

## Decisão
- Na emissão do JWT, se existir `Seller` para o `UserId`, o token recebe o claim `seller_id`.
- A policy `RequireSeller` exige a presença do claim. É aplicada à escrita de produtos e de categorias; a leitura do catálogo continua pública. Categorias são globais: qualquer vendedor pode gerenciá-las e vinculá-las aos próprios produtos.
- `ICurrentUserProvider` expõe `SellerId` como opcional, lido do claim. No cadastro de produto, o `SellerId` vem sempre do provider, nunca do corpo da requisição.
- A pergunta 2 não é responsabilidade da policy. A propriedade é verificada no repositório, com filtro por `SellerId` no `WHERE`. Um recurso de outro vendedor retorna 404, seguindo o mesmo padrão do filtro de dono já usado em pedidos.
- Em pedidos, têm acesso o comprador (`Purchase.UserId`) e o vendedor do pedido (`Order.SellerId`); qualquer outro usuário recebe 404.
- Não existe role de comprador.
- O perfil de vendedor é definido no cadastro da conta. Não existe conversão posterior de usuário comum em vendedor, então o claim está presente desde o primeiro token emitido.

## Alternativas consideradas
- **Role `Seller` no JWT.** Responde à pergunta 1, mas não informa qual vendedor; exigiria uma consulta extra para obter o `SellerId`. Descartada.
- **Consulta ao banco por requisição (`UserId → Seller`).** É sempre consistente e não tem problema de token desatualizado; o custo é uma busca indexada, desprezível nesta escala. Descartada por simplicidade, não por desempenho: o claim responde "é vendedor?" e "qual vendedor?" sem acesso ao banco.
- **Resource-based authorization (`IAuthorizationService` + `AuthorizationHandler<TRequirement, TResource>`).** É o mecanismo oficial do ASP.NET Core para autorizar por dono. Descartada porque:
  - acopla a Application ao ASP.NET Core, ou obriga a Api a carregar o recurso antes de chamar o service;
  - o resultado natural é 403, que revela a existência do recurso e contradiz a decisão de responder 404;
  - a regra atual é uma igualdade simples, que o filtro no `WHERE` já resolve.

## Consequências

### Positivas
- A autorização não acessa o banco.
- Um único claim responde às duas necessidades: identificar o vendedor e saber se o usuário é um.
- A Application não depende de tipos do ASP.NET Core.

### Negativas
- Um vendedor desativado mantém acesso até o token expirar. A janela de revogação é igual ao tempo de vida do token.
- Um usuário comum não consegue se tornar vendedor; precisa de uma nova conta.
- Há duas fontes de verdade: a tabela `Seller` é a verdade e o token é cache.
- A policy não garante propriedade. Todo endpoint de escrita de vendedor precisa do filtro por `SellerId`. Esquecer esse filtro significa um vendedor alterar dados de outro. Esse cenário deve ser coberto por teste de integração em cada endpoint.

## Gatilho de revisão
- Conversão de usuário comum em vendedor → o token precisa ser reemitido no momento da conversão, ou o claim fica ausente até o próximo login.
- Desativação ou banimento de vendedor com efeito imediato → consulta ao banco ou tokens curtos com refresh token.
- Regras de acesso compostas (admin, acesso delegado, equipe do vendedor) → resource-based authorization.
- Um usuário com mais de uma loja → um claim único deixa de ser suficiente.
