using CryptoBank_WebApi.Features.News.Domain;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Database;

public class CryptoBank_DbContext : DbContext
{
    public DbSet<News> News { get; set; }

    public CryptoBank_DbContext(DbContextOptions<CryptoBank_DbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MapNews(modelBuilder);
    }
    private void MapNews(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<News>(news =>
        {
            news.HasKey(x => x.Id);

            news.Property(x => x.Title)
                .IsRequired();

            news.Property(x => x.Date)
                .IsRequired();

            news.Property(x => x.Author)
                .IsRequired();

            news.Property(x => x.Text)
                .IsRequired();
        });
    }
}