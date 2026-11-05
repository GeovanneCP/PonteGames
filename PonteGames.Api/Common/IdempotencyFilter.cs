using Microsoft.EntityFrameworkCore;
using PonteGames.Api.Data;

namespace PonteGames.Api.Common;

public class IdempotencyFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        // Valida se o cabeçalho X-Idempotency-Key foi enviado
        if (!httpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var key) || string.IsNullOrWhiteSpace(key))
        {
            return Results.BadRequest(new { mensagem = "O cabeçalho X-Idempotency-Key é obrigatório para esta operação." });
        }

        var dbContext = httpContext.RequestServices.GetRequiredService<AppDbContext>();
        bool keyExists = await dbContext.Descartes.AnyAsync(d => d.IdempotencyKey == key.ToString());

        if (keyExists)
        {
            return Results.Conflict(new { mensagem = "Operação já processada com esta chave de idempotência." });
        }

        return await next(context);
    }
}