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

    public class RequestHandler : IRequestHandler<Request, NewsModel[]>
    {
        private CryptoBank_DbContext _db;
        public RequestHandler(CryptoBank_DbContext db)
        {
            _db = db;
        }
        public async Task<NewsModel[]> Handle(Request request, CancellationToken cancellationToken) =>
            await _db.News
                .Select(x => new NewsModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Date = x.Date,
                    Author = x.Author,
                    Text = x.Text
                })
                .ToArrayAsync(cancellationToken);
        
    }
}