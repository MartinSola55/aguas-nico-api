using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = LocalClock.Now;
    public DateTime UpdatedAt { get; set; } = LocalClock.Now;
    public DateTime? DeletedAt { get; set; }
}

public static class LocalClock
{
    public static DateTime Now => DateTime.UtcNow;
    public static DateTime Today => Now.Date;
}

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var displayAttribute = value
            .GetType()
            .GetField(value.ToString())?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();

        return displayAttribute?.Name ?? value.ToString();
    }
}
