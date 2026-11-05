namespace PonteGames.Api.DTOs;

public record RegistroDescarteDto(int UsuarioId, string CategoriaResiduo, double PesoKg);

public record DescarteResponseDto(int Id, int UsuarioId, string CategoriaResiduo, int XpGanho, DateTime DataRegistro);

public record RankingItemDto(int Id, string Nome, int PontosXp, int Nivel);