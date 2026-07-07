using Mapster;
using GoldbApi.Models;
using GoldbApi.DTOs;
using System.Linq;

namespace GoldbApi.Mappings.Registers;

public class ProductMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {

        config.NewConfig<Product, ProductSearchResultDto>()
            .Map(dest => dest.PhotoUrl, src => src.ProductPhotos.OrderBy(pp => pp.SortOrder).Select(pp => pp.PhotoUrl).FirstOrDefault());

        config.NewConfig<ProductSet, ProductSetSearchResultDto>()
            .Map(dest => dest.PhotoUrl, src => src.ProductSetPhotos.OrderBy(pp => pp.SortOrder).Select(pp => pp.PhotoUrl).FirstOrDefault());

        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.Name : null)
            .Map(dest => dest.Photos, src => src.ProductPhotos.OrderByDescending(ph => ph.IsMain).ThenBy(ph => ph.SortOrder))
            .Map(dest => dest.OptionWeights, src => src.OptionWeights);

        config.NewConfig<ProductSet, ProductSetDto>()
            .Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.Name : null)
            .Map(dest => dest.Photos, src => src.ProductSetPhotos.OrderByDescending(p => p.IsMain).ThenBy(p => p.SortOrder))
            .Map(dest => dest.Products, src => src.SetItems.Select(si => si.Product));

        config.NewConfig<Product, ProductListItemDto>();
    }
}
