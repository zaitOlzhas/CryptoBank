using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using CryptoBank_WebApi.Features.News.Models;

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
        public async Task<NewsModel[]> Handle(Request request, CancellationToken cancellationToken) =>
            await Task.FromResult(NewsModel.GenerateMockNews());
        
    }
}