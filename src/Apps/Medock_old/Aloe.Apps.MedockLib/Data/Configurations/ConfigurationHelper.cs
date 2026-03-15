using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Entity設定の共通ヘルパー
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// 監査フィールドの設定を行う
    /// </summary>
    public static void ConfigureAuditableEntity<T>(EntityTypeBuilder<T> entity)
        where T : class, IAuditableEntity
    {
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");
        entity.Property(e => e.CreatedSessionId).HasColumnName("created_session_id");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");
        entity.Property(e => e.UpdatedSessionId).HasColumnName("updated_session_id");
    }
}
