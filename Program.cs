using System.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration["CORS_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "https://invest-flow-azure.vercel.app" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("InvestFlowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("InvestFlowFrontend");

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/health", () => new
{
    status = "online",
    api = "InvestFlow API .NET",
    stack = "ASP.NET Core + C#",
    message = "Prova técnica da migração do InvestFlow para a stack .NET."
});

app.MapGet("/api/arquitetura", () => new
{
    prototipoAtual = "Next.js + Supabase + Vercel",
    provaDotnet = "ASP.NET Core Web API + PostgreSQL/Supabase",
    producaoSugerida = ".NET + SQL Server/Azure SQL ou banco homologado + autenticação corporativa",
    mensagem = "O InvestFlow não depende do Supabase. O Supabase foi usado para acelerar o protótipo; a camada oficial pode seguir o padrão técnico da empresa."
});

app.MapGet("/api/dashboard", async () =>
{
    var connectionString = GetConnectionString(app.Configuration);
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Ok(MockDashboard("sem_connection_string"));
    }

    try
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        var totalSolicitacoes = await ScalarLong(conn, "select count(*) from solicitacoes;");
        var aprovadas = await ScalarLong(conn, "select count(*) from solicitacoes where status ilike '%aprovada%';");
        var rejeitadas = await ScalarLong(conn, "select count(*) from solicitacoes where status ilike '%rejeitada%';");

        // Se alguma coluna ainda não existir no banco do protótipo, a API cai para dados demonstrativos.
        decimal valorOrcado = 0;
        decimal valorRealizado = 0;

        try
        {
            valorOrcado = await ScalarDecimal(conn, "select coalesce(sum(valor_orcado), 0) from itens_projeto;");
            valorRealizado = await ScalarDecimal(conn, "select coalesce(sum(valor_realizado), 0) from itens_projeto;");
        }
        catch
        {
            // Mantém os KPIs principais mesmo se itens_projeto/colunas ainda não estiverem padronizados.
        }

        var saldo = valorOrcado - valorRealizado;
        var execucaoPercentual = valorOrcado > 0 ? Math.Round((valorRealizado / valorOrcado) * 100, 2) : 0;

        return Results.Ok(new
        {
            origem = "dados_reais_supabase_postgres",
            tecnologia = "ASP.NET Core consumindo PostgreSQL",
            totalSolicitacoes,
            aprovadas,
            rejeitadas,
            valorOrcado,
            valorRealizado,
            saldo,
            execucaoPercentual
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(MockDashboard("erro_conexao_ou_schema", ex.Message));
    }
});

app.MapGet("/api/solicitacoes", async () =>
{
    var connectionString = GetConnectionString(app.Configuration);
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Ok(MockSolicitacoes("sem_connection_string"));
    }

    try
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        const string sql = """
            select id, ano, prioridade, status, justificativa, created_at
            from solicitacoes
            order by created_at desc
            limit 15;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var rows = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync())
        {
            var item = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                item[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            }
            rows.Add(item);
        }

        return Results.Ok(new
        {
            origem = "dados_reais_supabase_postgres",
            tecnologia = "ASP.NET Core + Npgsql",
            totalRetornado = rows.Count,
            dados = rows
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(MockSolicitacoes("erro_conexao_ou_schema", ex.Message));
    }
});

app.Run();

static string? GetConnectionString(IConfiguration config)
{
    // Aceita os dois formatos para facilitar deploy.
    return config.GetConnectionString("InvestFlow")
        ?? config["SUPABASE_DB_URL"]
        ?? config["DATABASE_URL"];
}

static async Task<long> ScalarLong(NpgsqlConnection conn, string sql)
{
    await using var cmd = new NpgsqlCommand(sql, conn);
    var result = await cmd.ExecuteScalarAsync();
    return Convert.ToInt64(result ?? 0);
}

static async Task<decimal> ScalarDecimal(NpgsqlConnection conn, string sql)
{
    await using var cmd = new NpgsqlCommand(sql, conn);
    var result = await cmd.ExecuteScalarAsync();
    return Convert.ToDecimal(result ?? 0);
}

static object MockDashboard(string motivo, string? detalhe = null) => new
{
    origem = "dados_demonstrativos",
    motivo,
    detalhe,
    tecnologia = "ASP.NET Core funcionando; conectar SUPABASE_DB_URL para dados reais",
    totalSolicitacoes = 644,
    aprovadas = 536,
    rejeitadas = 108,
    valorOrcado = 20942199.70m,
    valorRealizado = 0m,
    saldo = 20942199.70m,
    execucaoPercentual = 0m,
    mensagem = "Esta resposta prova a camada .NET. Ao configurar a connection string do Supabase, os endpoints passam a buscar dados reais."
};

static object MockSolicitacoes(string motivo, string? detalhe = null) => new
{
    origem = "dados_demonstrativos",
    motivo,
    detalhe,
    tecnologia = "ASP.NET Core funcionando; conectar SUPABASE_DB_URL para dados reais",
    dados = new[]
    {
        new { id = "demo-001", ano = 2026, prioridade = "Alta", status = "aprovada_diretoria", justificativa = "Substituição de equipamento crítico", created_at = DateTime.UtcNow.AddDays(-2) },
        new { id = "demo-002", ano = 2026, prioridade = "Média", status = "em_orcamento", justificativa = "Melhoria operacional em filial", created_at = DateTime.UtcNow.AddDays(-1) },
        new { id = "demo-003", ano = 2026, prioridade = "Baixa", status = "ajuste_solicitado", justificativa = "Revisar descrição e valor estimado", created_at = DateTime.UtcNow }
    }
};
