using System.Security.Claims;
using FluentValidation;

using static CryptoBank_WebApi.Features.Account.Errors.Codes.MoneyTransferValidationErrors;
namespace CryptoBank_WebApi.Validation;


public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, string> ValidSourceAccountNumber<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty().WithErrorCode(SourceAccountNumberRequired)
            .Must(da=>da.Length.Equals(36)).WithErrorCode(SourceAccountNumberInvalid);
    }
    public static IRuleBuilderOptions<T, string> ValidDestinationAccountNumber<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty().WithErrorCode(DestinationAccountNumberRequired)
            .Must(da=>da.Length.Equals(36)).WithErrorCode(DestinationAccountNumberInvalid);
    }
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty().WithErrorCode(EmailRequired)
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithErrorCode(EmailInvalid);
    }
}