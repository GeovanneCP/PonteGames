namespace PonteGames.Api.Services;

public interface IGamificationService
{
    int CalcularXp(string categoria, double pesoKg);
    int CalcularNivel(int totalXp);
}

public class GamificationService : IGamificationService
{
    public int CalcularXp(string categoria, double pesoKg)
    {
        if (pesoKg <= 0) return 0;

        int multiplicador = categoria.Trim().ToLower() switch
        {
            "eletronico" => 50,
            "oleo" => 40,
            "metal" => 30,
            "plastico" => 20,
            _ => 10
        };

        return (int)Math.Round(pesoKg * multiplicador);
    }

    public int CalcularNivel(int totalXp)
    {
        return 1 + (totalXp / 100);
    }
}