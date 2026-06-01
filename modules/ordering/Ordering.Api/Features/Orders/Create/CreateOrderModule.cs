using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering.Api.Features.Orders.Create
{
    public class CreateOrderModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/orders", async (
                [FromBody] CreateOrderRequest request,
                [FromServices] ISender sender,
                IValidator<CreateOrderRequest> validator,
                CancellationToken ct) =>
            {
                var validationResult = await validator.ValidateAsync(request, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var orderId = await sender.Send(new CreateOrderCommand(request.ProductId, request.Quantity), ct);
                return Results.Created(string.Empty, orderId);
            });
        }
    }
}
