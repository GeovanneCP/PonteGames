namespace PonteGames.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Cidadao"; // "Cidadao" ou "Gestor"
    public int PontosXp { get; set; } = 0;
    public int Nivel { get; set; } = 1;
}