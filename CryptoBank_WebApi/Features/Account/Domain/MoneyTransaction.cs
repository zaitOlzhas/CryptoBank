namespace CryptoBank_WebApi.Features.Account.Domain;

public class MoneyTransaction
{
    public int Id { get; set; }
    public string SourceAccount { get; set; }
    public string DestinationAccount { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
}