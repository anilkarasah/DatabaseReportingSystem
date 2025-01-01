namespace DatabaseReportingSystem.Shared;

public class Result
{
    public bool IsFailure { get; init; }

    public bool IsSuccess => !IsFailure;

    public string Error { get; set; } = string.Empty;

    public static Result Fail(string error = "")
    {
        return new Result
        {
            IsFailure = true,
            Error = error
        };
    }

    public static Result Ok()
    {
        return new Result
        {
            IsFailure = false
        };
    }
}

public class Result<T> : Result
{
    public T Value { get; set; } = default!;

    public new static Result<T> Fail(string error = "")
    {
        return new Result<T>
        {
            IsFailure = true,
            Error = error
        };
    }

    public static Result<T> Ok(T value)
    {
        return new Result<T>
        {
            IsFailure = false,
            Value = value
        };
    }
}
