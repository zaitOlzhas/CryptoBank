namespace CryptoBank_WebApi.Errors.Exceptions;

public class ValidationErrorsException : Exception
{
    public ValidationErrorsException(IEnumerable<RequestValidationError> errors) : base(
        "One or more validation errors have occurred")
    {
        Errors = errors;
    }

    private ValidationErrorsException(params RequestValidationError[] errors) : this(
        (IEnumerable<RequestValidationError>)errors)
    {
    }

    public ValidationErrorsException(string field, string message, string code) : this(
        new RequestValidationError(field, message, code))
    {
    }

    public IEnumerable<RequestValidationError> Errors { get; }
}

public record RequestValidationError(string Field, string Message, string Code);