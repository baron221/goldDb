using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace GoldbApi.Services;

public interface IPlasterOrderService
{

    Task<ApiResponse<PagedResult<PlasterOrderDto>>> GetPlasterOrdersAsync(PlasterOrderQueryDto query);

    Task<ApiResponse<PlasterOrderDto>> GetPlasterOrderAsync(int id);

    Task<ApiResponse<PlasterOrderDto>> CreatePlasterOrderAsync(CreatePlasterOrderDto request);

    Task<ApiResponse<string>> UpdatePlasterOrderAsync(int id, CreatePlasterOrderDto request);

    Task<ApiResponse<string>> DeletePlasterOrderAsync(int id);
}

public class PlasterOrderService : IPlasterOrderService
{
    private readonly IRepository<PlasterOrder> _plasterOrderRepository;

    public PlasterOrderService(IRepository<PlasterOrder> plasterOrderRepository)
    {
        _plasterOrderRepository = plasterOrderRepository;
    }

    public async Task<ApiResponse<PagedResult<PlasterOrderDto>>> GetPlasterOrdersAsync(PlasterOrderQueryDto query)
    {
        var dbQuery = _plasterOrderRepository.GetQueryable();

        if (query.OrderingCompanyId.HasValue)
            dbQuery = dbQuery.Where(o => o.OrderingCompanyId == query.OrderingCompanyId.Value);

        if (!string.IsNullOrEmpty(query.Status) && query.Status != "전체")
            dbQuery = dbQuery.Where(o => o.Status == query.Status);

        if (query.StartDate.HasValue)
            dbQuery = dbQuery.Where(o => o.OrderDate >= query.StartDate.Value);

        if (query.EndDate.HasValue)
            dbQuery = dbQuery.Where(o => o.OrderDate <= query.EndDate.Value);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(o => o.OrderDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectToType<PlasterOrderDto>()
            .ToListAsync();

        return ApiResponse<PagedResult<PlasterOrderDto>>.Success(new PagedResult<PlasterOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<PlasterOrderDto>> GetPlasterOrderAsync(int id)
    {
        var o = await _plasterOrderRepository.GetQueryable()
            .Include(o => o.OrderingCompany)
            .Include(o => o.Manufacturer)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (o == null) return ApiResponse<PlasterOrderDto>.Failure("Order not found", 404);

        return ApiResponse<PlasterOrderDto>.Success(o.Adapt<PlasterOrderDto>());
    }

    public async Task<ApiResponse<PlasterOrderDto>> CreatePlasterOrderAsync(CreatePlasterOrderDto request)
    {
        var order = new PlasterOrder
        {
            OrderingCompanyId = request.OrderingCompanyId,
            ManufacturerId = request.ManufacturerId,
            Quantity = request.Quantity,
            Status = request.Status,
            OrderDate = request.OrderDate,
            IsCancelled = request.IsCancelled,
            Remarks = request.Remarks
        };

        await _plasterOrderRepository.AddAsync(order);
        await _plasterOrderRepository.SaveChangesAsync();

        return await GetPlasterOrderAsync(order.Id);
    }

    public async Task<ApiResponse<string>> UpdatePlasterOrderAsync(int id, CreatePlasterOrderDto request)
    {
        var order = await _plasterOrderRepository.GetByIdAsync(id);
        if (order == null) return ApiResponse<string>.Failure("Order not found", 404);

        order.OrderingCompanyId = request.OrderingCompanyId;
        order.ManufacturerId = request.ManufacturerId;
        order.Quantity = request.Quantity;
        order.Status = request.Status;
        order.OrderDate = request.OrderDate;
        order.IsCancelled = request.IsCancelled;
        order.Remarks = request.Remarks;

        await _plasterOrderRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeletePlasterOrderAsync(int id)
    {
        var order = await _plasterOrderRepository.GetByIdAsync(id);
        if (order == null) return ApiResponse<string>.Failure("Order not found", 404);

        _plasterOrderRepository.Delete(order);
        await _plasterOrderRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}
