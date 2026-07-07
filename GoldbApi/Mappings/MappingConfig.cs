using Mapster;
using System.Reflection;

namespace GoldbApi.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
}
