using Mapster;
using GoldbApi.Models;
using GoldbApi.DTOs;

namespace GoldbApi.Mappings.Registers;

public class UserMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {

        config.NewConfig<UserMenuSetting, UserMenuSettingDto>()
            .Map(dest => dest.MenuId, src => src.MenuId)
            .Map(dest => dest.Affix, src => src.Affix)
            .Map(dest => dest.SortOrder, src => src.SortOrder)
            .Map(dest => dest.Title, src => src.Menu != null ? src.Menu.Title : null);

        config.NewConfig<User, UserListItemResponse>()
            .Map(dest => dest.CompanyName, src => src.UserCompanies.FirstOrDefault() != null ? src.UserCompanies.FirstOrDefault()!.Company!.Name : null)
            .Map(dest => dest.CompanyCategory, src => src.UserCompanies.FirstOrDefault() != null ? src.UserCompanies.FirstOrDefault()!.Company!.Category : null)
            .Map(dest => dest.LogisticsCompanyName, src => src.UserCompanies.FirstOrDefault() != null && src.UserCompanies.FirstOrDefault()!.Company!.LogisticsCompany != null ? src.UserCompanies.FirstOrDefault()!.Company!.LogisticsCompany!.Name : null)
            .Map(dest => dest.IsDirectManagement, src => src.UserCompanies.FirstOrDefault() != null ? src.UserCompanies.FirstOrDefault()!.Company!.IsDirectManagement : false);

        config.NewConfig<User, UserDetailResponse>()
            .Map(dest => dest.Ssn, src => string.IsNullOrEmpty(src.Ssn) ? src.Ssn : GoldbApi.Utils.EncryptionUtils.Decrypt(src.Ssn))
            .Map(dest => dest.CompanyId, src => src.UserCompanies.FirstOrDefault() != null ? (int?)src.UserCompanies.FirstOrDefault()!.CompanyId : null)
            .Map(dest => dest.CompanyName, src => src.UserCompanies.FirstOrDefault() != null ? src.UserCompanies.FirstOrDefault()!.Company!.Name : null)
            .Map(dest => dest.CompanyCategory, src => src.UserCompanies.FirstOrDefault() != null ? src.UserCompanies.FirstOrDefault()!.Company!.Category : null)
            .Map(dest => dest.LogisticsCompanyName, src => src.UserCompanies.FirstOrDefault() != null && src.UserCompanies.FirstOrDefault()!.Company!.LogisticsCompany != null ? src.UserCompanies.FirstOrDefault()!.Company!.LogisticsCompany!.Name : null)
            .Map(dest => dest.Emails, src => src.UserEmails)
            .Map(dest => dest.Phones, src => src.UserPhones)
            .Map(dest => dest.Photos, src => src.UserPhotos.OrderBy(p => p.SortOrder))
            .Map(dest => dest.Roles, src => src.UserRoles.Select(ur => ur.Role != null ? ur.Role.Key : "").Where(k => !string.IsNullOrEmpty(k)));
    }
}
