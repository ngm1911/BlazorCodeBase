using BlazorCodeBase.Server.Database.Interceptor;
using BlazorCodeBase.Server.Database.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using System.Linq.Expressions;

namespace BlazorCodeBase.Server.Database.DbContext
{
    public class SQLiteContext(DbContextOptions<SQLiteContext> options,
                                    ModifieldInterceptor modifieldInterceptor) : IdentityDbContext<UserInfo>(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                          .LogTo(x => Log.Logger.Debug(x),
                                    events:
                                    [
                                        RelationalEventId.CommandExecuted,
                                    ])
                          .EnableDetailedErrors()
                          .EnableSensitiveDataLogging();

            optionsBuilder.AddInterceptors(modifieldInterceptor);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Global filter here
            //modelBuilder.Entity<Table>().HasQueryFilter(b => b.IsDeleted = "1");

            base.OnModelCreating(modelBuilder);
        }
    }

    public static class DbSetUtils
    {
        /// <summary>
        /// Only check right expression and only 1 condition
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<TSource> WhereIfNotNull<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            IQueryable<TSource> result = source;
            try
            {
                switch (predicate.Body)
                {
                    case BinaryExpression binary:
                        if (binary.Right is MemberExpression right)
                        {
                            var rightValue = Expression.Lambda(right).Compile().DynamicInvoke();
                            if (rightValue != null)
                            {
                                result = source.Where(predicate);
                            }
                        }
                        break;

                    case MethodCallExpression methodCall:
                        if (methodCall.Arguments.Count == 1)
                        {
                            var rightValue = Expression.Lambda(methodCall.Arguments[0]).Compile().DynamicInvoke();
                            if (rightValue != null)
                            {
                                result = source.Where(predicate);
                            }
                        }
                        break;

                    default:
                        result = source.Where(predicate);
                        break;
                }
            }
            catch
            {
                result = source.Where(predicate);
            }
            return result;
        }
    }
}
