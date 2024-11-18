namespace CryptoBank_WebApi.Features.News.Domain;

public class News
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; }
    public string Text { get; set; }
}