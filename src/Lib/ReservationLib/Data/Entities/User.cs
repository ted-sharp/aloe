using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.CoreLib.Security;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("users")]
public class User : AuditableEntityBase<int>
{
    public override int Id => this.UserId;

    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    public string DisplayName { get; set; } = String.Empty;

    [Column("login_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string LoginName { get; set; } = String.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = String.Empty;

    [Column("password_hash")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PasswordHash { get; set; } = String.Empty;

    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PasswordSalt { get; set; } = String.Empty;

    public DateTime ExpireDate { get; set; } = DateTime.UtcNow;

    public int FailedAttemptCount { get; set; } = 0;

    public DateTime LockedUntilAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLogoutAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserInfo { get; set; } = String.Empty;

    #region Method

    /// <summary>
    /// パスワードが正しいかどうか検証します。
    /// </summary>
    public bool VerifyPassword(string password)
    {
        return PasswordHasher.Default.VerifyPassword(password, this.PasswordHash, this.PasswordSalt);
    }

    /// <summary>
    /// ログイン失敗時の処理を行います。
    /// 失敗回数をインクリメントし、試行回数が超えていたら指定秒数間ログインできないようにロックします。
    /// </summary>
    public void FailLogin(int maxFailedAttempts, int lockingSeconds, DateTime now)
    {
        this.FailedAttemptCount++;

        if (this.FailedAttemptCount >= maxFailedAttempts)
        {
            this.FailedAttemptCount = 0;
            this.LockedUntilAt = now.AddSeconds(lockingSeconds);
        }
    }

    public bool IsLocked(DateTime currentTime)
    {
        return this.LockedUntilAt > currentTime;
    }

    public bool IsExpired(DateTime currentDate)
    {
        return this.ExpireDate < currentDate;
    }

    #endregion Method
}





