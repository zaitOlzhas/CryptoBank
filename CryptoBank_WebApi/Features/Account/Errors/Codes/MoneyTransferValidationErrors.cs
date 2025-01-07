namespace CryptoBank_WebApi.Features.Account.Errors.Codes;

public static class MoneyTransferValidationErrors
{
    private const string Prefix = "money_transfer_validation_";

    public const string EmailRequired = Prefix + "email_required";
    public const string EmailInvalid = Prefix + "email_invalid";
    public const string AmountLessOrEqualZero = Prefix + "amount_less_or_equal_zero";
    public const string SourceAccountNumberRequired = Prefix + "source_account_number_required";
    public const string SourceAccountNumberInvalid = Prefix + "source_account_number_invalid";
    public const string DestinationAccountNumberRequired = Prefix + "destination_account_number_required";
    public const string DestinationAccountNumberInvalid = Prefix + "destination_account_number_invalid";
}