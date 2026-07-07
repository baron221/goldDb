using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GoldbApi.Services;

public interface IReceivableService
{

    Task<ApiResponse<PagedResult<UserReceivableSummaryDto>>> GetUserSummariesAsync(int page, int pageSize, string? search);

    Task<ApiResponse<PagedResult<ReceivableDto>>> GetReceivablesAsync(ReceivableQueryDto query);

    Task<ApiResponse<bool>> ProcessDepositAsync(CreateDepositDto request);

    Task<ApiResponse<LogisticsReceivableSummaryDto>> GetLogisticsSummaryAsync();
}

public class ReceivableService : IReceivableService
{
    private readonly IReceivableRepository _receivableRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReceivableService> _logger;

    public ReceivableService(IReceivableRepository receivableRepository, ICurrentUserService currentUserService, ILogger<ReceivableService> logger)
    {
        _receivableRepository = receivableRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }

    private async Task<int> GetCurrentCompanyIdAsync()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("GetCurrentCompanyIdAsync: Searching for UserId: {UserId}", userId);

        if (userId == 0) return 0;

        var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);

        if (user == null) {
            _logger.LogWarning("GetCurrentCompanyIdAsync: User not found for ID: {UserId}", userId);
            return 0;
        }

        _logger.LogInformation("GetCurrentCompanyIdAsync: Found user {Username}, associated companies: {Companies}", user.Username, string.Join(", ", user.UserCompanies.Select(uc => $"{uc.Company?.Name} (Cat: {uc.Company?.Category})")));

        var company = user.UserCompanies
            .Select(uc => uc.Company)
            .FirstOrDefault(c => c != null && c.Category == "DCC");

        if (company == null) {
            _logger.LogWarning("GetCurrentCompanyIdAsync: No DCC company found for user {Username}", user.Username);
        } else {
            _logger.LogInformation("GetCurrentCompanyIdAsync: Found DCC company: {CompanyName} (ID: {Id})", company.Name, company.Id);
        }

        return company?.Id ?? 0;
    }

    public async Task<ApiResponse<LogisticsReceivableSummaryDto>> GetLogisticsSummaryAsync()
    {
        var currentLogisticsCompanyId = await GetCurrentCompanyIdAsync();
        if (currentLogisticsCompanyId == 0)
        {
            return ApiResponse<LogisticsReceivableSummaryDto>.Failure("물류센터 소속 정보를 찾을 수 없습니다.", 400);
        }

        var managedRetailers = await _receivableRepository.GetManagedRetailersAsync(currentLogisticsCompanyId);
        var managedRetailerIds = managedRetailers.Select(r => r.Id).ToList();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var receivables = await _receivableRepository.GetReceivablesForLogisticsAsync(managedRetailerIds);

        _logger.LogInformation("GetLogisticsSummaryAsync: Found {Count} receivables for {RetailerCount} retailers", receivables.Count, managedRetailers.Count);
        foreach(var r in receivables.Take(5)) {
            _logger.LogInformation("Receivable ID: {Id}, Type: {Type}, Amount: {Amount}, UserId: {UserId}", r.Id, r.Type, r.Amount, r.UserId);
        }

        var totalUnpaid = receivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);

        var currentMonthCharge = receivables
            .Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonth)
            .Sum(r => r.Amount);

        var currentMonthDeposit = receivables
            .Where(r => r.Type == "DEPOSIT" && r.CreatedAt >= startOfMonth)
            .Sum(r => r.Amount);

        var currentMonthUnpaid = receivables
            .Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonth)
            .Sum(r => r.RemainingAmount);

        var retailerSummaries = new List<RetailerReceivableSummaryDto>();

        foreach (var retailer in managedRetailers)
        {
            var retailerReceivables = receivables.Where(r => r.User != null && r.User.UserCompanies.Any(uc => uc.CompanyId == retailer.Id)).ToList();

            retailerSummaries.Add(new RetailerReceivableSummaryDto
            {
                CompanyId = retailer.Id,
                CompanyName = retailer.Name,
                TotalUnpaid = retailerReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount),
                CurrentMonthCharge = retailerReceivables.Where(r => r.Type == "CHARGE" && r.CreatedAt >= startOfMonth).Sum(r => r.Amount),
                CurrentMonthDeposit = retailerReceivables.Where(r => r.Type == "DEPOSIT" && r.CreatedAt >= startOfMonth).Sum(r => r.Amount)
            });
        }

        var summary = new LogisticsReceivableSummaryDto
        {
            TotalUnpaid = totalUnpaid,
            CurrentMonthCharge = currentMonthCharge,
            CurrentMonthDeposit = currentMonthDeposit,
            CurrentMonthUnpaid = currentMonthUnpaid,
            RetailerSummaries = retailerSummaries
        };

        return ApiResponse<LogisticsReceivableSummaryDto>.Success(summary);
    }

    public async Task<ApiResponse<PagedResult<UserReceivableSummaryDto>>> GetUserSummariesAsync(int page, int pageSize, string? search)
    {
        var userId = GetCurrentUserId();
        if (userId != 0)
        {
            var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);

            if (user != null)
            {
                var isRetailer = user.UserCompanies
                    .Any(uc => uc.Company != null && uc.Company.Category == "RTL");

                if (isRetailer)
                {
                    search = user.Username;
                }
            }
        }

        var userIdsWithReceivables = await _receivableRepository.GetUserIdsWithReceivablesAsync();
        var totalCount = await _receivableRepository.GetUsersWithReceivablesCountAsync(search, userIdsWithReceivables);
        var users = await _receivableRepository.GetUsersWithReceivablesPagedAsync(page, pageSize, search, userIdsWithReceivables);

        var summaries = new List<UserReceivableSummaryDto>();
        foreach (var user in users)
        {
            var userReceivables = await _receivableRepository.GetReceivablesForUserAsync(user.Id);

            var totalCharge = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.Amount);
            var totalDeposit = userReceivables.Where(r => r.Type == "DEPOSIT").Sum(r => r.Amount);
            var totalReceivable = userReceivables.Where(r => r.Type == "CHARGE").Sum(r => r.RemainingAmount);

            summaries.Add(new UserReceivableSummaryDto
            {
                UserId = user.Id,
                UserName = user.Username,
                UserDisplayName = user.Name,
                CompanyName = user.UserCompanies.FirstOrDefault()?.Company?.Name,
                TotalCharge = totalCharge,
                TotalDeposit = totalDeposit,
                TotalReceivable = totalReceivable
            });
        }

        return ApiResponse<PagedResult<UserReceivableSummaryDto>>.Success(new PagedResult<UserReceivableSummaryDto>
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<PagedResult<ReceivableDto>>> GetReceivablesAsync(ReceivableQueryDto query)
    {
        var userId = GetCurrentUserId();
        if (userId != 0)
        {
            var user = await _receivableRepository.GetUserWithCompaniesAsync(userId);

            if (user != null)
            {
                var isRetailer = user.UserCompanies
                    .Any(uc => uc.Company != null && uc.Company.Category == "RTL");

                if (isRetailer)
                {
                    query.UserId = userId;
                }
            }
        }

        var (items, totalCount) = await _receivableRepository.GetReceivablesPagedAsync(query);

        return ApiResponse<PagedResult<ReceivableDto>>.Success(new PagedResult<ReceivableDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<bool>> ProcessDepositAsync(CreateDepositDto request)
    {
        if (request.Amount <= 0)
        {
            return ApiResponse<bool>.Failure("입금액은 0보다 커야 합니다.", 400);
        }

        var deposit = new Receivable
        {
            UserId = request.UserId,
            OrderId = request.OrderId,
            Type = "DEPOSIT",
            Amount = request.Amount,
            RemainingAmount = 0,
            Memo = request.Memo,
            SettlementMethod = request.SettlementMethod
        };
        await _receivableRepository.AddAsync(deposit);

        decimal remainingDepositToApply = request.Amount;

        if (request.OrderId.HasValue)
        {
            var targetCharges = await _receivableRepository.GetTargetChargesAsync(request.UserId, request.OrderId.Value);

            foreach (var charge in targetCharges)
            {
                if (remainingDepositToApply <= 0) break;

                if (charge.RemainingAmount <= remainingDepositToApply)
                {
                    remainingDepositToApply -= charge.RemainingAmount;
                    charge.RemainingAmount = 0;
                }
                else
                {
                    charge.RemainingAmount -= remainingDepositToApply;
                    remainingDepositToApply = 0;
                }
            }
        }

        if (remainingDepositToApply > 0)
        {
            var outstandingCharges = await _receivableRepository.GetOutstandingChargesAsync(request.UserId);

            foreach (var charge in outstandingCharges)
            {
                if (remainingDepositToApply <= 0) break;

                if (charge.RemainingAmount <= remainingDepositToApply)
                {
                    remainingDepositToApply -= charge.RemainingAmount;
                    charge.RemainingAmount = 0;
                }
                else
                {
                    charge.RemainingAmount -= remainingDepositToApply;
                    remainingDepositToApply = 0;
                }
            }
        }

        await _receivableRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }
}
