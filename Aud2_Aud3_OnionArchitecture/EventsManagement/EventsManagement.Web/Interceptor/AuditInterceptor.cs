using EventsManagement.Domain.Common;
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
        var entries = context.ChangeTracker
            .Entries<BaseAuditableEntity<string>>();

        foreach (var entry in entries)
        {
            var now  = DateTime.UtcNow;
            var user = _currentUser.GetUserId() ?? "system";

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedById      = user;
                entry.Entity.DateCreated      = now;
                entry.Entity.LastModifiedById  = user;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedById  = user;
                entry.Entity.DateLastModified  = now;
            }
        }

        return base.SavingChanges(eventData, result);
    }
}