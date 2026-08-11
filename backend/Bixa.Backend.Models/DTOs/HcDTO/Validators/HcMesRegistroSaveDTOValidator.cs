using FluentValidation;

namespace Bixa.Backend.Models.DTOs.HcDTO.Validators;

public class HcMesRegistroSaveDTOValidator : AbstractValidator<HcMesRegistroSaveDTO>
{
    public HcMesRegistroSaveDTOValidator()
    {
        RuleFor(x => x.Mes1)
            .GreaterThanOrEqualTo(0).WithMessage("El valor de Mes 1 no puede ser negativo.");

        RuleFor(x => x.Mes2)
            .GreaterThanOrEqualTo(0).WithMessage("El valor de Mes 2 no puede ser negativo.");

        RuleFor(x => x.Mes3)
            .GreaterThanOrEqualTo(0).WithMessage("El valor de Mes 3 no puede ser negativo.");
    }
}
