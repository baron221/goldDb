using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoldbApi.Services;

public interface ICartService
{

    Task<ApiResponse<List<CartItemDto>>> GetMyCartAsync();

    Task<ApiResponse<string>> AddToCartAsync(AddToCartDto request);

    Task<ApiResponse<string>> UpdateQuantityAsync(int id, int quantity);

    Task<ApiResponse<string>> RemoveFromCartAsync(int id);

    Task<ApiResponse<string>> ClearCartAsync();

    Task<ApiResponse<string>> UpdatePriceAsync(int id, decimal? factoryPrice, decimal? laborCost);
}

public class CartService : ICartService
{
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppDbContext _dbContext;

    public CartService(IRepository<CartItem> cartItemRepository, ICurrentUserService currentUserService, AppDbContext dbContext)
    {
        _cartItemRepository = cartItemRepository;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
    }

    private int GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }

    public async Task<ApiResponse<List<CartItemDto>>> GetMyCartAsync()
    {
        var userId = GetCurrentUserId();
        var items = await _cartItemRepository.GetQueryable()
            .Include(c => c.Product)
                .ThenInclude(p => p!.ProductPhotos)
            .Include(c => c.Product)
                .ThenInclude(p => p!.OptionWeights)
            .Include(c => c.Product)
                .ThenInclude(p => p!.Company)
            .Include(c => c.ProductSet)
                .ThenInclude(ps => ps!.ProductSetPhotos)
            .Include(c => c.ProductSet)
                .ThenInclude(ps => ps!.SetItems)
                    .ThenInclude(si => si.Product)
                        .ThenInclude(p => p!.ProductPhotos)
            .Include(c => c.ProductSet)
                .ThenInclude(ps => ps!.Company)
            .Where(c => c.UserId == userId)
            .ProjectToType<CartItemDto>()
            .ToListAsync();

        return ApiResponse<List<CartItemDto>>.Success(items);
    }

    public async Task<ApiResponse<string>> AddToCartAsync(AddToCartDto request)
    {
        var userId = GetCurrentUserId();

        if (request.TargetCompanyId.HasValue)
        {
            var targetUserCompany = await _dbContext.UserCompanies.FirstOrDefaultAsync(uc => uc.CompanyId == request.TargetCompanyId.Value && !uc.IsDeleted);
            if (targetUserCompany == null)
            {
                return ApiResponse<string>.Failure("No active user found for the target company.", 400);
            }
            userId = targetUserCompany.UserId;
        }

        var existingItem = await _cartItemRepository.GetQueryable()
            .FirstOrDefaultAsync(c => c.UserId == userId && 
                                     c.ProductId == request.ProductId && 
                                     c.ProductSetId == request.ProductSetId &&
                                     c.Purity == request.Purity &&
                                     c.Color == request.Color &&
                                     c.Size == request.Size &&
                                     c.Memo == request.Memo);

        if (existingItem != null)
        {

            existingItem.Quantity += request.Quantity;
        }
        else
        {

            var newItem = request.Adapt<CartItem>();
            newItem.UserId = userId;
            await _cartItemRepository.AddAsync(newItem);
        }

        await _cartItemRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> UpdateQuantityAsync(int id, int quantity)
    {
        var userId = GetCurrentUserId();
        var item = await _cartItemRepository.GetQueryable().FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item == null) return ApiResponse<string>.Failure("Cart item not found", 404);

        if (quantity <= 0)
        {
            _cartItemRepository.Delete(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        await _cartItemRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> RemoveFromCartAsync(int id)
    {
        var userId = GetCurrentUserId();
        var item = await _cartItemRepository.GetQueryable().FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item == null) return ApiResponse<string>.Failure("Cart item not found", 404);

        _cartItemRepository.Delete(item);
        await _cartItemRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> ClearCartAsync()
    {
        var userId = GetCurrentUserId();
        var items = await _cartItemRepository.GetQueryable().Where(c => c.UserId == userId).ToListAsync();
        foreach (var item in items)
        {
            _cartItemRepository.Delete(item);
        }
        await _cartItemRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> UpdatePriceAsync(int id, decimal? factoryPrice, decimal? laborCost)
    {
        var userId = GetCurrentUserId();
        var item = await _cartItemRepository.GetQueryable().FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item == null) return ApiResponse<string>.Failure("Cart item not found", 404);

        item.CustomFactoryPrice = factoryPrice;
        item.CustomLaborCost = laborCost;

        await _cartItemRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}
