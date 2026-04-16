using FluentValidation;
using static Bixa.Backend.Models.Validations.ValidationMessages;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.DTOs.RequestModelDTO;
using Bixa.Backend.Models.Configurations;

namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Validators;

public class RequestInsertDTOValidator : AbstractValidator<RequestInsertDTO>
{
    public RequestInsertDTOValidator()
    {
        When(x => x.EstablishmentID.HasValue, () =>
        {
            RuleFor(x => x.EstablishmentID!.Value)
                .IsValidId();
        });

        When(x => x.DeviceID.HasValue, () =>
        {
            RuleFor(x => x.DeviceID!.Value)
                .IsValidId();
        });

        When(x => !string.IsNullOrEmpty(x.UnitName), () =>
        {
            RuleFor(x => x.UnitName)
                .MaximumLength(255);
        });

        When(x => x.RequesterUserID.HasValue, () =>
        {
            RuleFor(x => x.RequesterUserID!.Value)
                .IsValidId();
        });

        When(x => x.ExecutiveUserID.HasValue, () =>
        {
            RuleFor(x => x.ExecutiveUserID!.Value)
                .IsValidId();
        });

        When(x => !string.IsNullOrEmpty(x.Observations), () =>
        {
            RuleFor(x => x.Observations)
                .MaximumLength(ModelLengths.Observation).WithMessage(MaxLength);
        });

        When(x => x.AccordID.HasValue, () =>
        {
            RuleFor(x => x.AccordID!.Value)
                .IsValidId();
        });
    }
}