using PonteGames.Api.Services;
using Xunit;

namespace PonteGames.Tests;

public class GamificationServiceTests
{
    private readonly GamificationService _service = new();

    [Theory]
    [InlineData("eletronico", 2.0, 100)]
    [InlineData("oleo", 1.5, 60)]
    [InlineData("metal", 3.0, 90)]
    [InlineData("plastico", 1.0, 20)]
    public void CalcularXp_DeveCalcularMultiplicadorCorreto(string categoria, double peso, int xpEsperado)
    {
        
        var resultado = _service.CalcularXp(categoria, peso);

        
        Assert.Equal(xpEsperado, resultado);
    }

    [Fact]
    public void CalcularXp_QuandoPesoForZeroOuNegativo_DeveRetornarZero()
    {
       
        var resultadoZero = _service.CalcularXp("plastico", 0);
        var resultadoNegativo = _service.CalcularXp("plastico", -5);


        Assert.Equal(0, resultadoZero);
        Assert.Equal(0, resultadoNegativo);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(250, 3)]
    public void CalcularNivel_DeveEvoluirNivel_ACada100Pontos(int xp, int nivelEsperado)
    {
        
        var nivel = _service.CalcularNivel(xp);

       
        Assert.Equal(nivelEsperado, nivel);
    }
}