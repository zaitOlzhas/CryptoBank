namespace CryptoBank_WebApi.Features.Account.Errors.Codes;

public static class CreateAccountValidationErrors
{
    private const string Prefix = "create_account_validation_";

    public const string EmailRequired = Prefix + "email_required";
    public const string EmailInvalid = Prefix + "email_invalid";
}