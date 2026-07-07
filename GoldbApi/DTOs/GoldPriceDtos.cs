namespace GoldbApi.DTOs;

public class GoldPriceDto
{

    public int Id { get; set; }

    public DateTime AnnouncedAt { get; set; }

    public decimal Platinum { get; set; }

    public decimal PureGold { get; set; }

    public decimal Gold18K { get; set; }

    public decimal Gold14K { get; set; }

    public decimal Silver { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateGoldPriceDto
{

    public DateTime AnnouncedAt { get; set; } = DateTime.UtcNow;

    public decimal Platinum { get; set; }

    public decimal PureGold { get; set; }

    public decimal Gold18K { get; set; }

    public decimal Gold14K { get; set; }

    public decimal Silver { get; set; }
}

public class UpdateGoldPriceDto
{

    public DateTime AnnouncedAt { get; set; }

    public decimal Platinum { get; set; }

    public decimal PureGold { get; set; }

    public decimal Gold18K { get; set; }

    public decimal Gold14K { get; set; }

    public decimal Silver { get; set; }
}
