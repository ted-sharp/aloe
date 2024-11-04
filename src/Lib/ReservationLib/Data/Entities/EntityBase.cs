using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    int CreatedUserId { get; set; }

    Guid CreatedSessionId { get; set; }

    DateTime UpdatedAt { get; set; }

    int UpdatedUserId { get; set; }

    Guid UpdatedSessionId { get; set; }

    bool IsDeleted { get; set; }
}

public abstract class AuditableEntityBase<TKey> : IAuditableEntity
    where TKey : struct
{
    public abstract TKey Id { get; }

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("created_user_id")]
    [Required]
    public int CreatedUserId { get; set; } = 0;

    [Column("created_session_id")]
    [Required]
    public Guid CreatedSessionId { get; set; } = Guid.Empty;

    [Column("updated_at")]
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Column("updated_user_id")]
    [Required]
    public int UpdatedUserId { get; set; } = 0;

    [Column("updated_session_id")]
    [Required]
    public Guid UpdatedSessionId { get; set; } = Guid.Empty;

    [Column("is_deleted")]
    [Required]
    public bool IsDeleted { get; set; } = false;
}

public static class AuditableEntityExtensions
{
    public static IAuditableEntity SetCreatedSession(this IAuditableEntity auditableEntity, SessionDto session, DateTime now)
    {
        auditableEntity.CreatedAt = now;
        auditableEntity.CreatedSessionId = session.SessionId;
        auditableEntity.CreatedUserId = session.UserId;
        return auditableEntity;
    }

    public static IAuditableEntity SetUpdatedSession(this IAuditableEntity auditableEntity, SessionDto session, DateTime now)
    {
        auditableEntity.UpdatedAt = now;
        auditableEntity.UpdatedSessionId = session.SessionId;
        auditableEntity.UpdatedUserId = session.UserId;
        return auditableEntity;
    }
}
