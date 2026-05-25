using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AguasNico_Api.Helpers.Interceptors;

public class DateTimeUtcKindInterceptor : SaveChangesInterceptor, IDbCommandInterceptor, IMaterializationInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        NormalizeTrackedDateTimes(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeTrackedDateTimes(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        NormalizeCommandParameters(command);
        return result;
    }

    public ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeCommandParameters(command);
        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    public InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        NormalizeCommandParameters(command);
        return result;
    }

    public ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeCommandParameters(command);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    public InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        NormalizeCommandParameters(command);
        return result;
    }

    public ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeCommandParameters(command);
        return new ValueTask<InterceptionResult<object>>(result);
    }

    public object InitializedInstance(
        MaterializationInterceptionData materializationData,
        object entity)
    {
        NormalizeEntityDateTimes(entity);
        return entity;
    }

    private static void NormalizeTrackedDateTimes(DbContext context)
    {
        if (context == null)
            return;

        var entries = context
            .ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dateTime)
                    property.CurrentValue = NormalizeDateTimeToUtc(dateTime);
            }
        }
    }

    private static void NormalizeCommandParameters(DbCommand command)
    {
        foreach (DbParameter parameter in command.Parameters)
        {
            parameter.Value = NormalizeParameterValue(parameter.Value, ParameterUsesTimestampWithoutTimeZone(parameter));
        }
    }

    private static object NormalizeParameterValue(object value, bool useUnspecifiedKind)
    {
        if (value == null || value == DBNull.Value)
            return value;

        return value switch
        {
            DateTime dateTime => NormalizeDateTimeForProvider(dateTime, useUnspecifiedKind),
            DateTime[] values => values.Select(x => NormalizeDateTimeForProvider(x, useUnspecifiedKind)).ToArray(),
            _ => value
        };
    }

    private static void NormalizeEntityDateTimes(object entity)
    {
        var properties = entity
            .GetType()
            .GetProperties()
            .Where(x => x.CanRead && x.CanWrite && IsDateTimeProperty(x.PropertyType));

        foreach (var property in properties)
        {
            if (property.GetValue(entity) is DateTime dateTime)
                property.SetValue(entity, NormalizeDateTimeToUtc(dateTime));
        }
    }

    private static bool IsDateTimeProperty(Type type)
    {
        return type == typeof(DateTime) || Nullable.GetUnderlyingType(type) == typeof(DateTime);
    }

    private static DateTime NormalizeDateTimeForProvider(DateTime value, bool useUnspecifiedKind)
    {
        var utc = NormalizeDateTimeToUtc(value);
        return useUnspecifiedKind ? DateTime.SpecifyKind(utc, DateTimeKind.Unspecified) : utc;
    }

    private static bool ParameterUsesTimestampWithoutTimeZone(DbParameter parameter)
    {
        var dataTypeName = parameter
            .GetType()
            .GetProperty("DataTypeName")
            ?.GetValue(parameter)
            ?.ToString();

        if (!string.IsNullOrWhiteSpace(dataTypeName))
        {
            return dataTypeName.Equals("timestamp without time zone", StringComparison.OrdinalIgnoreCase)
                || dataTypeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase);
        }

        var npgsqlDbType = parameter
            .GetType()
            .GetProperty("NpgsqlDbType")
            ?.GetValue(parameter)
            ?.ToString();

        return string.Equals(npgsqlDbType, "Timestamp", StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime NormalizeDateTimeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
