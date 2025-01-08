namespace CryptoBank_WebApi.Errors.Codes;

public static class CommonValidationErrors
{
    public const string EmailRequired = "email_required";
    public const string EmailInvalid = "email_invalid";
    public const string EmailDoesNotExist = "email_does_not_exist";
    public const string AmountLessOrEqualZero = "amount_less_or_equal_zero";
    public const string AccountNumberRequired = "account_number_required";
    public const string AccountNumberInvalid = "account_number_invalid";
    public const string AccountNumberDoesNotExist = "account_number_does_not_exist";
}