using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.News.Domain;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Database;

public class CryptoBank_DbContext : DbContext
{
    public DbSet<News> News { get; set; }
    public DbSet<User> Users { get; set; }

    public CryptoBank_DbContext(DbContextOptions<CryptoBank_DbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MapNews(modelBuilder);
        MapUser(modelBuilder);
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
    private void MapUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(x => x.Id);

            user.Property(x => x.Email)
                .IsRequired();

            user.Property(x => x.Password)
                .IsRequired();

            user.Property(x => x.Role)
                .IsRequired();
            
            user.Property(x => x.DateOfBirth)
                .IsRequired();
            
            user.Property(x => x.RegistrationDate)
                .IsRequired();
            
            user.Property(x => x.FirstName)
                .IsRequired();
            
            user.Property(x => x.LastName)
                .IsRequired();
        });
    }
}