using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_equipment_bookings")]
public class ReservationEquipmentBooking : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvEquipBkgId;

    [Key]
    [Column("resv_equip_bkg_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvEquipBkgId { get; set; }

    [Column("equip_id")]
    [Required]
    public int EquipId { get; set; } = 0;

    [Column("bkg_date", TypeName = "Date")]
    public DateOnly? BkgDate { get; set; }

    [Column("slot")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Slot { get; set; } = String.Empty;

    [Column("bkg_user_id")]
    [Required]
    public int BkgUserId { get; set; } = 0;

    [Column("bkg_at")]
    [Required]
    public DateTime BkgAt { get; set; } = DateTime.Now;

    [Column("bkg_symbol_text")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string BkgSymbolText { get; set; } = String.Empty;

    [Column("bkg_remark_text")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string BkgRemarkText { get; set; } = String.Empty;

    [Column("is_tentative")]
    [Required]
    public bool IsTentative { get; set; } = false;

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; } = 0;

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; } = 0;

    [Column("rec_id")]
    [Required]
    public int RecId { get; set; } = 0;

    [Column("is_cancelled")]
    [Required]
    public bool IsCancelled { get; set; } = false;

    [Column("is_noshow")]
    [Required]
    public bool IsNoShow { get; set; } = false;


    public ReservationEquipmentBooking() { }

    public ReservationEquipmentBooking(int equipId, DateOnly bkgDate, string slot, string symbol, string remark, bool isTentative)
    {
        this.EquipId = equipId;
        this.BkgDate = bkgDate;
        this.Slot = slot;
        this.BkgSymbolText = symbol;
        this.BkgRemarkText = remark;
        this.IsTentative = isTentative;
    }
}
