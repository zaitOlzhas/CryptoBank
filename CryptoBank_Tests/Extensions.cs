using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Account.Domain;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.News.Domain;

namespace CryptoBank_Tests;

public static class Extensions
{
    private static object _customerContextLock = new object();
    public static CryptoBank_DbContext InitializeTestDatabase(this CryptoBank_DbContext context)
    {
        lock (_customerContextLock)
        {
            context.News.Add(new News
            {
                Id = 1,
                Title = "TEST 1",
                Date = DateTime.Now,
                Author = "Author 1",
                Text = "Text 1"
            });

            context.News.Add(new News
            {
                Id = 2,
                Title = "TEST 2",
                Date = DateTime.Now,
                Author = "Author 1",
                Text = "Text 2"
            });

            context.News.Add(new News
            {
                Id = 3,
                Title = "TEST 3",
                Date = DateTime.Now,
                Author = "Author 1",
                Text = "Text 3"
            });

            context.Users.Add(new User
            {
                Id = 1,
                Email = "user1@admin.com",
                Password = "123",
                Role = "user",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });

            context.Users.Add(new User
            {
                Id = 2,
                Email = "user2@admin.com",
                Password = "123",
                Role = "user",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });
            context.Users.Add(new User
            {
                Id = 3,
                Email = "analyst@admin.com",
                Password = "123",
                Role = "Analyst",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "Analyst",
                LastName = "Admin"

            });
            context.Users.Add(new User
            {
                Id = 4,
                Email = "user3@admin.com",
                Password = "123",
                Role = "user",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });
            context.Users.Add(new User
            {
                Id = 5,
                Email = "admin@admin.com",
                Password = "argon2id$m=8192$i=40$p=16$$aLob6R7ZvWwMFJxCg0hl4uWhkmjWnDAhxWemg9ATW2QvTMIp86LB8r+XdB/aoeujPG+9rUccCKgQgXA4ixxMzw==",
                Role = "Administrator",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b43",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow.AddMonths(-1)
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b49",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow.AddMonths(-1)
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b48",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow.AddMonths(-1)
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b47",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow.AddDays(-20)
            });
            context.Accounts.Add(new Account
            {
                Number = "f78d6b07-2d33-4443-91f1-d679a2b80b46",
                Currency = "USD",
                UserId = 2,
                Amount = 1,
                CreatedOn = DateTime.UtcNow.AddDays(-20)
            });
            context.Accounts.Add(new Account
            {
                Number = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da8a",
                Currency = "USD",
                UserId = 1,
                Amount = 98,
                CreatedOn = DateTime.UtcNow.AddDays(-21)
            });
            
            
            context.Accounts.Add(new Account
            {
                Number = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da83",
                Currency = "USD",
                UserId = 1,
                Amount = 98,
                CreatedOn = DateTime.UtcNow.AddDays(-21)
            });
            context.Accounts.Add( new Account
            {
                Number = "7b6e4a4b-f0fe-4cea-8111-8cf504a7da84",
                Currency = "USD",
                UserId = 3,
                Amount = 1,
                CreatedOn = DateTime.UtcNow.AddDays(-21)
            });
            context.UserRefreshTokens.Add(new UserRefreshToken
            {
                Id = 1,
                Token = "eededyJD9w8icqFkiVFtn7Kt+DwFLrEEJ1OoSfTqkn0=",
                ExpiryDate = DateTime.UtcNow.AddMonths(1),
                UserId = 5
            });
            
            context.SaveChanges();
            return context;
        }
    }
}