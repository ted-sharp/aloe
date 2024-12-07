using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_daily_bookings")]
public class ReservationDailyBooking : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvDailyBkgId;

    [Key]
    [Column("resv_daily_bkg_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvDailyBkgId { get; set; }

    [Column("bkg_date")]
    public DateTime? BkgDate { get; set; }

    [Column("floor_id")]
    [Required]
    public int FloorId { get; set; } = 0;

    [Column("slot")]
    [Required]
    [MaxLength(Int32.MaxValue)]  // TEXT型に対応
    public string Slot { get; set; } = String.Empty;

    [Column("ampm_code")]
    [Required]
    public int AmPmCode { get; set; } = 0;

    [Column("sex_code")]
    [Required]
    public int SexCode { get; set; } = 0;

    [Column("bkg_user_id")]
    [Required]
    public int BkgUserId { get; set; } = 0;

    [Column("bkg_at")]
    [Required]
    public DateTime BkgAt { get; set; } = DateTime.Now;

    [Column("bkg_symbol_text")]
    [Required]
    [MaxLength(Int32.MaxValue)]  // TEXT型に対応
    public string BkgSymbolText { get; set; } = String.Empty;

    [Column("bkg_remark_text")]
    [Required]
    [MaxLength(Int32.MaxValue)]  // TEXT型に対応
    public string BkgRemarkText { get; set; } = String.Empty;

    [Column("is_held")]
    [Required]
    public bool IsHeld { get; set; } = false;

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; } = 0;

    [Column("resv_count")]
    [Required]
    public int ResvCount { get; set; } = 0;

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; } = 0;

    [Column("order_id")]
    [Required]
    public int OrderId { get; set; } = 0;

    [Column("sub_order_id")]
    [Required]
    public int SubOrderId { get; set; } = 0;

    [Column("is_cancelled")]
    [Required]
    public bool IsCancelled { get; set; } = false;

    [Column("noshow_count")]
    [Required]
    public int NoShowCount { get; set; } = 0;
}
