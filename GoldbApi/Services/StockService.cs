using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using GoldbApi.Utils;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace GoldbApi.Services;

public interface IStockService
{

    Task<ApiResponse<PagedResult<StockDto>>> GetStocksAsync(StockQueryDto query);

    Task<ApiResponse<StockSummaryDto>> GetStockSummaryAsync(StockQueryDto query);

    Task<ApiResponse<StockDetailDto>> GetStockDetailAsync(int id);

    Task<ApiResponse<StockDto>> CreateStockAsync(CreateStockDto request);

    Task<ApiResponse<string>> UpdateStockAsync(int id, UpdateStockDto request);

    Task<ApiResponse<string>> DeleteStockAsync(int id);

    Task<ApiResponse<string>> DeleteStocksAsync(StockDeleteDto request);

    Task<ApiResponse<string>> UpdateStockPhotosAsync(int stockId, List<int> attachmentIds, int? mainAttachmentId);

    Task<ApiResponse<string>> RentStocksAsync(StockRentDto request);

    Task<ApiResponse<string>> ReturnStocksAsync(List<int> ids);

    Task<ApiResponse<string>> RentByBarcodeAsync(StockBarcodeActionDto request);

    Task<ApiResponse<string>> ReturnByBarcodeAsync(StockBarcodeActionDto request);

    Task<ApiResponse<string>> BulkUpdateStockAsync(List<StockDto> stocks);

    Task<ApiResponse<string>> AddOrderItemsToStockAsync(int orderId);

    Task<ApiResponse<string>> ExhaustStockForOrderItemAsync(ExhaustStockOrderItemDto request);
}

public class StockService : IStockService
{
    private readonly IStockRepository _stockRepository;

    public StockService(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<ApiResponse<PagedResult<StockDto>>> GetStocksAsync(StockQueryDto query)
    {
        var (items, totalCount) = await _stockRepository.GetStocksAsync(query);
        var page = query.Page ?? 1;
        var pageSize = query.PageSize ?? 50;

        return ApiResponse<PagedResult<StockDto>>.Success(new PagedResult<StockDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<StockSummaryDto>> GetStockSummaryAsync(StockQueryDto query)
    {
        var list = await _stockRepository.GetStocksForSummaryAsync(query);

        var summary = new StockSummaryDto();
        foreach (var s in list)
        {
            var purity = s.Purity ?? (s.Product != null ? s.Product.Purity : "");

            if (purity == "14K") summary.Total14KWeight += s.ActualWeight;
            else if (purity == "18K") summary.Total18KWeight += s.ActualWeight;
            else if (purity == "24K") summary.TotalPureGoldWeight += s.ActualWeight;
        }

        summary.TotalCalculatedPureGoldWeight = (summary.Total14KWeight * 0.6435m) + (summary.Total18KWeight * 0.825m) + summary.TotalPureGoldWeight;

        return ApiResponse<StockSummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<StockDetailDto>> GetStockDetailAsync(int id)
    {
        var stock = await _stockRepository.GetByIdWithDetailsAsync(id);

        if (stock == null) return ApiResponse<StockDetailDto>.Failure("Stock not found", 404);

        var dto = stock.Adapt<StockDetailDto>();

        if (stock.SourceOrderId.HasValue)
        {
            var history = await _stockRepository.GetOrderHistoryForStockAsync(stock.SourceOrderId.Value);
            dto.OrderHistory = history;
        }

        return ApiResponse<StockDetailDto>.Success(dto);
    }

    public async Task<ApiResponse<StockDto>> CreateStockAsync(CreateStockDto request)
    {
        var stock = new Stock
        {
            ProductId = request.ProductId,
            ProductSetId = request.ProductSetId,
            CompanyId = request.CompanyId,
            StockNo = request.StockNo,
            Status = request.Status,
            ActualWeight = request.ActualWeight,
            Note = request.Note,
            RetailerConfirmMaterialCost = request.RetailerConfirmMaterialCost,
            RetailerConfirmLaborCost = request.RetailerConfirmLaborCost
        };

        await _stockRepository.AddAsync(stock);
        await _stockRepository.SaveChangesAsync();
        return ApiResponse<StockDto>.Success(new StockDto { Id = stock.Id });
    }

    public async Task<ApiResponse<string>> UpdateStockAsync(int id, UpdateStockDto request)
    {
        var stock = await _stockRepository.GetByIdAsync(id);
        if (stock == null) return ApiResponse<string>.Failure("Stock not found", 404);

        stock.Status = request.Status ?? stock.Status;
        stock.ActualWeight = request.ActualWeight ?? stock.ActualWeight;
        stock.RenterName = request.RenterName;
        stock.RentDate = request.RentDate;
        stock.ReturnDueDate = request.ReturnDueDate;
        stock.Note = request.Note;
        stock.RetailerConfirmMaterialCost = request.RetailerConfirmMaterialCost ?? stock.RetailerConfirmMaterialCost;
        stock.RetailerConfirmLaborCost = request.RetailerConfirmLaborCost ?? stock.RetailerConfirmLaborCost;

        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeleteStockAsync(int id)
    {
        var stock = await _stockRepository.GetByIdAsync(id);
        if (stock == null) return ApiResponse<string>.Failure("Stock not found", 404);

        _stockRepository.Delete(stock);
        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> DeleteStocksAsync(StockDeleteDto request)
    {
        var stocks = await _stockRepository.GetStocksByIdsAsync(request.Ids);

        Order? exhaustionOrder = null;
        if (request.ExhaustionOrderId.HasValue && request.Reason == "SOLD")
        {
            exhaustionOrder = await _stockRepository.GetOrderWithItemsAsync(request.ExhaustionOrderId.Value);

            if (exhaustionOrder == null) return ApiResponse<string>.Failure("소진 처리할 주문을 찾을 수 없습니다.");
            if (exhaustionOrder.Status != "LogisticsApproved") return ApiResponse<string>.Failure("물류승인 상태인 주문만 판매 소진 처리가 가능합니다.");

            exhaustionOrder.Status = "PENDING";
        }

        foreach (var stock in stocks)
        {
            stock.Status = request.Reason;
            stock.Note = request.Memo;
            stock.IsExhausted = true;
            stock.ExhaustionDate = request.ExhaustionDate ?? DateTime.UtcNow;

            if (exhaustionOrder != null)
            {
                stock.ExhaustionOrderId = exhaustionOrder.Id;
            }
        }
        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> UpdateStockPhotosAsync(int stockId, List<int> attachmentIds, int? mainAttachmentId)
    {
        var stock = await _stockRepository.GetByIdWithDetailsAsync(stockId);

        if (stock == null) return ApiResponse<string>.Failure("Stock not found", 404);

        stock.Attachments.Clear();
        var attachments = await _stockRepository.GetAttachmentsByIdsAsync(attachmentIds);

        foreach(var a in attachments)
        {
            a.IsMain = (a.Id == mainAttachmentId);
        }

        stock.Attachments.AddRange(attachments);

        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> RentStocksAsync(StockRentDto request)
    {
        var stocks = await _stockRepository.GetStocksByIdsAsync(request.StockIds);
        foreach (var stock in stocks)
        {
            stock.Status = "RENTED";
            stock.RenterName = request.RenterName;
            stock.RentDate = DateTime.Now;
            stock.ReturnDueDate = request.ReturnDueDate;
        }
        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> ReturnStocksAsync(List<int> ids)
    {
        var stocks = await _stockRepository.GetStocksByIdsAsync(ids);
        foreach (var stock in stocks)
        {
            stock.Status = "IN_STOCK";
            stock.RenterName = null;
            stock.RentDate = null;
            stock.ReturnDueDate = null;
        }
        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> RentByBarcodeAsync(StockBarcodeActionDto request)
    {
        var stock = await _stockRepository.GetByStockNoAsync(request.Barcode);
        if (stock == null) return ApiResponse<string>.Failure("Stock not found", 404);

        stock.Status = "RENTED";
        stock.RenterName = request.RenterName;
        stock.RentDate = DateTime.Now;
        stock.ReturnDueDate = request.ReturnDueDate;

        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> ReturnByBarcodeAsync(StockBarcodeActionDto request)
    {
        var stock = await _stockRepository.GetByStockNoAsync(request.Barcode);
        if (stock == null) return ApiResponse<string>.Failure("Stock not found", 404);

        stock.Status = "IN_STOCK";
        stock.RenterName = null;
        stock.RentDate = null;
        stock.ReturnDueDate = null;

        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> BulkUpdateStockAsync(List<StockDto> stocks)
    {
        foreach (var sDto in stocks)
        {
            var stock = await _stockRepository.GetByIdAsync(sDto.Id);
            if (stock != null)
            {
                stock.ActualWeight = sDto.ActualWeight;
                stock.Note = sDto.Note;
            }
        }
        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> AddOrderItemsToStockAsync(int orderId)
    {
        var order = await _stockRepository.GetOrderWithItemsAsync(orderId);

        if (order == null) return ApiResponse<string>.Failure("Order not found", 404);

        var userCompany = await _stockRepository.GetUserCompanyAsync(order.UserId);
        if (userCompany == null) return ApiResponse<string>.Failure("Ordering user has no associated company", 400);

        int count = 0;

        foreach (var item in order.OrderItems.Where(oi => oi.ParentId == null))
        {
            for (int i = 0; i < item.Quantity; i++)
            {
                count++;

                var stock = new Stock
                {
                    ProductId = item.ProductId,
                    ProductSetId = item.ProductSetId,
                    CompanyId = userCompany.CompanyId,
                    StockNo = $"STK-{order.OrderNo}-{count}",
                    Status = "IN_STOCK",
                    Purity = item.Purity,
                    Color = item.Color,
                    ActualWeight = item.ConfirmedWeight ?? item.ActualWeight ?? 0,
                    ProductionDate = item.ProductionDate,
                    Note = $"주문({order.OrderNo})을 통한 자동 입고",
                    SourceOrderId = order.Id,
                    SourceOrderItemId = item.Id,
                    RetailerConfirmMaterialCost = item.RetailerConfirmMaterialCost,
                    RetailerConfirmLaborCost = item.RetailerConfirmLaborCost
                };
                await _stockRepository.AddAsync(stock);

                if (item.Children != null && item.Children.Any())
                {
                    int subCount = 0;
                    foreach (var child in item.Children)
                    {
                        subCount++;
                        var childStock = new Stock
                        {
                            ProductId = child.ProductId,
                            ParentStock = stock, 
                            CompanyId = userCompany.CompanyId,
                            StockNo = $"STK-{order.OrderNo}-{count}-{subCount}",
                            Status = "IN_STOCK",
                            Purity = child.Purity,
                            Color = child.Color,
                            ActualWeight = child.ConfirmedWeight ?? child.ActualWeight ?? 0,
                            ProductionDate = child.ProductionDate,
                            Note = $"세트 구성품 자동 입고 (상위: {stock.StockNo})",
                            SourceOrderId = order.Id,
                            SourceOrderItemId = child.Id,
                            RetailerConfirmMaterialCost = child.RetailerConfirmMaterialCost,
                            RetailerConfirmLaborCost = child.RetailerConfirmLaborCost
                        };
                        await _stockRepository.AddAsync(childStock);
                    }
                }
            }
        }

        await _stockRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> ExhaustStockForOrderItemAsync(ExhaustStockOrderItemDto request)
    {
        var orderItem = await _stockRepository.GetOrderItemWithOrderAndExhaustedStocksAsync(request.OrderItemId);

        if (orderItem == null) return ApiResponse<string>.Failure("주문 항목을 찾을 수 없습니다.");

        var stock = await _stockRepository.GetByIdWithDetailsAsync(request.StockId);

        if (stock == null) return ApiResponse<string>.Failure("재고 정보를 찾을 수 없습니다.");

        if (stock.IsExhausted) return ApiResponse<string>.Failure("이미 소진된 재고입니다.");

        if (stock.ParentStockId.HasValue)
        {
            return ApiResponse<string>.Failure("세트의 구성품은 개별적으로 소진 처리할 수 없습니다. 세트 전체를 소진 처리해 주세요.");
        }

        stock.IsExhausted = true;
        stock.Status = "SOLD"; 
        stock.ExhaustionOrderId = orderItem.OrderId;
        stock.ExhaustionOrderItemId = orderItem.Id;
        stock.Note = request.Memo ?? $"주문({orderItem.Order?.OrderNo}) 항목 소진 처리";

        if (stock.Children.Any())
        {
            var childOrderItems = await _stockRepository.GetChildOrderItemsAsync(orderItem.Id);

            foreach (var childStock in stock.Children)
            {
                childStock.IsExhausted = true;
                childStock.Status = "SOLD";
                childStock.ExhaustionOrderId = orderItem.OrderId;

                var matchingOrderItem = childOrderItems.FirstOrDefault(oi => oi.ProductId == childStock.ProductId);
                if (matchingOrderItem != null)
                {
                    childStock.ExhaustionOrderItemId = matchingOrderItem.Id;
                }

                childStock.Note = $"세트 소진에 따른 하위 구성품 자동 소진 (상위: {stock.StockNo})";
            }
        }

        await _stockRepository.SaveChangesAsync();

        var order = await _stockRepository.GetOrderWithItemsAndExhaustedStocksAsync(orderItem.OrderId);

        if (order != null)
        {
            var topLevelItems = order.OrderItems.Where(oi => oi.ParentId == null).ToList();
            bool allExhausted = true;

            foreach(var item in topLevelItems)
            {
                if (item.Children.Any())
                {
                    if (item.Children.Any(c => !c.ExhaustedStocks.Any()))
                    {
                        allExhausted = false;
                        break;
                    }
                }
                else if (item.ProductId.HasValue && !item.ExhaustedStocks.Any())
                {
                    allExhausted = false;
                    break;
                }
            }

            if (allExhausted)
            {
                order.Status = "PENDING";

                var history = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    Remarks = "모든 주문 항목이 재고에서 소진되어 정산 대기 상태로 변경되었습니다."
                };
                await _stockRepository.AddOrderStatusHistoryAsync(history);
                await _stockRepository.SaveChangesAsync();
            }
        }

        return ApiResponse<string>.Success("success");
    }
}
