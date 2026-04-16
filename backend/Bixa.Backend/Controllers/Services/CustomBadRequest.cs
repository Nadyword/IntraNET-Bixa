using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bixa.Backend.Controllers.Services;

public class CustomBadRequest : ValidationProblemDetails
{
    public CustomBadRequest(ActionContext context)
    {
        Title = "Invalid arguments to the API";
        Detail = "The inputs supplied to the API are invalid";
        Status = 400;
        Type = context.HttpContext.TraceIdentifier;
        Errors = context.ModelState
            .Where(kvp => kvp.Value != null && kvp.Value.Errors.Any())
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors?.Select(GetErrorMessage).ToArray() ?? Array.Empty<string>()
            );
    }

    private string GetErrorMessage(ModelError error)
    {
        return string.IsNullOrEmpty(error.ErrorMessage) ?
               "The input was not valid." :
               error.ErrorMessage;
    }
}