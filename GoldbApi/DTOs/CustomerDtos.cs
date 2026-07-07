using System;
using System.Collections.Generic;

namespace GoldbApi.DTOs;

public class CustomerListRequest
{

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public DateTime? BirthDate { get; set; }

    public int? CompanyId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class CustomerListResponse
{

    public int Total { get; set; }

    public List<CustomerDto> Items { get; set; } = new();
}

public class CustomerDto
{

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Note { get; set; }

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public string? CreatorName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CustomerCreateRequest
{

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Note { get; set; }
}

public class CustomerUpdateRequest : CustomerCreateRequest
{
}
