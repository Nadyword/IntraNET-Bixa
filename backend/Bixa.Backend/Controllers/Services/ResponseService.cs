using Bixa.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Bixa.Backend.Controllers.Services;

public class ResponseService
{
    public IActionResult CreateResponse<T>(ApiResponse<T> apiResponse, HttpStatusCode? statusCode = null)
    {
        if (statusCode.HasValue)
            apiResponse.StatusCode = (int)statusCode.Value;
        return new ObjectResult(apiResponse) { StatusCode = apiResponse.StatusCode };
    }
}