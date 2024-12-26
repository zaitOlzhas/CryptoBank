namespace CryptoBank_WebApi.Features.Account.Domain;

public class MoneyTransaction
{
    public int Id { get; set; }
    public required string SourceAccount { get; set; }
    public required string DestinationAccount { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
}