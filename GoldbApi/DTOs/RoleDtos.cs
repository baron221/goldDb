namespace GoldbApi.DTOs;

public class RoleResponse
{

    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<MenuResponse>? Routes { get; set; }
}

public class RoleCreateRequest
{

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class RoleUpdateRequest
{

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class MenuPermissionDto
{

    public int MenuId { get; set; }

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

public class AssignMenusRequest
{

    public string RoleKey { get; set; } = string.Empty;

    public List<MenuPermissionDto> Permissions { get; set; } = new();
}

public class AssignUsersRequest
{

    public string RoleKey { get; set; } = string.Empty;

    public List<int> UserIds { get; set; } = new();
}

public class UserRoleResponse
{

    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsAssigned { get; set; }
}

public class MenuRoleResponse
{

    public int Id { get; set; }

    public string? Title { get; set; }

    public string Path { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public bool IsAssigned { get; set; }

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
