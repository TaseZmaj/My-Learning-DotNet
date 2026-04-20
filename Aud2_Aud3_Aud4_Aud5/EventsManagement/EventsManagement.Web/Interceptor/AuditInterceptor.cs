using EventsManagement.Domain.Common;
using EventsManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Service.Interfaces;

namespace EventsManagement.Web.Interceptor;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    
    //BITNO: 
    //This interceptor runs RIGHT BEFORE
    // the SQL is executed and
    // automatically fills the audit fields - the ones defined in the Base Auditable Entity
    
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context!;
        // var entries = context.ChangeTracker
        //     .Entries<BaseAuditableEntity<string>>();
        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseAuditableEntity<EventsAppUser> && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseAuditableEntity<EventsAppUser>)entry.Entity;
            var now  = DateTime.UtcNow;
            var user = _currentUser.GetUserId() ?? "system";

            if (entry.State == EntityState.Added)
            {
                entity.CreatedById = user;
                entity.DateCreated = now;
                entity.LastModifiedById = user;
            }

            if (entry.State == EntityState.Modified)
            {
                entity.LastModifiedById  = user;
                entity.DateLastModified  = now;
            }
        }

        return base.SavingChanges(eventData, result);
    }
    
    
    //Ova dole e od GEMINI
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // Redirect the async call to your existing logic to avoid duplication
        return new ValueTask<InterceptionResult<int>>(SavingChanges(eventData, result));
    }
}