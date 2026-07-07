using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldbApi.Models;

[Description("미수금 내역")]
public class Receivable : BaseModel
{
    [Key]

    public int Id { get; set; }

    [Required]

    public int UserId { get; set; }

    public User? User { get; set; }

    public int? OrderId { get; set; }

    public Order? Order { get; set; }

    [Required]
    [MaxLength(20)]

    public string Type { get; set; } = "CHARGE"; 

    public decimal Amount { get; set; }

    public decimal RemainingAmount { get; set; } 

    [MaxLength(500)]

    public string? Memo { get; set; }

    [MaxLength(20)]

    public string? SettlementMethod { get; set; }
}
