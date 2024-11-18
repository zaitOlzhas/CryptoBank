using System.Reflection;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Pipeline;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CryptoBank_DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CryptoBank_DbContext")));

builder.Services.AddSingleton<Dispatcher>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddFastEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  
}

app.MapGet("/health", async (context) =>
{
    context.Response.Headers.Append("Content-Type", new StringValues("text/plain; charset=UTF-8"));
    await context.Response.Body.WriteAsync("OK"u8.ToArray());
});
app.MapFastEndpoints();
app.Run();
