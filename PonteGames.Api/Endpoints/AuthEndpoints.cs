using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PonteGames.Api.Data;
using PonteGames.Api.DTOs;

namespace PonteGames.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (LoginRequestDto login, IConfiguration config, AppDbContext db) =>
        {
            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (usuario is null)
            {
                return Results.Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = config["JwtSettings:Secret"] ?? "ChavePadraoSeguraPonteGamesCitizenTech2026!";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Role, usuario.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = config["JwtSettings:Issuer"] ?? "PonteGamesApi",
                Audience = config["JwtSettings:Audience"] ?? "PonteGamesApp",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var usuarioDto = new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role, usuario.PontosXp, usuario.Nivel);
            return Results.Ok(new LoginResponseDto(tokenString, usuarioDto));
        })
        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithSummary("Autentica o utilizador e emite o token JWT");

        return group;
    }
}