namespace NutriAI.Application.Common;

public class ServiceResult
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];

    public static ServiceResult Success(string? message = null) =>
        new() { Succeeded = true, Message = message };

    public static ServiceResult Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };

    public static ServiceResult Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Success(T data, string? message = null) =>
        new() { Succeeded = true, Data = data, Message = message };

    public new static ServiceResult<T> Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };
}
