using Mapster;
using GoldbApi.Models;
using GoldbApi.DTOs;
using System.Linq;
using System;

namespace GoldbApi.Mappings.Registers;

public class OrderMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {

        config.NewConfig<Order, OrderSearchResultDto>()
            .Map(dest => dest.UserName, src => src.User != null ? src.User.Name : "")
            .Map(dest => dest.CompanyName, src => src.User != null && src.User.UserCompanies.Any() ? src.User.UserCompanies.First().Company!.Name : "");

        config.NewConfig<CartItem, CartItemDto>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.ProductNo, src => src.Product != null ? src.Product.ProductNo : null)
            .Map(dest => dest.CompanyName, src => src.Product != null ? (src.Product.Company != null ? src.Product.Company.Name : null) : (src.ProductSet != null && src.ProductSet.Company != null ? src.ProductSet.Company.Name : null))
            .Map(dest => dest.PhotoUrl, src => src.Product != null && src.Product.ProductPhotos.Any() 
                           ? src.Product.ProductPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl 
                           : (src.ProductSet != null 
                               ? (src.ProductSet.ProductSetPhotos.Any() 
                                   ? src.ProductSet.ProductSetPhotos.OrderBy(p => p.SortOrder).First().PhotoUrl 
                                   : src.ProductSet.SetItems.SelectMany(si => si.Product!.ProductPhotos).OrderBy(p => p.SortOrder).Select(p => p.PhotoUrl).FirstOrDefault())
                               : null))
            .Map(dest => dest.ProductSetTitle, src => src.ProductSet != null ? src.ProductSet.Title : null)
            .Map(dest => dest.Price, src => (src.CustomFactoryPrice ?? (src.Product != null ? src.Product.FactoryPrice : (src.ProductSet != null ? src.ProductSet.FactoryPrice : 0))) +
                        (src.CustomLaborCost ?? (src.Product != null ? src.Product.LaborCost : (src.ProductSet != null ? src.ProductSet.LaborCost : 0))))
            .Map(dest => dest.FactoryPrice, src => src.Product != null ? src.Product.FactoryPrice : (src.ProductSet != null ? src.ProductSet.FactoryPrice : 0))
            .Map(dest => dest.LaborCost, src => src.Product != null ? src.Product.LaborCost : (src.ProductSet != null ? src.ProductSet.LaborCost : 0))
            .Map(dest => dest.Weight, src => src.Product != null 
                         ? (src.Product.OptionWeights.Any(ow => ow.Purity == src.Purity && ow.Color == src.Color) 
                             ? src.Product.OptionWeights.First(ow => ow.Purity == src.Purity && ow.Color == src.Color).Weight 
                             : src.Product.Weight) 
                         : 0)
            .Map(dest => dest.CategoryLarge, src => src.Product != null ? src.Product.CategoryLarge : (src.ProductSet != null ? src.ProductSet.CategoryLarge : null))
            .Map(dest => dest.CategoryMedium, src => src.Product != null ? src.Product.CategoryMedium : (src.ProductSet != null ? src.ProductSet.CategoryMedium : null))
            .Map(dest => dest.CategorySmall, src => src.Product != null ? src.Product.CategorySmall : (src.ProductSet != null ? src.ProductSet.CategorySmall : null));

        config.NewConfig<OrderItem, FactoryRecentItemDto>()
            .Map(dest => dest.OrderItemId, src => src.Id)
            .Map(dest => dest.OrderNo, src => src.Order != null ? src.Order.OrderNo : null)
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.Status, src => src.Order != null ? src.Order.Status : null)
            .Map(dest => dest.RequestedAt, src => src.CreatedAt);
    }
}
