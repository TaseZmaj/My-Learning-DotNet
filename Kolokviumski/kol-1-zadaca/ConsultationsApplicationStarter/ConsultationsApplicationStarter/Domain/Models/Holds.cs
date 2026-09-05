using Domain.Common;

namespace Domain.Models;

public class Holds : BaseAuditableEntity<ConsultationsApplicationUser>
{
    public Guid ConsultationId { get; set; }
    public virtual Consultation Consultation { get; set; }
    
    public string UserId { get; set; }
    public virtual ConsultationsApplicationUser User { get; set; }
}