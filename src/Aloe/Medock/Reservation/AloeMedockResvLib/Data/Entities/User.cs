using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Security;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("users")]
public class User : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.UserId;

    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Column("display_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string DisplayName { get; set; } = String.Empty;

    [Column("login_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string LoginName { get; set; } = String.Empty;

    [Column("email")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = String.Empty;

    [Column("password_hash")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PasswordHash { get; set; } = String.Empty;

    [Column("password_salt")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PasswordSalt { get; set; } = String.Empty;

    [Column("expire_date")]
    [Required]
    public DateTime ExpireDate { get; set; } = DateTime.MaxValue.Date;

    [Column("failed_attempt_count")]
    [Required]
    public int FailedAttemptCount { get; set; }

    [Column("locked_until_at")]
    [Required]
    public DateTime LockedUntilAt { get; set; } = DateTime.Now;

    [Column("last_login_at")]
    [Required]
    public DateTime LastLoginAt { get; set; } = DateTime.Now;

    [Column("last_logout_at")]
    [Required]
    public DateTime LastLogoutAt { get; set; } = DateTime.Now;

    //[Column("user_info")]
    //[Required]
    //public string UserInfo { get; set; } = String.Empty;

    public User() { }

    public User(string displayName, string loginName, string email, string password)
    {
        this.DisplayName = displayName;
        this.LoginName = loginName;
        this.Email = email;
        var (hash, salt) = PasswordHasher.Default.HashPassword(password);
        this.PasswordHash = hash;
        this.PasswordSalt = salt;
    }

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





