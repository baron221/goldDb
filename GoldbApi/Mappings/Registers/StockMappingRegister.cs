using Mapster;
using GoldbApi.Models;
using GoldbApi.DTOs;
using System.Linq;
using System;

namespace GoldbApi.Mappings.Registers;

public class StockMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {

        config.NewConfig<Stock, StockSearchResultDto>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : "")
            .Map(dest => dest.ProductNo, src => src.Product != null ? src.Product.ProductNo : "")
            .Map(dest => dest.PhotoUrl, src => src.Product != null ? src.Product.ProductPhotos.OrderBy(pp => pp.SortOrder).Select(pp => pp.PhotoUrl).FirstOrDefault() : null);

        config.NewConfig<Stock, StockDto>()
            .Map(dest => dest.CompanyName, src => src.Product != null && src.Product.Company != null ? src.Product.Company.Name : 
                                                 (src.ProductSet != null && src.ProductSet.Company != null ? src.ProductSet.Company.Name : null))
            .Map(dest => dest.LogisticsCompanyName, src => src.SourceOrder != null && src.SourceOrder.LogisticsCompany != null ? src.SourceOrder.LogisticsCompany.Name : null)
            .Map(dest => dest.Attachments, src => src.Attachments)
            .Map(dest => dest.Children, src => src.Children)
            .Map(dest => dest.ProductPhotoUrl, src => src.Product != null && src.Product.ProductPhotos.Any() 
                                                      ? src.Product.ProductPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl 
                                                      : (src.ProductSet != null && src.ProductSet.ProductSetPhotos.Any() 
                                                          ? src.ProductSet.ProductSetPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl : null))
            .Map(dest => dest.CategoryLarge, src => src.Product != null ? src.Product.CategoryLarge : (src.ProductSet != null ? src.ProductSet.CategoryLarge : null))
            .Map(dest => dest.CategoryMedium, src => src.Product != null ? src.Product.CategoryMedium : (src.ProductSet != null ? src.ProductSet.CategoryMedium : null))
            .Map(dest => dest.CategorySmall, src => src.Product != null ? src.Product.CategorySmall : (src.ProductSet != null ? src.ProductSet.CategorySmall : null))
            .Map(dest => dest.SourceOrderDate, src => src.SourceOrder != null ? src.SourceOrder.CreatedAt : (DateTime?)null)
            .Map(dest => dest.ExhaustionOrderNo, src => src.ExhaustionOrder != null ? src.ExhaustionOrder.OrderNo : null);

        config.NewConfig<Stock, StockDetailDto>()
            .Inherits<Stock, StockDto>()
            .Map(dest => dest.Product, src => src.Product)
            .Map(dest => dest.ProductSet, src => src.ProductSet)
            .Map(dest => dest.Manufacturer, src => src.Product != null ? src.Product.Company : (src.ProductSet != null ? src.ProductSet.Company : null))
            .Map(dest => dest.LogisticsCompany, src => src.SourceOrder != null ? src.SourceOrder.LogisticsCompany : null);
    }
}
