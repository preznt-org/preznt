namespace Preznt.Core.Common;

public sealed class Result<T>
{
    public bool IsSuccess {get; }
    public bool IsFailure => !IsSuccess;
    public T? Value {get; }
    public ResultError? Error {get; }

    private Result(bool isSuccess, T? value, ResultError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(ResultError error) => new(false, default, error);
    
    public static Result<T> NotFound(string resource, string id) 
        => Failure(new ResultError(ErrorType.NotFound, $"{resource} '{id}' not found"));
    
    public static Result<T> Validation(string message) 
        => Failure(new ResultError(ErrorType.Validation, message));
    
    public static Result<T> Forbidden(string message) 
        => Failure(new ResultError(ErrorType.Forbidden, message));
    
    public static Result<T> Conflict(string message) 
        => Failure(new ResultError(ErrorType.Conflict, message));
}

public sealed record ResultError(ErrorType Type, string Message);

public enum ErrorType
{
    NotFound,
    Validation,
    Forbidden,
    Conflict,
    Unauthorized,
    InternalError
}