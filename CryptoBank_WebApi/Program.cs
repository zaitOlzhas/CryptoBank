using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  
}

//app.UseRouting();


app.MapGet("/health", async (context) =>
{
    context.Response.Headers.Append("Content-Type", new StringValues("text/plain; charset=UTF-8"));
    await context.Response.Body.WriteAsync("OK"u8.ToArray());
});

app.Run();
