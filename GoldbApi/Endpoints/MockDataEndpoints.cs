using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldbApi.Endpoints;

public static class MockDataEndpoints
{

    public static void MapMockDataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/roles", async (IMockDataService mockDataService) =>
        {
            var result = await mockDataService.GetRolesAsync();
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapGet("/routes", async (IAuthService authService) =>
        {
            var result = await authService.GetMenusAsync();
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapGet("/search/user", async (string? name, IMockDataService mockDataService) =>
        {
            var result = await mockDataService.SearchUsersAsync(name);
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapGet("/transaction/list", async (IMockDataService mockDataService) =>
        {
            var result = await mockDataService.GetTransactionsAsync();
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapPost("/role", () => Results.Ok(ApiResponse<object>.Success(new { key = new Random().Next(300, 5000) }))).RequireAuthorization();
        app.MapGet("/role/getUserInfo", () => Results.Ok(ApiResponse<object>.Success(new { status = "success" }))).RequireAuthorization();
        app.MapPost("/role/deleteUser", () => Results.Ok(ApiResponse<object>.Success(new { status = "success" }))).RequireAuthorization();
    }
}
