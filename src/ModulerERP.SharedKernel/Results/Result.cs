namespace ModulerERP.SharedKernel.Results;

/// <summary>
/// Result pattern for operation outcomes - avoids exceptions for expected failures
/// </summary>
/// <typeparam name="T">The type of the value on success</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public List<string> Errors { get; } = new();

    protected Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        if (error != null)
            Errors.Add(error);
    }

    protected Result(bool isSuccess, T? value, List<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
        Error = errors.FirstOrDefault();
    }

    public static Result<T> Success(T value) => new(true, value, (string?)null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> Failure(List<string> errors) => new(false, default, errors);

    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Non-generic result for void operations
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public List<string> Errors { get; } = new();

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
        if (error != null)
            Errors.Add(error);
    }

    protected Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Error = errors.FirstOrDefault();
    }

    public static Result Success() => new(true, (string?)null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(List<string> errors) => new(false, errors);
}
