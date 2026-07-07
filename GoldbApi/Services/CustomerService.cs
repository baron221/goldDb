using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mapster;

namespace GoldbApi.Services;

public interface ICustomerService
{

    Task<ApiResponse<CustomerListResponse>> GetCustomersAsync(CustomerListRequest request);

    Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id);

    Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerCreateRequest request);

    Task<ApiResponse<string>> UpdateCustomerAsync(int id, CustomerUpdateRequest request);

    Task<ApiResponse<string>> DeleteCustomerAsync(int id);
}

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<UserCompany> _userCompanyRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly ICurrentUserService _currentUserService;

    public CustomerService(
        IRepository<Customer> customerRepository,
        IRepository<UserCompany> userCompanyRepository,
        IRepository<User> userRepository,
        IRepository<Company> companyRepository,
        ICurrentUserService currentUserService)
    {
        _customerRepository = customerRepository;
        _userCompanyRepository = userCompanyRepository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
    }

    private int GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }

    private int? GetUserCompanyId()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userCompany = _userCompanyRepository.GetQueryable().FirstOrDefault(uc => uc.UserId == userId);
            return userCompany?.CompanyId;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<CustomerListResponse>> GetCustomersAsync(CustomerListRequest request)
    {
        var query = _customerRepository.GetQueryable();

        var userCompanyId = GetUserCompanyId();
        if (userCompanyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == userCompanyId.Value);
        }
        else if (request.CompanyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == request.CompanyId.Value);
        }

        if (!string.IsNullOrEmpty(request.Name))
            query = query.Where(c => c.Name.Contains(request.Name));

        if (!string.IsNullOrEmpty(request.Phone))
            query = query.Where(c => c.Phone.Contains(request.Phone));

        if (request.BirthDate.HasValue)
        {
            var searchDate = request.BirthDate.Value.Date;
            query = query.Where(c => c.BirthDate.HasValue && c.BirthDate.Value.Date == searchDate);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<CustomerDto>()
            .ToListAsync();

        var creatorIds = items.Select(i => i.Id).ToList(); 

        return ApiResponse<CustomerListResponse>.Success(new CustomerListResponse
        {
            Total = total,
            Items = items
        });
    }

    public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id)
    {
        var c = await _customerRepository.GetQueryable()
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return ApiResponse<CustomerDto>.Failure("Customer not found", 404);

        var userCompanyId = GetUserCompanyId();
        if (userCompanyId.HasValue && c.CompanyId != userCompanyId.Value)
        {
            return ApiResponse<CustomerDto>.Failure("Unauthorized access to this customer", 403);
        }

        var dto = c.Adapt<CustomerDto>();

        if (c.CreatedBy.HasValue)
        {
            dto.CreatorName = await _userRepository.GetQueryable()
                .Where(u => u.Id == c.CreatedBy.Value)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();
        }

        return ApiResponse<CustomerDto>.Success(dto);
    }

    public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerCreateRequest request)
    {
        var userCompanyId = GetUserCompanyId();
        var currentUserId = GetCurrentUserId();

        var customer = request.Adapt<Customer>();
        customer.CompanyId = userCompanyId; 

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();

        var dto = customer.Adapt<CustomerDto>();

        if (userCompanyId.HasValue)
        {
            dto.CompanyName = await _companyRepository.GetQueryable()
                .Where(co => co.Id == userCompanyId.Value)
                .Select(co => co.Name)
                .FirstOrDefaultAsync();
        }
        dto.CreatorName = await _userRepository.GetQueryable()
            .Where(u => u.Id == currentUserId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync();

        return ApiResponse<CustomerDto>.Success(dto);
    }

    public async Task<ApiResponse<string>> UpdateCustomerAsync(int id, CustomerUpdateRequest request)
    {
        var customer = await _customerRepository.GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
        if (customer == null) return ApiResponse<string>.Failure("Customer not found", 404);

        var userCompanyId = GetUserCompanyId();
        if (userCompanyId.HasValue && customer.CompanyId != userCompanyId.Value)
        {
            return ApiResponse<string>.Failure("Unauthorized access", 403);
        }

        request.Adapt(customer);

        await _customerRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("Updated successfully");
    }

    public async Task<ApiResponse<string>> DeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository.GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
        if (customer == null) return ApiResponse<string>.Failure("Customer not found", 404);

        var userCompanyId = GetUserCompanyId();
        if (userCompanyId.HasValue && customer.CompanyId != userCompanyId.Value)
        {
            return ApiResponse<string>.Failure("Unauthorized access", 403);
        }

        _customerRepository.Delete(customer);
        await _customerRepository.SaveChangesAsync();

        return ApiResponse<string>.Success("Deleted successfully");
    }
}
