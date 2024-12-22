using System.Security.Claims;
using CryptoBank_WebApi.Database;
using CryptoBank_WebApi.Features.Account.Domain;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank_WebApi.Features.Account.Requests;

public class MoneyTransfer
{
    [HttpPost("/money-transfer")]
    [Authorize]
    public class Endpoint(IMediator mediator, IHttpContextAccessor contextAccessor): Endpoint<EndPointRequest, Response>
    {
        public override async Task<Response> ExecuteAsync(EndPointRequest request, CancellationToken ct)
        {
            var principal = contextAccessor.HttpContext.User;
            var cqrsRequest = new Request
            {
                SourceAccountNumber = request.SourceAccountNumber,
                DestinationAccountNumber = request.DestinationAccountNumber,
                Amount = request.Amount,
                Principal = principal
            };
            return await mediator.Send(cqrsRequest, ct);
        }
    }
    public record EndPointRequest
    {
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
    public record Request: IRequest<Response>
    {
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public ClaimsPrincipal Principal { get; set; }
    }
    public record Response;
    public class RequestHandler(CryptoBank_DbContext dbContext) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            if (!request.Principal.HasClaim(x => x.Type == ClaimTypes.Email))
                throw new Exception("Authorized user has not Email claim");
            var claims = request.Principal.Claims.ToList();
            var email = claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = await dbContext.Users
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync(cancellationToken);
            if (user is null)
                throw new Exception("User not found");
            
            var sourceAccount = await dbContext.Accounts.FindAsync(request.SourceAccountNumber);
            if (sourceAccount is null)
                throw new Exception("Source account not found");
            
            if(sourceAccount.UserId!=user.Id)
                throw new Exception("You are not the owner of the source account");
            
            var destinationAccount = await dbContext.Accounts.FindAsync(request.DestinationAccountNumber);
            if (destinationAccount is null)
                throw new Exception("Destination account not found");
            
            if (sourceAccount.Amount < request.Amount)
                throw new Exception("Insufficient funds");
            if(sourceAccount.Currency != destinationAccount.Currency)
                throw new Exception("Currency mismatch");
            
            sourceAccount.Amount -= request.Amount;
            destinationAccount.Amount += request.Amount;

            var newTransaction = new MoneyTransaction
            {
                SourceAccount = sourceAccount.Number,
                DestinationAccount = destinationAccount.Number,
                Amount = request.Amount,
                CreatedOn = DateTime.UtcNow
            };
            
            await dbContext.MoneyTransactions.AddAsync(newTransaction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return new Response();
        }
    }
}