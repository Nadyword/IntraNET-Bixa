using System.Text.Json.Serialization;
using Bixa.Backend.Models;
using System.Text.Json;
using System.Net;

namespace Bixa.Backend.Controllers.Services;

public class CustomUnauthorizedMiddleware
{
    private readonly ILogger<CustomUnauthorizedMiddleware> _logger;
    private readonly RequestDelegate _next;

    public CustomUnauthorizedMiddleware(RequestDelegate next, ILogger<CustomUnauthorizedMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized || context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    var responseContent = FormatResponse(context.Response);
                    await context.Response.WriteAsync(responseContent);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception was caught by CustomUnauthorizedMiddleware.");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var apiResponse = ApiResponse<object>.ErrorResponse(
                    "An unexpected error occurred.",
                    StatusCodes.Status500InternalServerError);

                var responseService = new ResponseService();
                var formattedResponse = responseService.CreateResponse(apiResponse);

                var objectResult = formattedResponse as Microsoft.AspNetCore.Mvc.ObjectResult;
                if (objectResult != null)
                {
                    await context.Response.WriteAsync(JsonSerializer.Serialize(objectResult.Value));
                }
            }
        }
    }

    private string FormatResponse(HttpResponse response)
    {
        ApiResponse<object> apiResponse;

        if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            apiResponse = ApiResponse<object>.UnauthorizedResponse("You are not authorized to access this resource.");
        }
        else
        {
            apiResponse = ApiResponse<object>.Forbidden(new object(), "You do not have the necessary permissions to access this resource.");
        }

        var responseService = new ResponseService();
        var formattedResponse = responseService.CreateResponse(apiResponse);

        var objectResult = formattedResponse as Microsoft.AspNetCore.Mvc.ObjectResult;
        if (objectResult != null)
        {
            var serializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            return JsonSerializer.Serialize(objectResult.Value, serializerOptions);
        }

        return string.Empty;
    }
}