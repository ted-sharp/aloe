// <copyright file="AppEntity.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Apps.Medock.MdLauncherLib.Data.Entities;

/// <summary>
/// アプリ登録エンティティ。
/// </summary>
[Table("apps")]
public class AppEntity
{
    /// <summary>アプリコード (PK)。</summary>
    [Key]
    [Column("app_code")]
    [MaxLength(10)]
    public string AppCode { get; set; } = string.Empty;

    /// <summary>アプリ名。</summary>
    [Column("app_name")]
    [MaxLength(100)]
    public string AppName { get; set; } = string.Empty;

    /// <summary>アプリバージョン。</summary>
    [Column("app_ver")]
    [MaxLength(10)]
    public string AppVer { get; set; } = string.Empty;

    /// <summary>アプリ説明。</summary>
    [Column("app_desc")]
    [MaxLength(1000)]
    public string AppDesc { get; set; } = string.Empty;

    /// <summary>表示順。</summary>
    [Column("app_seq")]
    public int AppSeq { get; set; }

    /// <summary>実行ファイルパス。</summary>
    [Column("app_path")]
    [MaxLength(1000)]
    public string AppPath { get; set; } = string.Empty;

    /// <summary>起動引数。</summary>
    [Column("app_args")]
    [MaxLength(1000)]
    public string AppArgs { get; set; } = string.Empty;

    /// <summary>作業ディレクトリ。</summary>
    [Column("app_dir")]
    [MaxLength(1000)]
    public string AppDir { get; set; } = string.Empty;

    /// <summary>アイコンパス。</summary>
    [Column("app_icon")]
    [MaxLength(1000)]
    public string AppIcon { get; set; } = string.Empty;

    /// <summary>NamedPipe 名。</summary>
    [Column("pipe_name")]
    [MaxLength(1000)]
    public string PipeName { get; set; } = string.Empty;

    /// <summary>有効フラグ。</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>削除フラグ。</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    /// <summary>作成日時。</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時。</summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
