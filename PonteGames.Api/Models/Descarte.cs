namespace PonteGames.Api.Models;

public class Descarte
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string CategoriaResiduo { get; set; } = string.Empty; // Eletronico, Plastico, Metal, Oleo
    public int XpGanho { get; set; }
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    public string IdempotencyKey { get; set; } = string.Empty;
}