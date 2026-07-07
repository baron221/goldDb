namespace GoldbApi.DTOs;

public class ArticleListRequest
{

    public int? Importance { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public int Page { get; set; } = 1;

    public int Limit { get; set; } = 20;

    public string? Sort { get; set; }
}

public class ArticleListResponse
{

    public int Total { get; set; }

    public List<GoldbApi.Models.Article> Items { get; set; } = new();
}

public class TransactionListResponse
{

    public int Total { get; set; }

    public List<GoldbApi.Models.Transaction> Items { get; set; } = new();
}

public class UserSearchResponse
{

    public List<UserSearchItem> Items { get; set; } = new();
}

public class UserSearchItem
{

    public string Name { get; set; } = string.Empty;
}
