using Microsoft.EntityFrameworkCore;
using GoldbApi.Models;
using GoldbApi.Models.Views;
using GoldbApi.Utils;
using GoldbApi.Services;
using System.Security.Claims;

namespace GoldbApi.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) 
        : base(options) 
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Menu> Menus { get; set; }

    public DbSet<Article> Articles { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<MenuPermission> MenuPermissions { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<UserCompany> UserCompanies { get; set; }

    public DbSet<UserPhoto> UserPhotos { get; set; }

    public DbSet<UserEmail> UserEmails { get; set; }

    public DbSet<UserPhone> UserPhones { get; set; }

    public DbSet<CommonCode> CommonCodes { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<GoldPrice> GoldPrices { get; set; }

    public DbSet<DiamondPrice> DiamondPrices { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<ProductPhoto> ProductPhotos { get; set; }

    public DbSet<Catalog> Catalogs { get; set; }

    public DbSet<CatalogPage> CatalogPages { get; set; }

    public DbSet<CatalogPageProduct> CatalogPageProducts { get; set; }

    public DbSet<ProductSet> ProductSets { get; set; }

    public DbSet<ProductSetItem> ProductSetItems { get; set; }

    public DbSet<ProductSetPhoto> ProductSetPhotos { get; set; }

    public DbSet<PlasterOrder> PlasterOrders { get; set; }

    public DbSet<Stock> Stocks { get; set; }

    public DbSet<CartItem> CartItems { get; set; }

    public DbSet<Attachment> Attachments { get; set; }

    public DbSet<Notice> Notices { get; set; }

    public DbSet<ContactMessage> ContactMessages { get; set; }

    public DbSet<Favorite> Favorites { get; set; }

    public DbSet<MenuFavorite> MenuFavorites { get; set; }

    public DbSet<UserMenuSetting> UserMenuSettings { get; set; }

    public DbSet<UserPersonalSetting> UserPersonalSettings { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public DbSet<Receivable> Receivables { get; set; }

    public DbSet<ProductOptionWeight> ProductOptionWeights { get; set; }

    public DbSet<OrderStatement> OrderStatements { get; set; }

    public DbSet<MvAdminDashboardSummary> MvAdminDashboardSummaries { get; set; }

    public DbSet<MvDailyOrderTrend> MvDailyOrderTrends { get; set; }

    public DbSet<MvMonthlyOrderTrend> MvMonthlyOrderTrends { get; set; }

    public DbSet<MvProductPerformance> MvProductPerformances { get; set; }

    public DbSet<MvPartnerRetailerStats> MvPartnerRetailerStats { get; set; }

    public DbSet<ManufacturerLogistics> ManufacturerLogistics { get; set; }

    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries();
        var utcNow = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        foreach (var entry in entries)
        {

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime) || property.Metadata.ClrType == typeof(DateTime?))
                {
                    if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                }
            }

            if (entry.Entity is BaseModel trackable)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        trackable.UpdatedAt = utcNow;
                        trackable.UpdatedBy = userId;
                        break;

                    case EntityState.Added:
                        trackable.CreatedAt = utcNow;
                        trackable.CreatedBy = userId;
                        break;

                    case EntityState.Deleted:

                        entry.State = EntityState.Modified;
                        trackable.IsDeleted = true;
                        trackable.UpdatedAt = utcNow;
                        trackable.UpdatedBy = userId;
                        break;
                }
            }
        }
    }

    private int? GetCurrentUserId()
    {
        return _currentUserService?.UserId;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("goldb");

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {

            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

            var tableDescription = entity.ClrType.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true)
                .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
            if (tableDescription != null)
            {
                entity.SetComment(tableDescription.Description);
            }

            foreach (var property in entity.GetProperties())
            {

                property.SetColumnName(property.Name.ToSnakeCase());

                var propDescription = property.PropertyInfo?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true)
                    .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                if (propDescription != null)
                {
                    property.SetComment(propDescription.Description);
                }
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName()?.ToSnakeCase());
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
            }

            if (typeof(BaseModel).IsAssignableFrom(entity.ClrType))
            {
                modelBuilder.Entity(entity.ClrType).HasQueryFilter(GenerateIsDeletedFilter(entity.ClrType));
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private static System.Linq.Expressions.LambdaExpression GenerateIsDeletedFilter(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "it");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseModel.IsDeleted));
        var falseConstant = System.Linq.Expressions.Expression.Constant(false);
        var equal = System.Linq.Expressions.Expression.Equal(property, falseConstant);
        return System.Linq.Expressions.Expression.Lambda(equal, parameter);
    }
}
