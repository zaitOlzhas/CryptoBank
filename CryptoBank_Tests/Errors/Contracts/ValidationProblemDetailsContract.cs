namespace CryptoBank_Tests.Errors.Contracts;


public class ValidationProblemDetailsContract
{
    public required string Title { get; init; }

    public required string Type { get; init; }

    public required string Detail { get; init; }

    public required int Status { get; init; }

    public required string TraceId { get; init; }

    public required ErrorDataContract[] Errors { get; init; }
}

public class ErrorDataContract
{
    public required string Field { get; init; }

    public required string Message { get; init; }

    public required string Code { get; init; }
}
