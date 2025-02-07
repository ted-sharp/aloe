using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("customer_marks")]
public class CustomerMark : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CustMarkId;

    [Key]
    [Column("cust_mark_id")]
    [Required]
    public int CustMarkId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("mark_symbol")]
    [MaxLength(Int32.MaxValue)]
    public string MarkSymbol { get; set; } = String.Empty;

    [Column("mark_color")]
    [Required]
    public int MarkColor { get; set; }

    [Column("mark_desc")]
    [MaxLength(Int32.MaxValue)]
    public string MarkDesc { get; set; } = String.Empty;

    [Column("updated_user_name")]
    [MaxLength(Int32.MaxValue)]
    public string UpdatedUserName { get; set; } = String.Empty;
}
