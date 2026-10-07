# 0001 - Uso de PostgreSQL como banco de dados

## Status
Aceito

## Contexto
A stack comum de .NET para persistência de dados é o SQL Server. Porém como é um projeto pessoal,
precisa ser publicado com custo zero e ficar disponível o mês inteiro.

## Opções consideradas
- **SQL Server (Azure SQL Database, oferta gratuita):** existe tier gratuito, mas limitado a 100.000 vCore-segundos
  por mês (~28 horas de vCore). Com uso contínuo, a cota se esgota e o banco pausa até o mês seguinte,
  ou passa a ser cobrado.
- **PostgreSQL (Neon):** tier gratuito sem prazo, sem cota mensal de computação que derrube o banco no meio do mês.

## Decisão
Usaremos PostgreSQL como banco de dados, hospedado em provedor com tier gratuito sem prazo (Neon)

## Consequências

### Positivas
- Hospedagem gratuita e sem prazo, viabilizando o deploy contínuo para o início do projeto.
- Experiência com PostgreSQL, que é um banco de dados amplamente usado no mercado

### Negativas
- Divergência da stack comum de .NET, que é o SQL Server, podendo gerar dificuldades em encontrar soluções prontas para problemas específicos.
- Diferenças de comportamento (case sensitivity, timestamp/fuso, collation) exigem atenção que o SQL Server não exigia.
