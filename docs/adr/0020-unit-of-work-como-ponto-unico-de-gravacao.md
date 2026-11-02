# 0020 - Unit of Work como ponto único de gravação

## Status
Aceito

## Contexto
Cada método de escrita dos repositórios executava seu próprio `SaveChanges`, e o `Update()` marcava todas as colunas como alteradas. Um comando com vários repositórios salvava várias vezes, e a concorrência otimista ([0019](0019-concorrencia-otimista-nas-transicoes-de-status.md)) geraria conflitos falsos em linhas que não mudaram.

## Opções consideradas
- **Repositório salva:** esconde onde o comando grava e divide uma alteração em vários `SaveChanges`.
- **Filtro que salva ao fim da requisição:** não exige chamada manual, mas esconde o ponto de gravação e não atende quem precisa do id gerado no meio do comando.
- **`IUnitOfWork.SaveChangesAsync()` chamado pelo service.**

## Decisão
- O service chama `uow.SaveChangesAsync()` manualmente. Os repositórios não salvam.
- `Add`, `AddRange` e `Delete` são síncronos: só registram a alteração. Não existe `Update`: entidades com tracking são gravadas pelo `SaveChanges`.
- Operações atômicas (`ExecuteUpdate`) executam na hora, fora do UoW. Combinadas com `SaveChanges`, precisam de transação.
- Um comando que precisa do id gerado salva mais de uma vez, dentro de uma transação (checkout).
- Exceção: `UserManager.CreateAsync` salva por conta própria, então o registro roda em transação. Trocar isso exigiria um store customizado sem ganho real.

## Consequências
- ➕ O ponto de gravação fica visível no service.
- ➕ Um único `SaveChanges` já é atômico: o pagamento dispensa transação explícita.
- ➖ Um `SaveChanges` esquecido não gera erro de compilação: o método responde sucesso sem gravar.
- ➖ O id de uma entidade nova só existe depois do save, inclusive em `CustomerService.Create` e `SellerService.Create`.

## Gatilho de revisão
- Esquecimentos recorrentes do `SaveChanges`, ou adoção de eventos de domínio: avaliar o filtro automático.
