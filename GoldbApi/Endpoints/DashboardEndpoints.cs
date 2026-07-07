using GoldbApi.Services;

namespace GoldbApi.Endpoints;

public static class DashboardEndpoints
{

    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard").RequireAuthorization();

        group.MapGet("/stats", async (IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetStatsAsync();
            return Results.Ok(result);
        });

        group.MapGet("/retail-stats", async (IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetRetailStatsAsync();
            return Results.Ok(result);
        });

        group.MapGet("/company-stats", async (IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetCompanyStatsAsync();
            return Results.Ok(result);
        });

        group.MapGet("/daily-company-stats", async (IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetDailyCompanyStatsAsync();
            return Results.Ok(result);
        });

        group.MapGet("/factory-stats", async (IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetFactoryStatsAsync();
            return Results.Ok(result);
        });

        group.MapGet("/admin-stats", async (IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetAdminDashboardStatsAsync();
            return Results.Ok(result);
        });

        group.MapGet("/partner-retailers", async (IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetPartnerRetailerStatsAsync();
            return Results.Ok(result);
        });
    }
}
