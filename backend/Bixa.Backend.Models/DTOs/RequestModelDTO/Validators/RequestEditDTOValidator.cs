using FluentValidation;
using static Bixa.Backend.Models.Validations.ValidationMessages;
using Bixa.Backend.Models.DTOs.RequestModelDTO;

namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Validators;

public class RequestEditDTOValidator : AbstractValidator<RequestEditDTO>
{
    public RequestEditDTOValidator()
    {
    }
}