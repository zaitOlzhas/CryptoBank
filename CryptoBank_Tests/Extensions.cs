using System.Text.Json;
using CryptoBank_WebApi.Database;
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
                Email = "user3@admin.com",
                Password = "123",
                Role = "user",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20).Date),
                RegistrationDate = DateTime.UtcNow,
                FirstName = "User",
                LastName = "Admin"

            });

            context.SaveChanges();
            return context;
        }
    }

    public static async Task<T> DeserializeContent<T>(this HttpResponseMessage message) =>
        await JsonSerializer.DeserializeAsync<T>(await message.Content.ReadAsStreamAsync().ConfigureAwait(false)).ConfigureAwait(false);
}