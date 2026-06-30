# InvestFlow API .NET

Prova técnica em ASP.NET Core para mostrar que o InvestFlow pode migrar para a stack .NET da empresa.

## Endpoints

- `/`
- `/health`
- `/api/health`
- `/api/arquitetura`
- `/api/dashboard`
- `/api/solicitacoes`
- `/swagger`

## Variáveis no Render

```env
ASPNETCORE_ENVIRONMENT=Production
CORS_ORIGINS=https://invest-flow-azure.vercel.app
SUPABASE_DB_URL=postgresql://postgres.xxxxx:SUA_SENHA@xxxxx.pooler.supabase.com:5432/postgres
```
