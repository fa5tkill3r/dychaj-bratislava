using BP.Data.DbModels;
using BP.Data.Dto;

namespace BP.API.Utility;

public static class Extensions
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        => self.Select((item, index) => (item, index));
    
    public static IEnumerable<(T item, int index)> WithIndex<T>(this T[] self)
        => self.Select((item, index) => (item, index));
    
    public static TimeSpan ToDateTime(this Interval interval)
    {
        return interval switch
        {
            Interval.TenMinutes => TimeSpan.FromMinutes(10),
            Interval.Hourly => TimeSpan.FromHours(1),
            Interval.Daily => TimeSpan.FromDays(1),
            Interval.Weekly => TimeSpan.FromDays(7),
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
        };
    }
}