using GoldbApi.DTOs;
using GoldbApi.Services;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class RoleEndpoints
{

    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/role").RequireAuthorization();

        group.MapGet("/", async (IRoleService roleService) =>
        {
            var result = await roleService.GetRolesAsync();
            return Results.Ok(result);
        });

        group.MapPost("/", async (RoleCreateRequest request, IRoleService roleService) =>
        {
            var result = await roleService.CreateRoleAsync(request);
            return Results.Ok(result);
        }).WithValidation<RoleCreateRequest>();

        group.MapPut("/{id:int}", async (int id, RoleUpdateRequest request, IRoleService roleService) =>
        {
            var result = await roleService.UpdateRoleAsync(id, request);
            return Results.Ok(result);
        }).WithValidation<RoleUpdateRequest>();

        group.MapDelete("/{id:int}", async (int id, IRoleService roleService) =>
        {
            var result = await roleService.DeleteRoleAsync(id);
            return Results.Ok(result);
        });

        group.MapGet("/{roleKey}/menus", async (string roleKey, IRoleService roleService) =>
        {
            var result = await roleService.GetRoleMenusAsync(roleKey);
            return Results.Ok(result);
        });

        group.MapPost("/assign-menus", async (AssignMenusRequest request, IRoleService roleService) =>
        {
            var result = await roleService.AssignMenusToRoleAsync(request);
            return Results.Ok(result);
        }).WithValidation<AssignMenusRequest>();

        group.MapGet("/{roleKey}/users", async (string roleKey, IRoleService roleService) =>
        {
            var result = await roleService.GetRoleUsersAsync(roleKey);
            return Results.Ok(result);
        });

        group.MapPost("/assign-users", async (AssignUsersRequest request, IRoleService roleService) =>
        {
            var result = await roleService.AssignUsersToRoleAsync(request);
            return Results.Ok(result);
        }).WithValidation<AssignUsersRequest>();
    }
}
