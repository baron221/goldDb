using FluentValidation;
using GoldbApi.DTOs;
using GoldbApi.Services;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class CompanyEndpoints
{

    public static void MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] CompanyListRequest request, ICompanyService companyService) =>
        {
            var result = await companyService.GetCompaniesAsync(request);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, ICompanyService companyService) =>
        {
            var result = await companyService.GetCompanyByIdAsync(id);
            return Results.Ok(result);
        });

        group.MapPost("/", async (CompanyCreateRequest request, ICompanyService companyService) =>
        {
            var result = await companyService.CreateCompanyAsync(request);
            return Results.Ok(result);
        }).WithValidation<CompanyCreateRequest>();

        group.MapPut("/{id:int}", async (int id, CompanyUpdateRequest request, ICompanyService companyService) =>
        {
            var result = await companyService.UpdateCompanyAsync(id, request);
            return Results.Ok(result);
        }).WithValidation<CompanyUpdateRequest>();

        group.MapDelete("/{id:int}", async (int id, ICompanyService companyService) =>
        {
            var result = await companyService.DeleteCompanyAsync(id);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}/users", async (int id, ICompanyService companyService) =>
        {
            var result = await companyService.GetCompanyUsersAsync(id);
            return Results.Ok(result);
        });

        group.MapPost("/{id:int}/users/{userId:int}", async (int id, int userId, ICompanyService companyService) =>
        {
            var result = await companyService.AddUserToCompanyAsync(id, userId);
            return Results.Ok(result);
        });

        group.MapDelete("/{id:int}/users/{userId:int}", async (int id, int userId, ICompanyService companyService) =>
        {
            var result = await companyService.RemoveUserFromCompanyAsync(id, userId);
            return Results.Ok(result);
        });

        group.MapGet("/available-users", async (ICompanyService companyService) =>
        {
            var result = await companyService.GetAvailableUsersAsync();
            return Results.Ok(result);
        });

        group.MapGet("/logistics/{centerId:int}/retailers", async (int centerId, ICompanyService companyService) =>
        {
            var result = await companyService.GetRetailersByCenterAsync(centerId);
            return Results.Ok(result);
        });

        group.MapGet("/retailers/unassigned", async (ICompanyService companyService) =>
        {
            var result = await companyService.GetUnassignedRetailersAsync();
            return Results.Ok(result);
        });

        group.MapPost("/logistics/{centerId:int}/assign", async (int centerId, List<int> retailerIds, ICompanyService companyService) =>
        {
            var result = await companyService.AssignRetailersToCenterAsync(centerId, retailerIds);
            return Results.Ok(result);
        });

        group.MapGet("/logistics/{centerId:int}/manufacturers", async (int centerId, ICompanyService companyService) =>
        {
            var result = await companyService.GetManufacturersByCenterAsync(centerId);
            return Results.Ok(result);
        });

        group.MapGet("/logistics/{centerId:int}/manufacturers/unassigned", async (int centerId, ICompanyService companyService) =>
        {
            var result = await companyService.GetUnassignedManufacturersAsync(centerId);
            return Results.Ok(result);
        });

        group.MapPost("/logistics/{centerId:int}/assign-manufacturers", async (int centerId, List<int> manufacturerIds, ICompanyService companyService) =>
        {
            var result = await companyService.AssignManufacturersToCenterAsync(centerId, manufacturerIds);
            return Results.Ok(result);
        });

        group.MapGet("/manufacturers/{manufacturerId:int}/logistics", async (int manufacturerId, ICompanyService companyService) =>
        {
            var result = await companyService.GetLogisticsCentersByManufacturerAsync(manufacturerId);
            return Results.Ok(result);
        });

        group.MapGet("/manufacturers/{manufacturerId:int}/logistics/unassigned", async (int manufacturerId, ICompanyService companyService) =>
        {
            var result = await companyService.GetUnassignedLogisticsCentersForManufacturerAsync(manufacturerId);
            return Results.Ok(result);
        });

        group.MapPost("/manufacturers/{manufacturerId:int}/assign-logistics", async (int manufacturerId, List<int> logisticsIds, ICompanyService companyService) =>
        {
            var result = await companyService.AssignLogisticsCentersToManufacturerAsync(manufacturerId, logisticsIds);
            return Results.Ok(result);
        });
    }
}
