namespace GoldbApi.DTOs;

public class FavoriteDto
{

    public int Id { get; set; }

    public int UserId { get; set; }

    public int? ProductId { get; set; }

    public ProductDto? Product { get; set; }

    public int? ProductSetId { get; set; }

    public ProductSetDto? ProductSet { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateFavoriteDto
{

    public int? ProductId { get; set; }

    public int? ProductSetId { get; set; }
}
