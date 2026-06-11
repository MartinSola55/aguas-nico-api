using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Helpers.Interceptors;

public class InterceptorsContainer
{
    public static void AddInterceptors(IServiceCollection services)
    {
        services.AddSingleton<DateTimeUtcKindInterceptor>();
    }

    public static void ConfigureInterceptors(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        options.AddInterceptors(
            serviceProvider.GetRequiredService<DateTimeUtcKindInterceptor>()
        );
    }
}
