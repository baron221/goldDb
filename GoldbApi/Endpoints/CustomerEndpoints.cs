using FluentValidation;
using GoldbApi.DTOs;
using GoldbApi.Services;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class CustomerEndpoints
{

    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/customer").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] CustomerListRequest request, ICustomerService customerService) =>
        {
            var result = await customerService.GetCustomersAsync(request);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, ICustomerService customerService) =>
        {
            var result = await customerService.GetCustomerByIdAsync(id);
            return Results.Ok(result);
        });

        group.MapPost("/", async (CustomerCreateRequest request, ICustomerService customerService) =>
        {
            var result = await customerService.CreateCustomerAsync(request);
            return Results.Ok(result);
        }).WithValidation<CustomerCreateRequest>();

        group.MapPut("/{id:int}", async (int id, CustomerUpdateRequest request, ICustomerService customerService) =>
        {
            var result = await customerService.UpdateCustomerAsync(id, request);
            return Results.Ok(result);
        }).WithValidation<CustomerUpdateRequest>();

        group.MapDelete("/{id:int}", async (int id, ICustomerService customerService) =>
        {
            var result = await customerService.DeleteCustomerAsync(id);
            return Results.Ok(result);
        });
    }
}
