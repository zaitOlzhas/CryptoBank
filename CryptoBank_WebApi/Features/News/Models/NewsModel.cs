namespace CryptoBank_WebApi.Features.News.Models;

public class NewsModel
{
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; }
    public string Text { get; set; }
    public static NewsModel[] GenerateMockNews()
    {
        var news1 = new NewsModel { Title = "Title1", Date = DateTime.Now, Author = "Author1", Text = "Text1" };
        var news2 = new NewsModel { Title = "Title2", Date = DateTime.Now, Author = "Author1", Text = "Text2" };
        var news3 = new NewsModel { Title = "Title3", Date = DateTime.Now, Author = "Author1", Text = "Text3" };

        return [news1, news2, news3];
    }
}