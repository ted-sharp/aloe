using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("customer_files")]
public class CustomerFile : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.CustFileId;

    [Key]
    [Column("cust_file_id")]
    [Required]
    public int CustFileId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("file_path")]
    [MaxLength(Int32.MaxValue)]
    public string FilePath { get; set; } = String.Empty;

    [Column("file_type")]
    [MaxLength(Int32.MaxValue)]
    public string FileType { get; set; } = String.Empty;

    [Column("file_name")]
    [MaxLength(Int32.MaxValue)]
    public string FileName { get; set; } = String.Empty;

    [Column("file_desc")]
    [MaxLength(Int32.MaxValue)]
    public string FileDesc { get; set; } = String.Empty;

    [Column("updated_user_name")]
    [MaxLength(Int32.MaxValue)]
    public string UpdatedUserName { get; set; } = String.Empty;
}
