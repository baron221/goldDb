namespace GoldbApi.DTOs;

public class CompanyListRequest
{

    public string? Name { get; set; }

    public string? CEO { get; set; }

    public string? Phone { get; set; }

    public string? Category { get; set; }

    public string? Region { get; set; }

    public bool? IsDirectManagement { get; set; }

    public int? Page { get; set; }

    public int? PageSize { get; set; }
}

public class CompanyListResponse
{

    public int Total { get; set; }

    public List<CompanyDto> Items { get; set; } = new();
}

public class CompanyDto
{

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CEO { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string BusinessNumber { get; set; } = string.Empty;

    public string? CorporateNumber { get; set; }

    public string? BusinessLicenseFileUrl { get; set; }

    public string? BusinessType { get; set; }

    public string? BusinessCategory { get; set; }

    public string? Phone { get; set; }

    public string? AddressBase { get; set; }

    public string? AddressDetail { get; set; }

    public string? ZipCode { get; set; }

    public string? LogisticsCode { get; set; }

    public string? CenterNumber { get; set; }

    public bool IsDirectManagement { get; set; }

    public int? LogisticsCompanyId { get; set; }

    public string? LogisticsCompanyName { get; set; }

    public int RetailerCount { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? BankName { get; set; }

    public string? BankAccount { get; set; }

    public string? AccountHolder { get; set; }
}

public class CompanyCreateRequest
{

    public string Name { get; set; } = string.Empty;

    public string CEO { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string? BusinessNumber { get; set; }

    public string? CorporateNumber { get; set; }

    public string? BusinessLicenseFileUrl { get; set; }

    public string? BusinessType { get; set; }

    public string? BusinessCategory { get; set; }

    public string? Phone { get; set; }

    public string? AddressBase { get; set; }

    public string? AddressDetail { get; set; }

    public string? ZipCode { get; set; }

    public string? LogisticsCode { get; set; }

    public string? CenterNumber { get; set; }

    public bool IsDirectManagement { get; set; } = false;

    public string Category { get; set; } = "RTL";

    public int? LogisticsCompanyId { get; set; }

    public string? BankName { get; set; }

    public string? BankAccount { get; set; }

    public string? AccountHolder { get; set; }
}

public class CompanyUpdateRequest : CompanyCreateRequest
{
}
