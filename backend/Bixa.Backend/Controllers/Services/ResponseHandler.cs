using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Controllers.Services;

public static class ResponseHandler
{
    public static ApiResponse GetAppResponse(ResponseTypeEnum type, object? contract)
    {
        ApiResponse response;

        response = new ApiResponse { ResponseData = contract };
        switch (type)
        {
            case ResponseTypeEnum.Success:
                response.Message = "Success";
                break;

            case ResponseTypeEnum.NotFound:
                response.Message = "No record available";
                break;

            case ResponseTypeEnum.Failure:
                response.Message = "Operation rejected";
                break;
        }
        return response;
    }

    public static ApiResponse GetExceptionResponse(Exception ex)
    {
        ApiResponse response = new ApiResponse();
        response.Message = ex.Message;
        return response;
    }
}