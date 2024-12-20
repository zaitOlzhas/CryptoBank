namespace CryptoBank_WebApi.Features.Account.Domain;

public class Account
{
    public string Number { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UserId { get; set; }
    
}