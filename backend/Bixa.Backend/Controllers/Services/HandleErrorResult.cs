using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Controllers.Services;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;

public class HandleError : ControllerBase
{
    private readonly ResponseService _responseService;

    public HandleError()
    {
        _responseService = new ResponseService();
    }

    public IActionResult HandleErrorResult(Result result)
    {
        return result.ErrorTypeEnum switch
        {
            ErrorTypeEnum.NotFound => _responseService.CreateResponse(ApiResponse<object>.NotFoundResponse(result.Error)),
            ErrorTypeEnum.Conflict => StatusCode(409, ApiResponse<object>.ConflictResponse(result.Error)), // HTTP 409
            ErrorTypeEnum.BadRequest => BadRequest(ApiResponse<object>.BadRequest(new object(), result.Error)), // HTTP 400
            ErrorTypeEnum.Unauthorized => Unauthorized(ApiResponse<object>.UnauthorizedResponse(result.Error)), // HTTP 401
            ErrorTypeEnum.Database => StatusCode(500, ApiResponse<object>.ErrorResponse(result.Error)), // HTTP 500
            ErrorTypeEnum.Validation => BadRequest(ApiResponse<object>.BadRequest(null, result.Error)), // HTTP 400
            _ => StatusCode(500, ApiResponse<object>.ErrorResponse(result.Error)) // Error general
        };
    }
}