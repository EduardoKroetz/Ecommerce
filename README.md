# E-commerce API (.NET)

Projeto de estudo: um backend de e-commerce construído em fases, do monolito
simples rumo a uma arquitetura distribuída. O foco está nas decisões de
arquitetura e na refatoração, não em ser uma loja de produção.

> **Demo:** https://ecommerce-api-c6cgh6hgdrd9cfd0.westus3-01.azurewebsites.net/scalar
>
> Ambiente público de testes: qualquer usuário cadastrado pode alterar o catálogo
> e os dados podem ser resetados a qualquer momento.

## Por que este projeto existe

Um sandbox pessoal para praticar.
Ele começa propositalmente simples (domínio anêmico, pastas técnicas) e é refatorado à mão
ao longo das versões — a ideia é *sentir* os trade-offs de cada passo, em vez
de já começar no design "correto".

## Stack

- .NET / ASP.NET Core (Web API)
- PostgreSQL + Entity Framework Core
- ASP.NET Core Identity + JWT (autenticação)
- Docker + docker-compose
- Scalar (referência da API)
- CI/CD + deploy na Azure

## Rodando localmente

```bash
# 1. Clonar e entrar no projeto
git clone https://github.com/EduardoKroetz/Ecommerce.git
cd Ecommerce

# 2. Subir o PostgreSQL
docker-compose up -d

# 3. Rodar a API (as migrations são aplicadas automaticamente no startup)
dotnet run

# 4. API disponível em
#    http://localhost:8000
#    Referência da API: http://localhost:8000/scalar/v1
```

A configuração local (connection string e chaves de JWT) fica em
`appsettings.Development.json`, já versionado com valores de desenvolvimento que
apontam para o PostgreSQL do `docker-compose.yml`. Em produção esses valores vêm
das configurações do App Service na Azure.

## Decisões de arquitetura

As decisões técnicas estão documentadas como ADRs em [`docs/adr/`](docs/adr/).

## Status

Projeto em desenvolvimento ativo, evoluindo por fases.
