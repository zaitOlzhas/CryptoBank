namespace CryptoBank_WebApi.Features.Account.Model;

public class AccountModel
{
    public string Number { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UserId { get; set; }

    public AccountModel(string number, string currency, decimal amount, DateTime createdOn, int userId)
    {
        Number = number;
        Currency = currency;
        Amount = amount;
        CreatedOn = createdOn;
        UserId = userId;
    }
}