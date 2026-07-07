namespace GoldbApi.DTOs;

public class PlasterOrderDto
{

    public int Id { get; set; }

    public int? OrderingCompanyId { get; set; }

    public string? OrderingCompanyName { get; set; }

    public int? ManufacturerId { get; set; }

    public string? ManufacturerName { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public bool IsCancelled { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CreatePlasterOrderDto
{

    public int? OrderingCompanyId { get; set; }

    public int? ManufacturerId { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = "발주";

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public bool IsCancelled { get; set; } = false;

    public string Remarks { get; set; } = string.Empty;
}

public class PlasterOrderQueryDto
{

    public int? OrderingCompanyId { get; set; }

    public string? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
