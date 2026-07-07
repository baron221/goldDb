using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GoldbApi.Endpoints;

public static class ContactEndpoints
{

    public static void MapContactEndpoints(this IEndpointRouteBuilder app)
    {

        app.MapPost("/contact", async (ContactMessageRequest request, IRepository<ContactMessage> contactRepository) =>
        {
            var message = new ContactMessage
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Subject = request.Subject,
                Message = request.Message
            };

            await contactRepository.AddAsync(message);
            await contactRepository.SaveChangesAsync();

            return Results.Ok(ApiResponse<string>.Success("Message sent successfully."));
        });

        var adminGroup = app.MapGroup("/admin/contacts").WithTags("ContactAdmin").RequireAuthorization();

        adminGroup.MapGet("/", async (IRepository<ContactMessage> contactRepository, [FromQuery] bool? isProcessed, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var query = contactRepository.GetQueryable();

            if (isProcessed.HasValue)
            {
                query = query.Where(c => c.IsProcessed == isProcessed.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Results.Ok(ApiResponse<PagedResult<ContactMessage>>.Success(new PagedResult<ContactMessage>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            }));
        });

        adminGroup.MapPut("/{id}/process", async (int id, ContactProcessRequest request, IRepository<ContactMessage> contactRepository, HttpContext httpContext) =>
        {
            var message = await contactRepository.GetByIdAsync(id);
            if (message == null) return Results.Ok(ApiResponse<string>.Failure("Message not found", 404));

            var userIdClaim = httpContext.User.FindFirst("UserId")?.Value;
            int? userId = int.TryParse(userIdClaim, out var idVal) ? idVal : null;

            message.IsProcessed = request.IsProcessed;
            message.AdminMemo = request.AdminMemo;
            message.ProcessedAt = request.IsProcessed ? DateTime.UtcNow : null;
            message.ProcessedBy = request.IsProcessed ? userId : null;

            contactRepository.Update(message);
            await contactRepository.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Status updated successfully."));
        });

        adminGroup.MapDelete("/{id}", async (int id, IRepository<ContactMessage> contactRepository) =>
        {
            var message = await contactRepository.GetByIdAsync(id);
            if (message == null) return Results.Ok(ApiResponse<string>.Failure("Message not found", 404));

            contactRepository.Delete(message);
            await contactRepository.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Message deleted successfully."));
        });
    }
}

public class ContactMessageRequest
{

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}

public class ContactProcessRequest
{

    public bool IsProcessed { get; set; }

    public string? AdminMemo { get; set; }
}
