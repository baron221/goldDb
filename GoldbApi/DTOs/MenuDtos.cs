namespace GoldbApi.DTOs;

public class MenuResponse
{

    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string Path { get; set; } = string.Empty;

    public string? Component { get; set; }

    public string? Name { get; set; }

    public string? Redirect { get; set; }

    public int SortOrder { get; set; }

    public int AffixSortOrder { get; set; }

    public bool IsMobile { get; set; }

    public bool IsFavorite { get; set; }

    public int FavoriteSortOrder { get; set; }

    public MenuMeta? Meta { get; set; }

    public List<MenuResponse>? Children { get; set; }
}

public class MenuMeta
{

    public string? Title { get; set; }

    public string? Icon { get; set; }

    public bool? NoCache { get; set; }

    public bool? Affix { get; set; }

    public int AffixSortOrder { get; set; }

    public bool? Hidden { get; set; }

    public bool? AlwaysShow { get; set; }

    public string? ActiveMenu { get; set; }

    public bool? AllowDuplicate { get; set; }

    public bool IsMobile { get; set; }

    public bool IsFavorite { get; set; }

    public int FavoriteSortOrder { get; set; }

    public bool CanSearch { get; set; }

    public bool CanCreate { get; set; }

    public bool CanDelete { get; set; }

    public bool CanSave { get; set; }

    public bool CanPrint { get; set; }

    public bool Custom1 { get; set; }

    public bool Custom2 { get; set; }

    public bool Custom3 { get; set; }

    public bool Custom4 { get; set; }

    public bool Custom5 { get; set; }

    public bool Custom6 { get; set; }

    public bool Custom7 { get; set; }

    public bool Custom8 { get; set; }
}

public class MenuSaveRequest
{

    public int? Id { get; set; }

    public int? ParentId { get; set; }

    public string Path { get; set; } = string.Empty;

    public string? Component { get; set; }

    public string? Name { get; set; }

    public string? Redirect { get; set; }

    public string? Title { get; set; }

    public string? Icon { get; set; }

    public bool? NoCache { get; set; }

    public bool? Affix { get; set; }

    public bool? Hidden { get; set; }

    public bool? AlwaysShow { get; set; }

    public string? ActiveMenu { get; set; }

    public bool? AllowDuplicate { get; set; }

    public int SortOrder { get; set; }

    public bool IsMobile { get; set; }
}

public class MenuBatchUpdateItem
{

    public int Id { get; set; }

    public int? ParentId { get; set; }

    public int SortOrder { get; set; }

    public bool IsMobile { get; set; }
}

public class MenuBatchUpdateRequest
{

    public List<MenuBatchUpdateItem> Items { get; set; } = new();
}

public class UserMenuSettingDto
{

    public int MenuId { get; set; }

    public bool? Affix { get; set; }

    public int SortOrder { get; set; }

    public string? Title { get; set; }
}

public class UserMenuSettingUpdateRequest
{

    public int MenuId { get; set; }

    public bool? Affix { get; set; }

    public int? SortOrder { get; set; }
}

public class UserMenuSettingSortRequest
{

    public List<UserMenuSettingSortItem> Items { get; set; } = new();
}

public class UserMenuSettingSortItem
{

    public int MenuId { get; set; }

    public int SortOrder { get; set; }
}

public class MenuFavoriteResponse
{

    public int MenuId { get; set; }

    public string? Title { get; set; }

    public int SortOrder { get; set; }
}

public class MenuFavoriteSortRequest
{

    public List<MenuFavoriteSortItem> Items { get; set; } = new();
}

public class MenuFavoriteSortItem
{

    public int MenuId { get; set; }

    public int SortOrder { get; set; }
}
