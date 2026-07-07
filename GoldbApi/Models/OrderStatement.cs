using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldbApi.Models;

public class OrderStatement : BaseModel
{

    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    public string SnapshotData { get; set; } = string.Empty;

    public byte[]? PdfBinary { get; set; }
}
