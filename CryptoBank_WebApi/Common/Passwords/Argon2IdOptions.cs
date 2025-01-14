namespace CryptoBank_WebApi.Common.Passwords;

public class Argon2IdOptions
{
    public int PasswordHashSizeInBytes { get; init; }
    public int SaltSizeInBytes { get; init; }
    public int DegreeOfParallelism { get; init; }
    public int MemorySize { get; init; }
    public int Iterations { get; init; }
}