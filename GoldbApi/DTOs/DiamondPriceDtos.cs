namespace GoldbApi.DTOs;

public class DiamondPriceDto
{

    public int Id { get; set; }

    public string PriceType { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    public decimal VirginPrice { get; set; }

    public decimal WooshinPrice { get; set; }

    public decimal HyundaiPrice { get; set; }

    public decimal GukjePrice { get; set; }

    public decimal GukboPrice { get; set; }

    public decimal OtherPrice { get; set; }

    public DateTime AnnouncedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateDiamondPriceDto
{

    public string PriceType { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    public decimal VirginPrice { get; set; }

    public decimal WooshinPrice { get; set; }

    public decimal HyundaiPrice { get; set; }

    public decimal GukjePrice { get; set; }

    public decimal GukboPrice { get; set; }

    public decimal OtherPrice { get; set; }

    public DateTime AnnouncedAt { get; set; } = DateTime.UtcNow;
}

public class UpdateDiamondPriceDto
{

    public string PriceType { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    public decimal VirginPrice { get; set; }

    public decimal WooshinPrice { get; set; }

    public decimal HyundaiPrice { get; set; }

    public decimal GukjePrice { get; set; }

    public decimal GukboPrice { get; set; }

    public decimal OtherPrice { get; set; }

    public DateTime AnnouncedAt { get; set; }
}
