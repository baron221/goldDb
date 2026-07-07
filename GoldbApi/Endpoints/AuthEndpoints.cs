using GoldbApi.DTOs;
using GoldbApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GoldbApi.Filters;

namespace GoldbApi.Endpoints;

public static class AuthEndpoints
{

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {

        app.MapPost("/user/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            return Results.Ok(result);
        }).WithValidation<LoginRequest>();

        app.MapGet("/user/dev-users", async (IAuthService authService) =>
        {
            var result = await authService.GetDevUsersAsync();
            return Results.Ok(result);
        });

        app.MapPost("/user/register", async (RegisterRequest request, IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(request);
            return Results.Ok(result);
        }).WithValidation<RegisterRequest>();

        app.MapPost("/user/find-id", async (FindIdRequest request, IAuthService authService) =>
        {
            var result = await authService.FindIdAsync(request);
            return Results.Ok(result);
        }).WithValidation<FindIdRequest>();

        app.MapPost("/user/request-reset-password", async (ResetPasswordRequest request, IAuthService authService) =>
        {
            var result = await authService.RequestPasswordResetAsync(request);
            return Results.Ok(result);
        }).WithValidation<ResetPasswordRequest>();

        app.MapPost("/user/reset-password", async (PasswordResetActionRequest request, IAuthService authService) =>
        {
            var result = await authService.ResetPasswordAsync(request);
            return Results.Ok(result);
        }).WithValidation<PasswordResetActionRequest>();

        app.MapGet("/user/info", async (HttpContext httpContext, IAuthService authService) =>
        {
            var username = httpContext.User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                var token = httpContext.Request.Query["token"].ToString();
                if (string.IsNullOrEmpty(token))
                {
                    return Results.Ok(ApiResponse<UserInfoResponse>.Failure("Unauthorized", 50008));
                }
            }

            var result = await authService.GetUserInfoAsync(username ?? "admin"); 
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapGet("/user/menus", async (bool? raw, IAuthService authService) =>
        {
            var result = await authService.GetMenusAsync(raw ?? false);
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapPost("/user/menus", async (MenuSaveRequest request, IAuthService authService) =>
        {
            var result = await authService.SaveMenuAsync(request);
            return Results.Ok(result);
        }).RequireAuthorization().WithValidation<MenuSaveRequest>();

        app.MapPost("/user/menus/batch", async (MenuBatchUpdateRequest request, IAuthService authService) =>
        {
            var result = await authService.BatchUpdateMenusAsync(request);
            return Results.Ok(result);
        }).RequireAuthorization().WithValidation<MenuBatchUpdateRequest>();

        app.MapPost("/user/menu-settings", async (UserMenuSettingUpdateRequest request, IUserMenuSettingService userMenuSettingService) =>
        {
            var result = await userMenuSettingService.UpdateUserMenuSettingAsync(request);
            return Results.Ok(result);
        }).RequireAuthorization().WithValidation<UserMenuSettingUpdateRequest>();

        app.MapGet("/user/menu-settings", async (IUserMenuSettingService userMenuSettingService) =>
        {
            var result = await userMenuSettingService.GetUserMenuSettingsAsync();
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapPost("/user/menu-settings/sort", async (UserMenuSettingSortRequest request, IUserMenuSettingService userMenuSettingService) =>
        {
            var result = await userMenuSettingService.UpdateSortOrderAsync(request);
            return Results.Ok(result);
        }).RequireAuthorization().WithValidation<UserMenuSettingSortRequest>();

        app.MapDelete("/user/menus/{id:int}", async (int id, IAuthService authService) =>
        {
            var result = await authService.DeleteMenuAsync(id);
            return Results.Ok(result);
        }).RequireAuthorization();

        app.MapPost("/user/logout", () =>
        {
            return Results.Ok(ApiResponse<string>.Success("success"));
        });
    }
}
