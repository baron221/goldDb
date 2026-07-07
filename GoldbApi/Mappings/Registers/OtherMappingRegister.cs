using Mapster;
using GoldbApi.Models;
using GoldbApi.DTOs;
using System.Linq;

namespace GoldbApi.Mappings.Registers;

public class OtherMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {

        config.NewConfig<PlasterOrder, PlasterOrderDto>()
            .Map(dest => dest.OrderingCompanyName, src => src.OrderingCompany != null ? src.OrderingCompany.Name : null)
            .Map(dest => dest.ManufacturerName, src => src.Manufacturer != null ? src.Manufacturer.Name : null);

        config.NewConfig<Company, CompanyDto>()
            .Map(dest => dest.LogisticsCompanyName, src => src.LogisticsCompany != null ? src.LogisticsCompany.Name : null)
            .Map(dest => dest.RetailerCount, src => src.Retailers.Count);

        config.NewConfig<Customer, CustomerDto>()
            .Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.Name : string.Empty);

        config.NewConfig<Notice, NoticeDto>();

        config.NewConfig<GoldPrice, GoldPriceDto>();

        config.NewConfig<DiamondPrice, DiamondPriceDto>();

        config.NewConfig<Favorite, FavoriteDto>();

        config.NewConfig<Catalog, CatalogDto>()
            .Map(dest => dest.Pages, src => src.CatalogPages.OrderBy(p => p.PageNumber));

        config.NewConfig<CatalogPage, CatalogPageDto>()
            .Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.Name : null)
            .Map(dest => dest.Products, src => src.CatalogPageProducts.Select(cpp => cpp.Product));
    }
}
