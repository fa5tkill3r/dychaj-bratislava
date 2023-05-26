namespace BP.API.Utility;

public static class Extensions
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        => self.Select((item, index) => (item, index));
    
    public static IEnumerable<(T item, int index)> WithIndex<T>(this T[] self)
        => self.Select((item, index) => (item, index));
}