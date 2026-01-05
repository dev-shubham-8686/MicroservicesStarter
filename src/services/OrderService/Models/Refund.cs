using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Refund
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? PaymentTransactionId { get; set; }
        public Guid? OrderId { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundType { get; set; }
        public string? Status { get; set; }
        public string? RefundTransactionId { get; set; }
        public Guid? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
    }
}
