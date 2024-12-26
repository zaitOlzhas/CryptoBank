using CryptoBank_WebApi.Authorization;
using CryptoBank_WebApi.Database;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class GetAccountReport
{
    [HttpGet("/accounts-report")]
    [Authorize(Policy = PolicyNames.AnalystRole)]
    public class Endpoint(IMediator mediator) : Endpoint<Request, string[]>
    {
        public override Task<string[]> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            return mediator.Send(request, cancellationToken);
        }
    }

    public record Request : IRequest<string[]>
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<Request, string[]>
    {
        public async Task<string[]> Handle(Request request, CancellationToken cancellationToken)
        {
            var startDate = request.StartDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
            var endDate = request.EndDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();

            var accountsByDay = await dbContext.Accounts
                .Where(a => a.CreatedOn >= startDate && a.CreatedOn <= endDate)
                .GroupBy(a => a.CreatedOn.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var reportRows = accountsByDay
                .Select(a => $"{a.Date.ToShortDateString()} - {a.Count} accounts")
                .ToArray();

            return reportRows;
        }
    }
}