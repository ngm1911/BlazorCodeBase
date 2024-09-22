using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using BlazorCodeBase.Server.Database.Interface;

namespace BlazorCodeBase.Server.Database.Interceptor
{
    public class ModifieldInterceptor(IHttpContextAccessor _httpContextAccessor) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var user = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(user) == false)
            {
                foreach (var entry in eventData.Context.ChangeTracker.Entries<IModified>())
                {
                    entry.Entity.UserModified = user;
                    entry.Entity.DateModified = DateTime.Now;
                }
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
