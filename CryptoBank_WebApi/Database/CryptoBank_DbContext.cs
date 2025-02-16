using CryptoBank_WebApi.Features.Account.Domain;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.News.Domain;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Database;

public class CryptoBank_DbContext : DbContext
{
    public DbSet<News> News { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<MoneyTransaction> MoneyTransactions { get; set; }

    public CryptoBank_DbContext(DbContextOptions<CryptoBank_DbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MapNews(modelBuilder);
        MapUser(modelBuilder);
        MapUserRefreshToken(modelBuilder);
        MapAccount(modelBuilder);
        MapMoneyTransaction(modelBuilder);
    }

    private void MapMoneyTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MoneyTransaction>(moneyTransaction =>
        {
            moneyTransaction.HasKey(x => x.Id);
            moneyTransaction.Property(x => x.SourceAccount)
                .IsRequired();
            moneyTransaction.Property(x => x.DestinationAccount)
                .IsRequired();
            moneyTransaction.Property(x => x.Amount)
                .IsRequired();
            moneyTransaction.Property(x => x.CreatedOn)
                .HasDefaultValueSql("NOW()")
                .IsRequired();
        });
    }

    private void MapAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(account =>
        {
            account.HasKey(x => x.Number);
            account.Property(x => x.Number)
                .HasMaxLength(36)
                .ValueGeneratedOnAdd();

            account.Property(x => x.Currency)
                .IsRequired();

            account.Property(x => x.Amount)
                .HasDefaultValue(0);

            account.Property(x => x.CreatedOn)
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            account.Property(x => x.UserId)
                .IsRequired();
            account.HasIndex(x => x.UserId);
        });
    }

    private void MapUserRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRefreshToken>(userRefreshToken =>
        {
            userRefreshToken.HasKey(x => x.Id);
            userRefreshToken.Property(x => x.Token);
            userRefreshToken.HasIndex(x => x.Token);

            userRefreshToken.Property(x => x.ExpiryDate)
                .IsRequired();

            userRefreshToken.Property(x => x.UserId)
                .IsRequired();
        });
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
                .IsRequired(false);

            user.Property(x => x.LastName)
                .IsRequired(false);
        });
    }
}