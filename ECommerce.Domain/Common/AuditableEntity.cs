namespace ECommerce.Domain.Common;
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }
}

