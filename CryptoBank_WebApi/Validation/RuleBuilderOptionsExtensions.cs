using System.Security.Claims;
using CryptoBank_WebApi.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using static CryptoBank_WebApi.Errors.Codes.CommonValidationErrors;

namespace CryptoBank_WebApi.Validation;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, string> ValidateAccountNumber<T>(this IRuleBuilder<T, string> builder,
        string prefix,
        CryptoBank_DbContext context)
    {
        return builder
            .NotEmpty().WithErrorCode(prefix + AccountNumberRequired)
            .Must(da => da.Length.Equals(36)).WithErrorCode(prefix + AccountNumberInvalid)
            .MustAsync(async (da, ct) => await context.Accounts.AnyAsync(x => x.Number == da, ct))
            .WithErrorCode(prefix + AccountNumberDoesNotExist);
    }

    public static IRuleBuilderOptions<T, string?> ValidateEmail<T>(this IRuleBuilder<T, string?> builder,
        string prefix,
        CryptoBank_DbContext context)
    {
        return builder
            .NotEmpty().WithErrorCode(prefix + EmailRequired)
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithErrorCode(prefix + EmailInvalid)
            .MustAsync(async (email, ct) => await context.Users.AnyAsync(x => x.Email == email, ct))
            .WithErrorCode(prefix + EmailDoesNotExist);
    }

    public static IRuleBuilderOptions<T, decimal> ValidateAmount<T>(this IRuleBuilder<T, decimal> builder,
        string prefix)
    {
        return builder
            .GreaterThan(0)
            .WithErrorCode(prefix + AmountLessOrEqualZero);
    }
}