using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;

namespace GoldbApi.Services;

public interface IOrderService
{

    Task<ApiResponse<PagedResult<OrderDto>>> GetMyOrdersAsync(OrderQueryDto query);

    Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto request);

    Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersAsync(OrderQueryDto query);

    Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto request);

    Task<ApiResponse<bool>> DeleteOrderAsync(int id);

    Task<ApiResponse<SettlementHistorySummaryDto>> GetSettlementSummaryAsync(OrderQueryDto query);

    Task<ApiResponse<List<OrderStatusHistoryDto>>> GetOrderHistoryAsync(int orderId);

    Task<ApiResponse<bool>> SaveOrderStatementAsync(SaveOrderStatementDto request);

    Task<ApiResponse<OrderStatementDto>> GetOrderStatementAsync(int orderId);

    Task<ApiResponse<OrderStatementDto>> GetOrderStatementPdfAsync(int orderId);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStockService _stockService;
    private readonly IReceivableService _receivableService;

    public OrderService(IOrderRepository orderRepository, ICurrentUserService currentUserService, IStockService stockService, IReceivableService receivableService)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _stockService = stockService;
        _receivableService = receivableService;
    }

    private int GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }

    private async Task<UserCompany?> GetCurrentUserCompanyInfoAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            return await _orderRepository.GetUserCompanyInfoAsync(userId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetMyOrdersAsync(OrderQueryDto query)
    {
        var userId = GetCurrentUserId();
        var (items, totalCount) = await _orderRepository.GetMyOrdersAsync(userId, query);

        foreach (var item in items)
        {
            if (item.OrderItems.Any())
            {
                item.ManufacturerName = item.OrderItems.First().ManufacturerName;
            }
        }

        return ApiResponse<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto request)
    {
        var userId = GetCurrentUserId();

        var userCompany = await _orderRepository.GetUserCompanyInfoAsync(userId);
        int? assignedLogisticsId = userCompany?.Company?.LogisticsCompanyId;

        var cartItems = await _orderRepository.GetCartItemsForUserAsync(userId, request.CartItemIds);

        if (!cartItems.Any())
        {
            return ApiResponse<OrderDto>.Failure("주문할 항목이 선택되지 않았습니다.", 400);
        }

        var todayLocal = DateTime.Today;
        var tomorrowLocal = todayLocal.AddDays(1);
        var todayLocalUtcStart = todayLocal.ToUniversalTime();
        var tomorrowLocalUtcStart = tomorrowLocal.ToUniversalTime();

        var todayOrderCount = await _orderRepository.GetTodayOrderCountAsync(todayLocalUtcStart, tomorrowLocalUtcStart);

        var groupOrderNo = $"GRP-{DateTime.Now:yyyyMMddHHmmss}-{userId}";

        var itemsByManufacturer = cartItems.GroupBy(ci => 
            ci.Product?.CompanyId ?? ci.ProductSet?.CompanyId ?? 0);

        var createdOrders = new List<Order>();

        foreach (var manufacturerGroup in itemsByManufacturer)
        {
            todayOrderCount++;
            var order = new Order
            {
                UserId = userId,
                OrderNo = $"ORD-{DateTime.Now:yyMMddHHmm}-{userId}-{todayOrderCount}",
                GroupOrderNo = groupOrderNo,
                Status = "ORDERED",
                TotalAmount = 0,
                LogisticsCompanyId = assignedLogisticsId,
                OrderMemo = request.OrderMemo,
                FactoryRemarks = request.FactoryRemarks,
                LogisticsRemarks = request.LogisticsRemarks,
                CustomerId = request.CustomerId
            };

            foreach (var cartItem in manufacturerGroup)
            {

                decimal baseFactoryPrice = cartItem.CustomFactoryPrice ?? (cartItem.Product != null ? cartItem.Product.FactoryPrice : (cartItem.ProductSet?.FactoryPrice ?? 0));
                decimal baseLaborCost = cartItem.CustomLaborCost ?? (cartItem.Product != null ? cartItem.Product.LaborCost : (cartItem.ProductSet?.LaborCost ?? 0));
                decimal price = baseFactoryPrice + baseLaborCost;

                decimal? requestedWeight = null;
                if (cartItem.Product != null)
                {
                    var optWeight = cartItem.Product.OptionWeights
                        .FirstOrDefault(ow => ow.Purity == cartItem.Purity && ow.Color == cartItem.Color);
                    requestedWeight = optWeight != null ? optWeight.Weight : cartItem.Product.Weight;
                }

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductSetId = cartItem.ProductSetId,
                    Quantity = cartItem.Quantity,
                    Price = price,
                    FactoryPrice = baseFactoryPrice,
                    LaborCost = baseLaborCost,
                    Purity = cartItem.Purity,
                    Color = cartItem.Color,
                    OrderWeight = requestedWeight
                };

                if (cartItem.ProductSetId.HasValue)
                {
                    var set = await _orderRepository.GetProductSetWithItemsAsync(cartItem.ProductSetId.Value);

                    if (set != null)
                    {
                        foreach (var setItem in set.SetItems)
                        {
                            orderItem.Children.Add(new OrderItem
                            {
                                ProductId = setItem.ProductId,
                                Quantity = cartItem.Quantity,
                                Price = 0,
                                Order = order
                            });
                        }
                    }
                }

                order.OrderItems.Add(orderItem);
                order.TotalAmount += price * cartItem.Quantity;
            }

            await _orderRepository.AddAsync(order);
            createdOrders.Add(order);

            var history = new OrderStatusHistory
            {
                Order = order,
                Status = order.Status,
                UserId = userId,
                Remarks = "주문 접수"
            };
            await _orderRepository.AddStatusHistoryAsync(history);
        }

        _orderRepository.RemoveCartItems(cartItems);
        await _orderRepository.SaveChangesAsync();

        var firstOrder = createdOrders.First();
        return ApiResponse<OrderDto>.Success(new OrderDto 
        { 
            Id = firstOrder.Id, 
            OrderNo = firstOrder.OrderNo, 
            GroupOrderNo = groupOrderNo 
        });
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersAsync(OrderQueryDto query)
    {
        var userCompany = await GetCurrentUserCompanyInfoAsync();
        if (userCompany != null && userCompany.Company?.Category == "RTL")
        {
            query.CompanyId = userCompany.CompanyId;
        }

        var (items, totalCount) = await _orderRepository.GetAllOrdersAsync(query);

        foreach (var item in items)
        {
            if (item.OrderItems.Any())
            {
                item.ManufacturerName = item.OrderItems.First().ManufacturerName;
            }
        }

        return ApiResponse<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<SettlementHistorySummaryDto>> GetSettlementSummaryAsync(OrderQueryDto query)
    {
        var userCompany = await GetCurrentUserCompanyInfoAsync();
        if (userCompany != null && userCompany.Company?.Category == "RTL")
        {
            query.CompanyId = userCompany.CompanyId;
        }

        var summary = await _orderRepository.GetSettlementSummaryAsync(query);

        return ApiResponse<SettlementHistorySummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto request)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return ApiResponse<bool>.Failure("Order not found", 404);

        var oldStatus = order.Status;

        if (request.Status == "Cancelled" && oldStatus != "ORDERED")
        {
            return ApiResponse<bool>.Failure("주문접수 상태인 주문만 취소할 수 있습니다.", 400);
        }

        order.Status = request.Status;

        if (request.FactoryRemarks != null) order.FactoryRemarks = request.FactoryRemarks;
        if (request.InspectionRemarks != null) order.InspectionRemarks = request.InspectionRemarks;
        if (request.WorkOrderRemarks != null) order.WorkOrderRemarks = request.WorkOrderRemarks;
        if (request.SettlementMethod != null) order.SettlementMethod = request.SettlementMethod;
        if (request.SettlementRemarks != null) order.SettlementRemarks = request.SettlementRemarks;
        if (request.SettlementStartMemo != null) order.SettlementStartMemo = request.SettlementStartMemo;
        if (request.SettlementAmount.HasValue) order.SettlementAmount = request.SettlementAmount.Value;
        if (request.LogisticsCompanyId.HasValue) order.LogisticsCompanyId = request.LogisticsCompanyId;
        if (!string.IsNullOrEmpty(request.CancellationReason)) order.CancellationReason = request.CancellationReason;
        if (!string.IsNullOrEmpty(request.ModificationMemo)) order.ModificationMemo = request.ModificationMemo;
        if (!string.IsNullOrEmpty(request.CloseMemo)) order.CloseMemo = request.CloseMemo;
        if (request.DeliveryDate.HasValue) order.DeliveryDate = request.DeliveryDate;

        string historyDetails = "";

        if (request.ItemWeights != null && request.ItemWeights.Any())
        {
            var itemIds = request.ItemWeights.Select(iw => iw.OrderItemId).ToList();
            var orderItems = await _orderRepository.GetOrderItemsByIdsAsync(id, itemIds);

            var detailLines = new List<string>();

            foreach (var itemWeight in request.ItemWeights)
            {
                var orderItem = orderItems.FirstOrDefault(oi => oi.Id == itemWeight.OrderItemId);
                if (orderItem != null)
                {
                    if (itemWeight.ActualWeight.HasValue) orderItem.ActualWeight = itemWeight.ActualWeight;
                    if (itemWeight.InspectionMemo != null) orderItem.InspectionMemo = itemWeight.InspectionMemo;
                    if (itemWeight.Purity != null) orderItem.Purity = itemWeight.Purity;
                    if (itemWeight.IsAsOrder.HasValue) orderItem.IsAsOrder = itemWeight.IsAsOrder.Value;
                    if (itemWeight.ConfirmedWeight.HasValue) orderItem.ConfirmedWeight = itemWeight.ConfirmedWeight;
                    if (itemWeight.LogisticsMemo != null) orderItem.LogisticsMemo = itemWeight.LogisticsMemo;
                    if (itemWeight.ApprovedWeight.HasValue) orderItem.ApprovedWeight = itemWeight.ApprovedWeight;
                    if (itemWeight.ApprovedMemo != null) orderItem.ApprovedMemo = itemWeight.ApprovedMemo;
                    if (itemWeight.RequestedWeight.HasValue) orderItem.RequestedWeight = itemWeight.RequestedWeight;
                    if (itemWeight.RequestedMemo != null) orderItem.RequestedMemo = itemWeight.RequestedMemo;
                    if (itemWeight.SettlementRatio.HasValue) orderItem.SettlementRatio = itemWeight.SettlementRatio.Value;
                    if (itemWeight.SettlementAmount.HasValue) orderItem.SettlementAmount = itemWeight.SettlementAmount;
                    if (itemWeight.SettlementMemo != null) orderItem.SettlementMemo = itemWeight.SettlementMemo;
                    if (itemWeight.FactoryInputMaterialCost.HasValue) orderItem.FactoryInputMaterialCost = itemWeight.FactoryInputMaterialCost;
                    if (itemWeight.FactoryInputLaborCost.HasValue) orderItem.FactoryInputLaborCost = itemWeight.FactoryInputLaborCost;
                    if (itemWeight.RetailerConfirmMaterialCost.HasValue) orderItem.RetailerConfirmMaterialCost = itemWeight.RetailerConfirmMaterialCost;
                    if (itemWeight.RetailerConfirmLaborCost.HasValue) orderItem.RetailerConfirmLaborCost = itemWeight.RetailerConfirmLaborCost;
                    if (itemWeight.ProductionDate.HasValue) orderItem.ProductionDate = itemWeight.ProductionDate;

                    bool priceChanged = false;
                    if (itemWeight.FactoryPrice.HasValue)
                    {
                        orderItem.FactoryPrice = itemWeight.FactoryPrice.Value;
                        priceChanged = true;
                    }
                    if (itemWeight.LaborCost.HasValue)
                    {
                        orderItem.LaborCost = itemWeight.LaborCost.Value;
                        priceChanged = true;
                    }
                    if (priceChanged)
                    {
                        orderItem.Price = orderItem.FactoryPrice + orderItem.LaborCost;
                    }

                    var parts = new List<string>();
                    if (itemWeight.ActualWeight.HasValue) parts.Add($"실중량: {itemWeight.ActualWeight}g");
                    if (!string.IsNullOrEmpty(itemWeight.InspectionMemo)) parts.Add($"검수메모: {itemWeight.InspectionMemo}");
                    if (itemWeight.ConfirmedWeight.HasValue) parts.Add($"확정중량: {itemWeight.ConfirmedWeight}g");
                    if (itemWeight.ApprovedWeight.HasValue) parts.Add($"승인중량: {itemWeight.ApprovedWeight}g");
                    if (!string.IsNullOrEmpty(itemWeight.ApprovedMemo)) parts.Add($"승인메모: {itemWeight.ApprovedMemo}");
                    if (itemWeight.RequestedWeight.HasValue) parts.Add($"의뢰중량: {itemWeight.RequestedWeight}g");
                    if (!string.IsNullOrEmpty(itemWeight.RequestedMemo)) parts.Add($"의뢰메모: {itemWeight.RequestedMemo}");
                    if (itemWeight.SettlementAmount.HasValue) parts.Add($"정산: {itemWeight.SettlementAmount:N0}원");
                    if (!string.IsNullOrEmpty(itemWeight.LogisticsMemo)) parts.Add($"물류메모: {itemWeight.LogisticsMemo}");
                    if (itemWeight.FactoryPrice.HasValue) parts.Add($"공장도가: {itemWeight.FactoryPrice.Value:N0}원");
                    if (itemWeight.LaborCost.HasValue) parts.Add($"수공비: {itemWeight.LaborCost.Value:N0}원");
                    if (itemWeight.RetailerConfirmMaterialCost.HasValue) parts.Add($"소매점재료비: {itemWeight.RetailerConfirmMaterialCost.Value:N0}원");
                    if (itemWeight.RetailerConfirmLaborCost.HasValue) parts.Add($"소매점수공비: {itemWeight.RetailerConfirmLaborCost.Value:N0}원");
                    if (itemWeight.ProductionDate.HasValue) parts.Add($"제작생산일: {itemWeight.ProductionDate.Value:yyyy-MM-dd}");

                    if (parts.Any())
                    {
                        var itemName = orderItem.Product?.Name ?? orderItem.ProductSet?.Title ?? "상품";
                        detailLines.Add($"- {itemName}: {string.Join(", ", parts)}");
                    }
                }
            }

            var allItemsInOrder = await _orderRepository.GetTopLevelOrderItemsAsync(id);
            order.TotalAmount = allItemsInOrder.Sum(oi => oi.Price * oi.Quantity);

            if (detailLines.Any())
            {
                historyDetails = "\n" + string.Join("\n", detailLines);
            }
        }

        if (!string.IsNullOrEmpty(request.FactoryRemarks)) historyDetails = $"\n[전달 메시지] {request.FactoryRemarks}" + historyDetails;
        if (!string.IsNullOrEmpty(request.InspectionRemarks)) historyDetails = $"\n[검수 요청 메시지] {request.InspectionRemarks}" + historyDetails;
        if (!string.IsNullOrEmpty(request.WorkOrderRemarks)) historyDetails = $"\n[작업서 작성 메시지] {request.WorkOrderRemarks}" + historyDetails;
        if (!string.IsNullOrEmpty(request.CancellationReason)) historyDetails = $"\n[취소 사유] {request.CancellationReason}" + historyDetails;
        if (!string.IsNullOrEmpty(request.ModificationMemo)) historyDetails = $"\n[수정의뢰 메모] {request.ModificationMemo}" + historyDetails;
        if (!string.IsNullOrEmpty(request.CloseMemo)) historyDetails = $"\n[종결 메모] {request.CloseMemo}" + historyDetails;

        if (oldStatus != request.Status || !string.IsNullOrEmpty(request.ModificationMemo))
        {
            var userId = GetCurrentUserId();
            var history = new OrderStatusHistory
            {
                OrderId = id,
                Status = request.Status,
                UserId = userId,
                Remarks = (oldStatus != request.Status ? $"상태 변경: {oldStatus} -> {request.Status}" : "수정가공 의뢰") + historyDetails
            };
            await _orderRepository.AddStatusHistoryAsync(history);

            if (oldStatus != request.Status && request.Status == "Completed")
            {
                await _stockService.AddOrderItemsToStockAsync(id);
            }

            if (oldStatus != request.Status && request.Status == "PENDING")
            {

                var topLevelItems = await _orderRepository.GetTopLevelOrderItemsAsync(id);

                var calculatedSettlementAmount = topLevelItems.Sum(oi => 
                    ((oi.RetailerConfirmMaterialCost ?? oi.FactoryInputMaterialCost ?? 0) + 
                     (oi.RetailerConfirmLaborCost ?? oi.FactoryInputLaborCost ?? 0)) * oi.Quantity);

                order.SettlementAmount = calculatedSettlementAmount > 0 ? calculatedSettlementAmount : order.TotalAmount;

                var charge = new Receivable
                {
                    UserId = order.UserId,
                    OrderId = order.Id,
                    Type = "CHARGE",
                    Amount = order.SettlementAmount ?? 0m,
                    RemainingAmount = order.SettlementAmount ?? 0m,
                    Memo = $"주문 정산 청구 (정산시작): {order.OrderNo}"
                };
                await _orderRepository.AddReceivableAsync(charge);
            }

            if (oldStatus != request.Status && request.Status == "SETTLED")
            {
                if (request.SettlementAmount.HasValue && request.SettlementAmount.Value > 0)
                {
                    await _receivableService.ProcessDepositAsync(new CreateDepositDto
                    {
                        UserId = order.UserId,
                        OrderId = order.Id,
                        Amount = request.SettlementAmount.Value,
                        Memo = $"주문({order.OrderNo}) 정산 확인 시 실수납 처리"
                    });
                }
            }
        }

        await _orderRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<bool>> DeleteOrderAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return ApiResponse<bool>.Failure("Order not found", 404);

        if (order.Status.ToUpper() != "CANCELLED" && order.Status.ToUpper() != "SETTLED_CANCELLED")
        {
            return ApiResponse<bool>.Failure("취소된 주문건만 삭제할 수 있습니다.", 400);
        }

        _orderRepository.Delete(order);
        await _orderRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<List<OrderStatusHistoryDto>>> GetOrderHistoryAsync(int orderId)
    {
        var history = await _orderRepository.GetOrderHistoryAsync(orderId);
        return ApiResponse<List<OrderStatusHistoryDto>>.Success(history);
    }

    public async Task<ApiResponse<bool>> SaveOrderStatementAsync(SaveOrderStatementDto request)
    {
        var existing = await _orderRepository.GetOrderStatementAsync(request.OrderId);

        if (existing != null)
        {
            existing.SnapshotData = request.SnapshotData;
            if (!string.IsNullOrEmpty(request.PdfBase64))
            {
                existing.PdfBinary = Convert.FromBase64String(request.PdfBase64);
            }
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            var statement = new OrderStatement
            {
                OrderId = request.OrderId,
                SnapshotData = request.SnapshotData,
                PdfBinary = !string.IsNullOrEmpty(request.PdfBase64) 
                            ? Convert.FromBase64String(request.PdfBase64) 
                            : null,
                CreatedAt = DateTime.UtcNow
            };
            await _orderRepository.AddOrderStatementAsync(statement);
        }

        await _orderRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<OrderStatementDto>> GetOrderStatementAsync(int orderId)
    {
        var statement = await _orderRepository.GetOrderStatementDtoAsync(orderId);

        if (statement == null)
        {
            return ApiResponse<OrderStatementDto>.Failure("Stored statement not found", 404);
        }

        return ApiResponse<OrderStatementDto>.Success(statement);
    }

    public async Task<ApiResponse<OrderStatementDto>> GetOrderStatementPdfAsync(int orderId)
    {
        var statement = await _orderRepository.GetOrderStatementPdfDtoAsync(orderId);

        if (statement == null)
        {
            return ApiResponse<OrderStatementDto>.Failure("Stored statement pdf not found", 404);
        }

        return ApiResponse<OrderStatementDto>.Success(statement);
    }
}
