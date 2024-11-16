using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Microsoft.EntityFrameworkCore;

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
{
    [NotMapped]
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

    public AuditableEntityBase<TKey> SetCreatedSession(SessionDto session, DateTime now)
    {
        this.CreatedAt = now;
        this.CreatedSessionId = session.SessionId;
        this.CreatedUserId = session.UserId;
        return this;
    }

    public AuditableEntityBase<TKey> SetUpdatedSession(SessionDto session, DateTime now)
    {
        this.UpdatedAt = now;
        this.UpdatedSessionId = session.SessionId;
        this.UpdatedUserId = session.UserId;
        return this;
    }
}
