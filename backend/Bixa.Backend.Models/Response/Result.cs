using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.Response;

public class Result
{
    protected Result(bool isSuccess, string error, ErrorTypeEnum errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorTypeEnum = errorType;
    }

    public bool IsSuccess { get; }
    public string Error { get; } = string.Empty;
    public ErrorTypeEnum ErrorTypeEnum { get; } = ErrorTypeEnum.None;

    public static Result Success() => new(true, string.Empty, ErrorTypeEnum.None);

    public static Result<T> Success<T>(T value) => new(value, true, string.Empty, ErrorTypeEnum.None);

    public static Result Fail(string error, ErrorTypeEnum errorType = ErrorTypeEnum.General) => new(false, error, errorType);

    public static Result<T> Fail<T>(string error, ErrorTypeEnum errorType = ErrorTypeEnum.General) => new(default!, false, error, errorType);
}

public class Result<T> : Result
{
    internal Result(T value, bool isSuccess, string error, ErrorTypeEnum errorType)
        : base(isSuccess, error, errorType)
    {
        Value = value;
    }

    public T Value { get; }
}