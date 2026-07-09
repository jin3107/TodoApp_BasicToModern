using System.Linq.Expressions;
using MayNghien.Infrastructures.Models.Requests;

namespace Todo.Infrastructure.Persistence.Repositories
{
    internal static class QuerySortExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> input, SortByInfo sortByInfo)
        {
            var fieldName = char.ToUpper(sortByInfo.FieldName[0]) + sortByInfo.FieldName.Substring(1);
            var param = Expression.Parameter(typeof(T), "m");
            var property = Expression.Property(param, fieldName);
            var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(property, typeof(object)), param);

            return sortByInfo.Ascending == true
                ? input.OrderBy(lambda)
                : input.OrderByDescending(lambda);
        }
    }
}
