using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PonteGames.Api.Common;
using PonteGames.Api.Data;
using PonteGames.Api.DTOs;
using PonteGames.Api.Models;
using PonteGames.Api.Services;

namespace PonteGames.Api.Endpoints;

public static class DescarteEndpoints
{
    public static RouteGroupBuilder MapDescarteEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            RegistroDescarteDto dto,
            HttpContext ctx,
            AppDbContext db,
            IGamificationService gamification,
            IValidator<RegistroDescarteDto> validator) =>
        {
            // 1. Executa a validação formal das regras de negócio
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            string? idempotencyKey = ctx.Request.Headers["X-Idempotency-Key"].FirstOrDefault();

            var usuario = await db.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario is null)
            {
                return Results.NotFound(new { mensagem = "Cidadão não encontrado na base de dados." });
            }

            int xp = gamification.CalcularXp(dto.CategoriaResiduo, dto.PesoKg);
            usuario.PontosXp += xp;
            usuario.Nivel = gamification.CalcularNivel(usuario.PontosXp);

            var descarte = new Descarte
            {
                UsuarioId = usuario.Id,
                CategoriaResiduo = dto.CategoriaResiduo.Trim().ToLower(),
                XpGanho = xp,
                IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString()
            };

            db.Descartes.Add(descarte);
            await db.SaveChangesAsync();

            var response = new DescarteResponseDto(descarte.Id, descarte.UsuarioId, descarte.CategoriaResiduo, descarte.XpGanho, descarte.DataRegistro);
            return Results.Created($"/api/descartes/{descarte.Id}", response);
        })
        .AddEndpointFilter<IdempotencyFilter>()
        .Produces<DescarteResponseDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .WithSummary("Registra um novo descarte sustentável respeitando os limites da CitizenTech");

        group.MapGet("/historico/{usuarioId:int}", async (int usuarioId, AppDbContext db) =>
        {
            var historico = await db.Descartes
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.DataRegistro)
                .Select(d => new DescarteResponseDto(d.Id, d.UsuarioId, d.CategoriaResiduo, d.XpGanho, d.DataRegistro))
                .ToListAsync();

            return Results.Ok(historico);
        })
        .Produces<List<DescarteResponseDto>>(StatusCodes.Status200OK)
        .WithSummary("Consulta o histórico de descartes de um cidadão");

        return group;
    }
}