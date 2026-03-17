// <copyright file="UserEntity.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Apps.Medock.MdLauncherLib.Data.Entities;

/// <summary>
/// ユーザーエンティティ（読取専用・最低限のカラム）。
/// </summary>
[Table("users")]
public class UserEntity
{
    /// <summary>ユーザー ID (PK)。</summary>
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>ユーザーコード。</summary>
    [Column("user_code")]
    [MaxLength(100)]
    public string UserCode { get; set; } = string.Empty;

    /// <summary>削除フラグ。</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
}
