using FluentValidation;
using PonteGames.Api.DTOs;

namespace PonteGames.Api.Validators;

public class RegistroDescarteValidator : AbstractValidator<RegistroDescarteDto>
{
    private static readonly Dictionary<string, double> LimitesPorCategoria = new()
    {
        { "oleo", 5.0 },         // Máximo 5L de óleo residencial
        { "eletronico", 20.0 },   // Máximo 20kg de lixo eletrônico
        { "plastico", 10.0 },     // Máximo 10kg de recicláveis secos
        { "metal", 10.0 },
        { "papel", 10.0 }
    };

    public RegistroDescarteValidator()
    {
        RuleFor(x => x.UsuarioId)
            .GreaterThan(0)
            .WithMessage("O identificador do cidadão deve ser maior que zero.");

        RuleFor(x => x.CategoriaResiduo)
            .NotEmpty()
            .WithMessage("A categoria do resíduo é obrigatória.")
            .Must(cat => LimitesPorCategoria.ContainsKey(cat.Trim().ToLower()))
            .WithMessage("Categoria inválida. Categorias aceitas: oleo, eletronico, plastico, metal, papel.");

        RuleFor(x => x.PesoKg)
            .GreaterThan(0.05)
            .WithMessage("O peso descartado deve ser no mínimo 0.05 kg (50g).");

        // Regra avançada: valida o peso conforme a categoria informada
        RuleFor(x => x)
            .Must(ValidarLimiteCategoria)
            .WithMessage(x =>
            {
                var cat = x.CategoriaResiduo?.Trim().ToLower() ?? "";
                var limite = LimitesPorCategoria.TryGetValue(cat, out var max) ? max : 10.0;
                return $"Limite excedido para {x.CategoriaResiduo}. O descarte individual máximo permitido para cidadãos é de {limite} kg/L.";
            });
    }

    private bool ValidarLimiteCategoria(RegistroDescarteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoriaResiduo)) return true;
        var categoria = dto.CategoriaResiduo.Trim().ToLower();

        if (LimitesPorCategoria.TryGetValue(categoria, out double limiteMaximo))
        {
            return dto.PesoKg <= limiteMaximo;
        }

        return false;
    }
}