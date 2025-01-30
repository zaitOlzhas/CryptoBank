namespace CryptoBank_Tests.Errors.Contracts;

public class LogicConflictProblemDetailsContract
{
    public required string Title { get; init; }

    public required string Type { get; init; }

    public required string Detail { get; init; }

    public required int Status { get; init; }

    public required string TraceId { get; init; }
    public required string Errors { get; init; }
}