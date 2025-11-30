using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 監視用に必要な項目を定義したもの。
/// データベースのトリガーで記録するのに必要。
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// 削除
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者ID
    /// </summary>
    int CreatedUserId { get; set; }

    /// <summary>
    /// 作成セッションID
    /// </summary>
    Guid CreatedSessionId { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    int UpdatedUserId { get; set; }

    /// <summary>
    /// 更新セッションID
    /// </summary>
    Guid UpdatedSessionId { get; set; }
}

public abstract class AuditableEntityBase<TKey> : IAuditableEntity
{
    [NotMapped]
    public abstract TKey Id { get; }

    [Column("is_deleted")]
    [Required]
    public bool IsDeleted { get; set; }

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("created_user_id")]
    [Required]
    public int CreatedUserId { get; set; }

    [Column("created_session_id")]
    [Required]
    public Guid CreatedSessionId { get; set; }

    [Column("updated_at")]
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Column("updated_user_id")]
    [Required]
    public int UpdatedUserId { get; set; }

    [Column("updated_session_id")]
    [Required]
    public Guid UpdatedSessionId { get; set; }

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
