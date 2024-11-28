using CryptoBank_WebApi.Authorization;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.News.Configurations;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using CryptoBank_WebApi.Features.News.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank_WebApi.Features.News.Requests;

public static class GetNews
{
    [HttpGet("/usernews")]
    [Authorize(Policy = PolicyNames.UserRole)]
    public class Endpoint(IMediator mediator) : EndpointWithoutRequest<NewsModel[]>
    {
        public override async Task<NewsModel[]> ExecuteAsync(CancellationToken cancellationToken)
        {
            var request = new GetNewsRequest();
            var response = await mediator.Send(request, cancellationToken);
            return response;
        }
    }
    [HttpGet("/adminnews")]
    [Authorize(Policy = PolicyNames.AdministratorRole)]
    public class Endpoint2(IMediator mediator) : EndpointWithoutRequest<NewsModel[]>
    {
        public override async Task<NewsModel[]> ExecuteAsync(CancellationToken cancellationToken)
        {
            var request = new GetNewsRequest();
            var response = await mediator.Send(request, cancellationToken);
            return response;
        }
    }
    [HttpGet("/allnews")]
    [AllowAnonymous]
    public class Endpoint3(IMediator mediator) : EndpointWithoutRequest<NewsModel[]>
    {
        public override async Task<NewsModel[]> ExecuteAsync(CancellationToken cancellationToken)
        {
            var request = new GetNewsRequest();
            var response = await mediator.Send(request, cancellationToken);
            return response;
        }
    }

    public record GetNewsRequest : IRequest<NewsModel[]>;

    public class RequestHandler(CryptoBank_DbContext dbContext, IOptions<NewsConfigurations> configs)
        : IRequestHandler<GetNewsRequest, NewsModel[]>
    {
        public async Task<NewsModel[]> Handle(GetNewsRequest request, CancellationToken cancellationToken) {
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
                .Take(configs.Value.NewsLimit)
                .ToArrayAsync(cancellationToken);
        }
    }
}