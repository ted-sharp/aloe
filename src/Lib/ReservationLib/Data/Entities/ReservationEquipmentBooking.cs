using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("reservation_equipment_bookings")]
public class ReservationEquipmentBooking : AuditableEntityBase<int>
{
    public override int Id => this.ResvEquipBkgId;

    [Key]
    [Column("resv_equip_bkg_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvEquipBkgId { get; set; }

    [Column("bkg_date")]
    public DateTime? BkgDate { get; set; }

    [Column("equip_id")]
    [Required]
    public int EquipId { get; set; } = 0;

    [Column("slot")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Slot { get; set; } = String.Empty;

    [Column("bkg_user_id")]
    [Required]
    public int BkgUserId { get; set; } = 0;

    [Column("bkg_at")]
    [Required]
    public DateTime BkgAt { get; set; } = DateTime.UtcNow;

    [Column("bkg_symbol_text")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string BkgSymbolText { get; set; } = String.Empty;

    [Column("bkg_remark_text")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string BkgRemarkText { get; set; } = String.Empty;

    [Column("is_held")]
    [Required]
    public bool IsHeld { get; set; } = false;

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; } = 0;

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; } = 0;

    [Column("order_id")]
    [Required]
    public int OrderId { get; set; } = 0;

    [Column("sub_order_id")]
    [Required]
    public int SubOrderId { get; set; } = 0;
}
