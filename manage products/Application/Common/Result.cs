namespace ManageProducts.Application.Common;

public sealed record Result
{
    private Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public static Result Success() => new(true);

    public static Result Failure(string error) => new(false, error);
}

public sealed record Result<T>
{
    private Result(T? value, bool isSuccess, string? error = null)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public T? Value { get; }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public static Result<T> Success(T value) => new(value, true);

    public static Result<T> Failure(string error) => new(default, false, error);
}

