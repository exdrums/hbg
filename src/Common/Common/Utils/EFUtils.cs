using Microsoft.EntityFrameworkCore;

namespace Common.Utils;

public static class EFUtils
{
    public static IQueryable<T> AsNoTrackingByCondition<T>(this IQueryable<T> query, bool cond) where T : class => cond ? query.AsNoTracking() : query;
}
