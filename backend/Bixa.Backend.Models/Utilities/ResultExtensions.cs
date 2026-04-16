using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Models.Utilities;

public static class ResultExtensions
{
    public static T Match<T>(this Result result, Func<T> onSuccess, Func<Result, T> onError) =>
        result.IsSuccess ? onSuccess() : onError(result);

    public static T Match<T, U>(this Result<U> result, Func<U, T> onSuccess, Func<Result<U>, T> onError) =>
        result.IsSuccess ? onSuccess(result.Value) : onError(result);
}