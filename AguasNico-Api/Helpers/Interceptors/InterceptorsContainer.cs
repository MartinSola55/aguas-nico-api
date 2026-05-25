using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.Helpers.Interceptors;

public class InterceptorsContainer
{
    public static void AddInterceptors(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(
            new DateTimeUtcKindInterceptor()
        );
    }
}
