using Bixa.Backend.Models.DTOs.UserModelDTO;
using FluentValidation;
using static Bixa.Backend.Models.Validations.ValidationMessages;

namespace Bixa.Backend.Models.DTOs.UserModelDTO.Validators;

public class SolicitarCorreccionDTOValidator : AbstractValidator<SolicitarCorreccionDTO>
{
    public SolicitarCorreccionDTOValidator()
    {
        RuleFor(x => x.Comentario)
            .NotEmpty().WithMessage(Required)
            .MaximumLength(2000).WithMessage(MaxLength);
    }
}
