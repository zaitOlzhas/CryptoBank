using CryptoBank_WebApi.Database;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using CryptoBank_WebApi.Features.News.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Features.News.Requests;

public static class GetNews
{
    [HttpGet("/news")]
    [AllowAnonymous]
    public class Endpoint(IMediator mediator) : Endpoint<Request, NewsModel[]>
    {
        public override async Task<NewsModel[]> ExecuteAsync(Request request, CancellationToken cancellationToken) =>
            await mediator.Send(request, cancellationToken);
    }

    public record Request(int temp) : IRequest<NewsModel[]>;

    public class RequestHandler(CryptoBank_DbContext dbContext, IConfiguration configuration)
        : IRequestHandler<Request, NewsModel[]>
    {
        public async Task<NewsModel[]> Handle(Request request, CancellationToken cancellationToken) {
            
            int newsLimit = configuration.GetValue<int>("Features:News:NewsLimit");
            
            return await dbContext.News
                .Select(x => new NewsModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Date = x.Date,
                    Author = x.Author,
                    Text = x.Text
                })
                .OrderByDescending(x => x.Date)
                .Take(newsLimit)
                .ToArrayAsync(cancellationToken);
        }
    }
}