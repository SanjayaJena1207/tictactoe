namespace TicTacToe.Application;

public enum ServiceErrorReason
{
    NotFound,
    ValidationFailed
}

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ServiceErrorReason? ErrorReason { get; }

    private ServiceResult(bool isSuccess, T? value, string? errorMessage, ServiceErrorReason? errorReason)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorReason = errorReason;
    }

    public static ServiceResult<T> Success(T value) => new(true, value, null, null);

    public static ServiceResult<T> Failure(string errorMessage, ServiceErrorReason reason) =>
        new(false, default, errorMessage, reason);
}
