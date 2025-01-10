using System.Reflection;
using System.Text;
using CryptoBank_WebApi.Authorization;
using CryptoBank_WebApi.Authorization.Requirements;
using CryptoBank_WebApi.Common.Passwords;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Errors.Extensions;
using CryptoBank_WebApi.Features.Account.Configurations;
using CryptoBank_WebApi.Features.Auth.Common;
using CryptoBank_WebApi.Features.Auth.Configurations;
using CryptoBank_WebApi.Features.Auth.Domain;
using CryptoBank_WebApi.Features.News.Configurations;
using CryptoBank_WebApi.Pipeline;
using CryptoBank_WebApi.Pipeline.Behaviors;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddDbContext<CryptoBank_DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CryptoBank_DbContext")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection("Features:Auth").Get<AuthConfigurations>()!.Jwt;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
    };
});
builder.Services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.UserRole, policy => policy.AddRequirements(new RoleRequirement(UserRole.User)));
    options.AddPolicy(PolicyNames.AnalystRole, policy => policy.AddRequirements(new RoleRequirement(UserRole.Analyst)));
    options.AddPolicy(PolicyNames.AdministratorRole,
        policy => policy.AddRequirements(new RoleRequirement(UserRole.Administrator)));
});

builder.Services.AddSingleton<Dispatcher>();

builder.Services.AddFastEndpoints();
builder.Services.Configure<NewsConfigurations>(builder.Configuration.GetSection("Features:News"));
builder.Services.Configure<AuthConfigurations>(builder.Configuration.GetSection("Features:Auth"));
builder.Services.Configure<AccountConfigurations>(builder.Configuration.GetSection("Features:Account"));
builder.Services.AddTransient<Argon2IdPasswordHasher>();
builder.Services.AddTransient<TokenGenerator>();
builder.Services.Configure<Argon2IdOptions>(builder.Configuration.GetSection("Common:Passwords:Argon2Id"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.MapProblemDetailsComplete();
app.MapGet("/health", async (context) =>
{
    context.Response.Headers.Append("Content-Type", new StringValues("text/plain; charset=UTF-8"));
    await context.Response.Body.WriteAsync("OK"u8.ToArray());
});
app.UseAuthentication();
app.UseAuthorization();
app.MapFastEndpoints();
app.MapGet("/dictionary/user-roles", () => Enum.GetNames<UserRole>());
app.Run();