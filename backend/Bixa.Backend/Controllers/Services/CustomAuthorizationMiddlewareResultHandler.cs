using Bixa.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bixa.Backend.Controllers.Services
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult == null)
            {
                await _defaultHandler.HandleAsync(next, context, policy, authorizeResult!);
                return;
            }

            if (authorizeResult.Forbidden)
            {
                var allowedRoles = policy?.Requirements?
                    .SelectMany(r =>
                    {
                        var prop = r.GetType().GetProperty("AllowedRoles");
                        if (prop == null) return Enumerable.Empty<string>();
                        var value = prop.GetValue(r) as System.Collections.IEnumerable;
                        if (value == null) return Enumerable.Empty<string>();
                        return value.Cast<object>().Select(o => o?.ToString() ?? string.Empty);
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToArray()
                    ?? System.Array.Empty<string>();

                string message = allowedRoles.Length > 0
                    ? $"Acceso denegado: se requiere uno de los roles: {string.Join(", ", allowedRoles)}."
                    : "Acceso denegado: no tiene permisos suficientes para acceder a este recurso.";

                var apiResponse = ApiResponse<object>.Forbidden(new object(), message);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var responseService = context.RequestServices.GetService<ResponseService>() ?? new ResponseService();
                var formatted = responseService.CreateResponse(apiResponse) as ObjectResult;

                var serializerOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
                var payload = JsonSerializer.Serialize(formatted?.Value, serializerOptions);

                await context.Response.WriteAsync(payload ?? string.Empty);
                return;
            }

            if (authorizeResult.Challenged)
            {
                var apiResponse = ApiResponse<object>.UnauthorizedResponse("Token inválido o expirado. Acceso denegado.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var responseService = context.RequestServices.GetService<ResponseService>() ?? new ResponseService();
                var formatted = responseService.CreateResponse(apiResponse) as ObjectResult;

                var serializerOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
                var payload = JsonSerializer.Serialize(formatted?.Value, serializerOptions);

                await context.Response.WriteAsync(payload ?? string.Empty);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}