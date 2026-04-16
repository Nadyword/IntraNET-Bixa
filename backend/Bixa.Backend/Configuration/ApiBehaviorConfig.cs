using Bixa.Backend.Controllers.Services;
using Bixa.Backend.Models;

namespace Bixa.Backend.Configuration;

public static class ApiBehaviorConfig
{
    public static void ConfigureApiBehavior(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                CustomBadRequest problems = new CustomBadRequest(context);
                return new ResponseService().CreateResponse(ApiResponse<object>.BadRequest(problems, problems?.Title ?? "Invalid Model State"));
            };
        });
    }
}