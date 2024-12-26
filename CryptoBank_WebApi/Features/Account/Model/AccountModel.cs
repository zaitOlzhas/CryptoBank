namespace CryptoBank_WebApi.Features.Account.Model;

public class AccountModel
{
    public required string Number { get; set; }
    public required string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UserId { get; set; }
}