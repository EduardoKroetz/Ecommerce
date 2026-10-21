# 0012 - Perfis de papel Customer e Seller sobre User

## Status
Aceito — 2026-10-19

Substitui parcialmente o ADR 0011 (identificação do vendedor), no ponto "comprador não é perfil; qualquer usuário autenticado pode comprar".

## Contexto
O comprador era representado diretamente pelo `UserId`. Como o vendedor também é um usuário, `UserId` passou a ter dois significados (identidade autenticada e papel de comprador), tornando parâmetros e métodos ambíguos.

Além disso, dados futuros do comprador (endereço de entrega, CPF, celular) não têm onde morar sem ficarem presos ao `User`, que é compartilhado com o vendedor.

## Decisão
- `User` representa apenas a identidade autenticada.
- `Customer` é o papel de comprador: obrigatório, 1:1 com todo `User` (índice único em `UserId`), criado na mesma transação de qualquer criação de usuário.
- `Seller` continua sendo um papel opcional, criado apenas no cadastro de vendedor. O cadastro de vendedor cria `User`, `Customer` e `Seller`, então vendedores também compram.
- Os dados do comprador são preenchidos sob demanda e validados no checkout, não no cadastro.
- O token inclui `customer_id` para todo usuário. Não há policy para comprador.
- Fora da borda de autenticação, `UserId` nunca representa um papel.

## Alternativas consideradas
- **Manter o `User` como comprador e só renomear os parâmetros (`buyerId`), ou usar IDs fortemente tipados.** Resolvem a ambiguidade de nome e de tipo, mas não dão lugar para os dados do comprador.
- **`Customer` opcional.** Um vendedor que quisesse comprar precisaria de uma conversão posterior ("tornar-se comprador"), trazendo de volta o problema do token sem o claim até ser reemitido. A única motivação, um usuário administrativo, ainda não existe.
- **Vendedor não compra com a mesma conta (um papel por conta).** Eliminaria a compra do próprio produto, mas contraria o modelo de marketplace em que vendedores também compram.

## Consequências

### Positivas
- Os papéis ficam explícitos no modelo e nos nomes.
- Os dados do comprador têm um lugar próprio, separado do vendedor.
- Como todo usuário é comprador, não há conversão de perfil nem claim ausente para comprador.

### Negativas
- Usuários que nunca compram (vendedores que só vendem, um futuro admin) têm um `Customer` vazio.
- O banco não garante que todo `User` tenha um `Customer`; a invariante depende de um ponto único de criação de usuários.
- Um vendedor pode comprar o próprio produto; a regra para isso ainda não foi decidida.

## Gatilho de revisão
- Surgir um tipo de usuário que nunca compra e cujo `Customer` vazio passe a causar problema.
- Surgir a necessidade de criar o `Customer` depois do cadastro.

## Relacionadas
- ADR 0011 (identificação do vendedor via claim `seller_id`) — o `customer_id` segue o mesmo mecanismo de claim no JWT.
