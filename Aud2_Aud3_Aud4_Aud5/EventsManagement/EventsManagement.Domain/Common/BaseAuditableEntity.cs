namespace EventsManagement.Domain.Common;

public abstract class BaseAuditableEntity<TU> : BaseEntity
{
    // public TU CreatedBy { get; set; } = default!;
    public string CreatedById { get; set; }
    public DateTime DateCreated { get; set; }

    // public TU LastModifiedBy { get; set; } = default!;
    public string LastModifiedById { get; set; }
    public DateTime? DateLastModified { get; set; }
}