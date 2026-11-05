namespace PonteGames.Api.DTOs;

public record LoginRequestDto(string Email);

public record LoginResponseDto(string Token, UsuarioDto Usuario);

public record UsuarioDto(int Id, string Nome, string Email, string Role, int PontosXp, int Nivel);