using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PonteGames.Api.Data;
using PonteGames.Api.Endpoints;
using PonteGames.Api.Models;
using PonteGames.Api.Services;
using PonteGames.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de Dados SQLite (ou Oracle via ConnectionString)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=pontegames.db"));

// 2. Injeção de Dependências & Validadores
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegistroDescarteValidator>();

// 3. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// 4. Autenticação e Autorização JWT
var jwtKey = builder.Configuration["JwtSettings:Secret"] ?? "ChavePadraoSeguraPonteGamesCitizenTech2026!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "PonteGamesApi",
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "PonteGamesApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// 5. Rate Limiting com Feedback Estruturado (HTTP 429)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            status = 429,
            mensagem = "Demasiadas requisições num curto intervalo de tempo. Aguarde alguns segundos."
        }, cancellationToken);
    };
    options.AddFixedWindowLimiter("GlobalLimiter", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 0;
    });
});

// 6. OpenAPI & Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ponte Games API - CitizenTech",
        Version = "v1",
        Description = "API de gamificação e descarte consciente para cidades sustentáveis."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduza o token JWT gerado: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Inicialização da base de dados e seed de demonstração
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Usuarios.Any())
    {
        db.Usuarios.AddRange(
            new Usuario { Nome = "Geovanne Passos", Email = "geovanne@pontegames.com", Role = "Cidadao", PontosXp = 320, Nivel = 4 },
            new Usuario { Nome = "Lucas Pinheiro", Email = "lucas@pontegames.com", Role = "Cidadao", PontosXp = 210, Nivel = 3 },
            new Usuario { Nome = "Guilherme Soares", Email = "guilherme@pontegames.com", Role = "Cidadao", PontosXp = 150, Nivel = 2 }
        );
        db.SaveChanges();
    }
}

// Pipeline HTTP
app.UseSwagger(c => { c.SerializeAsV2 = true; });
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ponte Games API v1");
    c.RoutePrefix = "swagger";
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Rota de Health Check
app.MapHealthChecks("/health").WithTags("Monitoramento");

// Mapeamento modular dos Endpoints
app.MapGroup("/api/auth").WithTags("Autenticacao").MapAuthEndpoints();
app.MapGroup("/api/descartes").WithTags("Descartes e Gamificacao").RequireRateLimiting("GlobalLimiter").RequireAuthorization().MapDescarteEndpoints();
app.MapGroup("/api/ranking").WithTags("Ranking Comunitario").MapRankingEndpoints();

app.Run();

public partial class Program { }