namespace CryptoBank_WebApi.Errors.Exceptions;

public class LogicConflictException : Exception
{
    public LogicConflictException(string message, string code) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
