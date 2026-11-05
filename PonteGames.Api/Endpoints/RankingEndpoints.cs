using Microsoft.EntityFrameworkCore;
using PonteGames.Api.Data;
using PonteGames.Api.DTOs;

namespace PonteGames.Api.Endpoints;

public static class RankingEndpoints
{
    public static RouteGroupBuilder MapRankingEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (AppDbContext db) =>
        {
            var ranking = await db.Usuarios
                .OrderByDescending(u => u.PontosXp)
                .Take(10)
                .Select(u => new RankingItemDto(u.Id, u.Nome, u.PontosXp, u.Nivel))
                .ToListAsync();

            return Results.Ok(ranking);
        })
        .Produces<List<RankingItemDto>>(StatusCodes.Status200OK)
        .WithSummary("Retorna a tabela dos 10 melhores classificados");

        return group;
    }
}