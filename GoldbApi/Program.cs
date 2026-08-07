using Microsoft.EntityFrameworkCore;
using Serilog;
using FluentValidation;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using GoldbApi.Middlewares;
using GoldbApi.Services;
using Spectre.Console;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

var builder = WebApplication.CreateBuilder(args);

GoldbApi.Mappings.MappingConfig.Configure();

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration["GOLDB"] ?? Environment.GetEnvironmentVariable("GOLDB");

builder.Services.AddDbContext<GoldbApi.Data.AppDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "goldb")));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "supersecretkey_for_goldenbar_1234567890");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<GoldbApi.Services.IAuthService, GoldbApi.Services.AuthService>();
builder.Services.AddScoped<GoldbApi.Services.IArticleService, GoldbApi.Services.ArticleService>();
builder.Services.AddScoped<GoldbApi.Services.IMockDataService, GoldbApi.Services.MockDataService>();
builder.Services.AddScoped<GoldbApi.Services.IRoleService, GoldbApi.Services.RoleService>();
builder.Services.AddScoped<GoldbApi.Services.IUserService, GoldbApi.Services.UserService>();
builder.Services.AddScoped<GoldbApi.Services.ICommonCodeService, GoldbApi.Services.CommonCodeService>();
builder.Services.AddScoped<GoldbApi.Services.IDashboardService, GoldbApi.Services.DashboardService>();
builder.Services.AddScoped<GoldbApi.Services.ICompanyService, GoldbApi.Services.CompanyService>();
builder.Services.AddScoped<GoldbApi.Services.ICustomerService, GoldbApi.Services.CustomerService>();
builder.Services.AddScoped<GoldbApi.Services.IFileService, GoldbApi.Services.FileService>();
builder.Services.AddScoped<GoldbApi.Services.IGoldPriceService, GoldbApi.Services.GoldPriceService>();
builder.Services.AddScoped<GoldbApi.Services.IDiamondPriceService, GoldbApi.Services.DiamondPriceService>();
builder.Services.AddScoped<GoldbApi.Services.IProductService, GoldbApi.Services.ProductService>();
builder.Services.AddScoped<GoldbApi.Services.ICatalogService, GoldbApi.Services.CatalogService>();
builder.Services.AddScoped<GoldbApi.Services.IProductSetService, GoldbApi.Services.ProductSetService>();
builder.Services.AddScoped<GoldbApi.Services.IPlasterOrderService, GoldbApi.Services.PlasterOrderService>();
builder.Services.AddScoped<GoldbApi.Services.IStockService, GoldbApi.Services.StockService>();
builder.Services.AddScoped<GoldbApi.Services.ICartService, GoldbApi.Services.CartService>();
builder.Services.AddScoped<GoldbApi.Services.INoticeService, GoldbApi.Services.NoticeService>();
builder.Services.AddScoped<GoldbApi.Services.IFavoriteService, GoldbApi.Services.FavoriteService>();
builder.Services.AddScoped<GoldbApi.Services.IMenuFavoriteService, GoldbApi.Services.MenuFavoriteService>();
builder.Services.AddScoped<GoldbApi.Services.IUserMenuSettingService, GoldbApi.Services.UserMenuSettingService>();
builder.Services.AddScoped<GoldbApi.Services.IUserPersonalSettingService, GoldbApi.Services.UserPersonalSettingService>();
builder.Services.AddScoped(typeof(GoldbApi.Repositories.IRepository<>), typeof(GoldbApi.Repositories.RepositoryBase<>));
builder.Services.AddScoped<GoldbApi.Repositories.IOrderRepository, GoldbApi.Repositories.OrderRepository>();
builder.Services.AddScoped<GoldbApi.Repositories.IStockRepository, GoldbApi.Repositories.StockRepository>();
builder.Services.AddScoped<GoldbApi.Repositories.IReceivableRepository, GoldbApi.Repositories.ReceivableRepository>();
builder.Services.AddScoped<GoldbApi.Services.IOrderService, GoldbApi.Services.OrderService>();
builder.Services.AddScoped<GoldbApi.Services.IReceivableService, GoldbApi.Services.ReceivableService>();
builder.Services.AddScoped<GoldbApi.Services.IPayableService, GoldbApi.Services.PayableService>();
builder.Services.AddScoped<GoldbApi.Services.ISearchService, GoldbApi.Services.SearchService>();
builder.Services.AddHostedService<GoldbApi.Services.MaterializedViewRefreshService>();
builder.Services.AddHostedService<GoldbApi.Services.LogisticsAutoApprovalService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600;
    options.ValueLengthLimit = 104857600;
    options.MultipartHeadersLengthLimit = 104857600;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new KstDateTimeConverter());
    options.SerializerOptions.Converters.Add(new KstNullableDateTimeConverter());
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new KstDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new KstNullableDateTimeConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Goldb API",
        Version = "v1",
        Description = "Goldb API"
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

var storagePath = builder.Configuration["Storage:LocalPath"];
if (string.IsNullOrEmpty(storagePath))
{
    storagePath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
}

if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

var uploadPath = Path.Combine(storagePath, "uploads");

if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

var fallbackUrl = builder.Configuration["Storage:FallbackUrl"];
if (!string.IsNullOrEmpty(fallbackUrl))
{
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value;
        if (path != null && 
            (path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) || 
             path.StartsWith("/upload/", StringComparison.OrdinalIgnoreCase)))
        {

            if (path.StartsWith("/upload/", StringComparison.OrdinalIgnoreCase) && 
                !path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                path = "/uploads" + path.Substring(7);
            }

            var targetUrl = $"{fallbackUrl.TrimEnd('/')}{path}";
            context.Response.Redirect(targetUrl);
            return;
        }
        await next();
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

GoldbApi.Endpoints.AuthEndpoints.MapAuthEndpoints(app);
GoldbApi.Endpoints.ArticleEndpoints.MapArticleEndpoints(app);
GoldbApi.Endpoints.MockDataEndpoints.MapMockDataEndpoints(app);
GoldbApi.Endpoints.RoleEndpoints.MapRoleEndpoints(app);
GoldbApi.Endpoints.UserEndpoints.MapUserEndpoints(app);
GoldbApi.Endpoints.CommonCodeEndpoints.MapCommonCodeEndpoints(app);
GoldbApi.Endpoints.DashboardEndpoints.MapDashboardEndpoints(app);
GoldbApi.Endpoints.CompanyEndpoints.MapCompanyEndpoints(app);
GoldbApi.Endpoints.CustomerEndpoints.MapCustomerEndpoints(app);
GoldbApi.Endpoints.FileEndpoints.MapFileEndpoints(app);
GoldbApi.Endpoints.GoldPriceEndpoints.MapGoldPriceEndpoints(app);
GoldbApi.Endpoints.DiamondPriceEndpoints.MapDiamondPriceEndpoints(app);
GoldbApi.Endpoints.ProductEndpoints.MapProductEndpoints(app);
GoldbApi.Endpoints.CatalogEndpoints.MapCatalogEndpoints(app);
GoldbApi.Endpoints.ProductSetEndpoints.MapProductSetEndpoints(app);
GoldbApi.Endpoints.PlasterOrderEndpoints.MapPlasterOrderEndpoints(app);
GoldbApi.Endpoints.StockEndpoints.MapStockEndpoints(app);
GoldbApi.Endpoints.CartEndpoints.MapCartEndpoints(app);
GoldbApi.Endpoints.NoticeEndpoints.MapNoticeEndpoints(app);
GoldbApi.Endpoints.ContactEndpoints.MapContactEndpoints(app);
GoldbApi.Endpoints.FavoriteEndpoints.MapFavoriteEndpoints(app);
GoldbApi.Endpoints.MenuFavoriteEndpoints.MapMenuFavoriteEndpoints(app);
GoldbApi.Endpoints.OrderEndpoints.MapOrderEndpoints(app);
GoldbApi.Endpoints.ReceivableEndpoints.MapReceivableEndpoints(app);
GoldbApi.Endpoints.PayableEndpoints.MapPayableEndpoints(app);
GoldbApi.Endpoints.SearchEndpoints.MapSearchEndpoints(app);

app.MapGet("/api/seed", async (GoldbApi.Data.AppDbContext dbContext) =>
{
    await dbContext.Database.ExecuteSqlRawAsync(@"
        UPDATE goldb.menus SET parent_id = 116, is_deleted = false WHERE id IN (201, 202, 203, 204);
        DELETE FROM goldb.menu_permissions WHERE menu_id IN (87, 134, 135, 136, 116, 201, 202, 203, 204, 205);
        UPDATE goldb.menus SET is_deleted = true WHERE id IN (87, 134, 135, 136);

        INSERT INTO goldb.menu_permissions (role_key, menu_id, can_search, can_create, can_delete, can_save, can_print, custom1, custom2, custom3, custom4, custom5, custom6, custom7, custom8, created_at, is_deleted)
        SELECT r.role_key, m.menu_id, true, true, true, true, true, true, true, true, true, true, true, true, true, NOW(), false
        FROM (VALUES ('admin'), ('editor'), ('dc'), ('mfg'), ('retail'), ('market'), ('manufacturer')) AS r(role_key)
        CROSS JOIN (VALUES (116), (201), (202), (203), (204)) AS m(menu_id);

        -- 1. Factory (mfg, manufacturer): NO 139 (물류 승인 내역)
        DELETE FROM goldb.menu_permissions WHERE role_key IN ('mfg', 'manufacturer') AND menu_id = 139;

        -- 2. Logistics (dc, market): NO 131 (공장의뢰내역)
        DELETE FROM goldb.menu_permissions WHERE role_key IN ('dc', 'market') AND menu_id = 131;

        -- 3. Retailer (editor, retail): NO 131 AND NO 139
        DELETE FROM goldb.menu_permissions WHERE role_key IN ('editor', 'retail') AND menu_id IN (131, 139);

        -- Ensure Factory has 131 (공장의뢰내역)
        DELETE FROM goldb.menu_permissions WHERE role_key IN ('mfg', 'manufacturer') AND menu_id = 131;
        INSERT INTO goldb.menu_permissions (role_key, menu_id, can_search, can_create, can_delete, can_save, can_print, custom1, custom2, custom3, custom4, custom5, custom6, custom7, custom8, created_at, is_deleted)
        SELECT r.role_key, 131, true, true, true, true, true, true, true, true, true, true, true, true, true, NOW(), false
        FROM (VALUES ('mfg'), ('manufacturer')) AS r(role_key);

        -- Ensure Logistics has 139 (물류 승인 내역)
        DELETE FROM goldb.menu_permissions WHERE role_key IN ('dc', 'market') AND menu_id = 139;
        INSERT INTO goldb.menu_permissions (role_key, menu_id, can_search, can_create, can_delete, can_save, can_print, custom1, custom2, custom3, custom4, custom5, custom6, custom7, custom8, created_at, is_deleted)
        SELECT r.role_key, 139, true, true, true, true, true, true, true, true, true, true, true, true, true, NOW(), false
        FROM (VALUES ('dc'), ('market')) AS r(role_key);

        -- 4. Employee Management (직원 관리 - views/sys/employees) Menu 205 under parent 125 (거래처)
        DELETE FROM goldb.menus WHERE id = 205;
        INSERT INTO goldb.menus (id, parent_id, path, component, name, title, redirect, sort_order, is_mobile, is_deleted, created_at)
        VALUES (205, 125, 'employee', 'views/sys/employees', 'Employees', '직원 관리', '', 10, false, false, NOW());

        INSERT INTO goldb.menu_permissions (role_key, menu_id, can_search, can_create, can_delete, can_save, can_print, custom1, custom2, custom3, custom4, custom5, custom6, custom7, custom8, created_at, is_deleted)
        SELECT r.role_key, 205, true, true, true, true, true, true, true, true, true, true, true, true, true, NOW(), false
        FROM (VALUES ('admin'), ('editor'), ('dc'), ('mfg'), ('retail'), ('market'), ('manufacturer')) AS r(role_key);

        DELETE FROM goldb.menu_permissions WHERE menu_id = 125;
        INSERT INTO goldb.menu_permissions (role_key, menu_id, can_search, can_create, can_delete, can_save, can_print, custom1, custom2, custom3, custom4, custom5, custom6, custom7, custom8, created_at, is_deleted)
        SELECT r.role_key, 125, true, true, true, true, true, true, true, true, true, true, true, true, true, NOW(), false
        FROM (VALUES ('admin'), ('editor'), ('dc'), ('mfg'), ('retail'), ('market'), ('manufacturer')) AS r(role_key);
        INSERT INTO goldb.gold_prices (announced_at, platinum, pure_gold, gold18_k, gold14_k, silver, is_deleted, created_at)
        SELECT NOW(), 350000, 450000, 330000, 260000, 5000, false, NOW()
        WHERE NOT EXISTS (SELECT 1 FROM goldb.gold_prices);
        ALTER TABLE goldb.stocks ADD COLUMN IF NOT EXISTS quantity integer NOT NULL DEFAULT 1;
        ALTER TABLE goldb.stocks ADD COLUMN IF NOT EXISTS size character varying(50);
        ALTER TABLE goldb.receivables ADD COLUMN IF NOT EXISTS weight numeric NOT NULL DEFAULT 0;
        ALTER TABLE goldb.receivables ADD COLUMN IF NOT EXISTS remaining_weight numeric NOT NULL DEFAULT 0;
        ALTER TABLE goldb.receivables ADD COLUMN IF NOT EXISTS discount numeric NOT NULL DEFAULT 0;
        ALTER TABLE goldb.receivables ADD COLUMN IF NOT EXISTS is_cancelled boolean NOT NULL DEFAULT false;
        ALTER TABLE goldb.cart_items ADD COLUMN IF NOT EXISTS size character varying(50);
        ALTER TABLE goldb.cart_items ADD COLUMN IF NOT EXISTS memo character varying(500);
        UPDATE goldb.menus SET is_deleted = true WHERE path = 'partner-retailers' OR name = 'PartnerRetailers';
    ");

    if (!dbContext.GoldPrices.Any())
    {
        dbContext.GoldPrices.Add(new GoldbApi.Models.GoldPrice
        {
            PureGold = 100000,
            Gold18K = 80000,
            Gold14K = 60000,
            Platinum = 50000,
            Silver = 1000,
            AnnouncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

    var roles = new[] { 
        new { Key = "admin", Name = "관리자" }, 
        new { Key = "manufacturer", Name = "제조사" }, 
        new { Key = "retail", Name = "소매점" }, 
        new { Key = "market", Name = "도매/마켓" },
        new { Key = "dc", Name = "물류" },
        new { Key = "logistics", Name = "물류" }
    };

    foreach (var r in roles)
    {
        if (!dbContext.Roles.Any(x => x.Key == r.Key))
            dbContext.Roles.Add(new GoldbApi.Models.Role { Key = r.Key, Name = r.Name });
    }
    await dbContext.SaveChangesAsync();

    var companies = new[]
    {
        new GoldbApi.Models.Company { Name = "Admin Company", CEO = "Admin", Region = "Toshkent", BusinessNumber = "111", Category = "ADMIN" },
        new GoldbApi.Models.Company { Name = "MFG Company", CEO = "Mfg", Region = "Toshkent", BusinessNumber = "222", Category = "MFG" },
        new GoldbApi.Models.Company { Name = "RTL Company", CEO = "Rtl", Region = "Toshkent", BusinessNumber = "333", Category = "RTL" },
        new GoldbApi.Models.Company { Name = "Market Company", CEO = "Mkt", Region = "Toshkent", BusinessNumber = "444", Category = "DCC" }
    };

    foreach(var c in companies) {
        if (!dbContext.Companies.Any(x => x.Name == c.Name))
            dbContext.Companies.Add(c);
    }
    await dbContext.SaveChangesAsync();

    var users = new[]
    {
        new { Username = "admin", RoleKey = "admin", CompanyName = "Admin Company" },
        new { Username = "manufacturer", RoleKey = "manufacturer", CompanyName = "MFG Company" },
        new { Username = "retail", RoleKey = "retail", CompanyName = "RTL Company" },
        new { Username = "market", RoleKey = "market", CompanyName = "Market Company" }
    };

    foreach (var u in users)
    {
        if (!dbContext.Users.Any(x => x.Username == u.Username))
        {
            var user = new GoldbApi.Models.User
            {
                Username = u.Username,
                Password = passwordHash,
                Name = u.Username.ToUpper(),
                UserType = u.RoleKey.ToUpper()
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var role = dbContext.Roles.First(r => r.Key == u.RoleKey);
            dbContext.UserRoles.Add(new GoldbApi.Models.UserRole { UserId = user.Id, RoleId = role.Id });

            var company = dbContext.Companies.First(c => c.Name == u.CompanyName);
            dbContext.UserCompanies.Add(new GoldbApi.Models.UserCompany { UserId = user.Id, CompanyId = company.Id });
        }
    }

    await dbContext.SaveChangesAsync();
    return Results.Ok("Seeding complete.");
});
string GetServerName()

{
    return Environment.GetEnvironmentVariable("SERVER_NAME")
        ?? Assembly.GetEntryAssembly()?.GetName().Name
        ?? typeof(Program).Namespace
        ?? "API";
}

app.Lifetime.ApplicationStarted.Register(() =>
{var serverName = GetServerName();
    var env = app.Environment.EnvironmentName;
    var pid = Environment.ProcessId;

    var color = serverName.ToUpper() switch
    {
        "GATEWAY" => Color.Blue,
        "GOLDBAPI" => Color.Green,
        "PAYMENT" => Color.Yellow,
        "AUTH" => Color.Purple,
        _ => Color.White
    };

    var envColor = env switch
    {
        "Development" => Color.Green,
        "Staging" => Color.Yellow,
        "Production" => Color.Red,
        _ => Color.Grey
    };

    var urlLines = app.Urls.Select(url =>
    {
        try
        {
            var uri = new Uri(url);
            return $"[blue]🌐 {url}[/]  [grey](PORT: {uri.Port})[/]";
        }
        catch
        {
            return $"[blue]🌐 {url}[/]";
        }
    });

    var panelContent =
        $"[bold {color}]{serverName} SERVICE STARTED[/]\n" +
        $"[yellow]PID:[/] {pid}\n" +
        $"[bold {envColor}]ENV:[/] {env}\n\n" +
        string.Join("\n", urlLines);

    var panel = new Panel(panelContent)
        .Border(BoxBorder.Double)
        .BorderColor(color)
        .Padding(1, 1);

    AnsiConsole.Write(panel);
});

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public class KstDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
{
    private static readonly TimeZoneInfo KstZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

    public override DateTime Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.String && DateTime.TryParse(reader.GetString(), out var date))
        {
            return date;
        }
        return reader.GetDateTime();
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime value, System.Text.Json.JsonSerializerOptions options)
    {
        var kstTime = value.Kind switch
        {
            DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(value, KstZone),
            DateTimeKind.Unspecified => TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(value, DateTimeKind.Utc), KstZone),
            _ => TimeZoneInfo.ConvertTime(value, KstZone)
        };

        writer.WriteStringValue(kstTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}

public class KstNullableDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime?>
{
    private static readonly TimeZoneInfo KstZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

    public override DateTime? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType == System.Text.Json.JsonTokenType.String && DateTime.TryParse(reader.GetString(), out var date))
        {
            return date;
        }
        return reader.GetDateTime();
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime? value, System.Text.Json.JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var val = value.Value;
        var kstTime = val.Kind switch
        {
            DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(val, KstZone),
            DateTimeKind.Unspecified => TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(val, DateTimeKind.Utc), KstZone),
            _ => TimeZoneInfo.ConvertTime(val, KstZone)
        };

        writer.WriteStringValue(kstTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}
