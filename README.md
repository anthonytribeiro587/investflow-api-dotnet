# InvestFlow API .NET — Prova técnica

Esta API é uma prova de migração do InvestFlow para a stack da empresa: **ASP.NET Core + C#**.

Ela não substitui o sistema inteiro ainda. A função dela é mostrar que o InvestFlow pode sair de uma stack rápida de protótipo e evoluir para uma arquitetura corporativa em .NET.

## Endpoints para apresentar

- `/health` — mostra que a API .NET está online.
- `/swagger` — documentação visual dos endpoints.
- `/api/arquitetura` — explica a arquitetura atual, a prova em .NET e a produção sugerida.
- `/api/dashboard` — tenta buscar KPIs reais no banco Supabase/PostgreSQL. Se não tiver conexão configurada, retorna dados demonstrativos.
- `/api/solicitacoes` — tenta buscar as últimas solicitações reais. Se não tiver conexão configurada, retorna dados demonstrativos.

## Como rodar localmente

```bash
dotnet restore
dotnet run
```

Acesse:

```txt
http://localhost:5000/swagger
```

ou a URL indicada pelo terminal.

## Como conectar no Supabase

No Supabase:

1. Abra o projeto.
2. Vá em **Project Settings > Database**.
3. Clique em **Connect**.
4. Copie a connection string do Postgres.
5. Configure a variável:

```txt
SUPABASE_DB_URL=Host=...;Port=...;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
```

Importante: não colocar essa string dentro do código público.

## Deploy sugerido para teste rápido

### Opção Render

1. Suba este projeto para um repositório no GitHub.
2. No Render, escolha **New > Web Service**.
3. Conecte o repositório.
4. Use Docker.
5. Adicione as variáveis:
   - `SUPABASE_DB_URL`
   - `CORS_ORIGINS`
   - `ASPNETCORE_ENVIRONMENT=Production`
6. Depois do deploy, abra:
   - `/health`
   - `/swagger`
   - `/api/dashboard`

## Discurso para apresentação

“O InvestFlow atual foi feito como protótipo funcional para validar o fluxo de CAPEX. Como a empresa utiliza .NET, criei também uma prova técnica em ASP.NET Core consumindo o mesmo conceito de dados. Isso mostra que a solução não depende do Supabase ou da stack inicial. A versão corporativa pode seguir o padrão interno: .NET, banco homologado, autenticação corporativa, auditoria, backup e revisão da TI.”
