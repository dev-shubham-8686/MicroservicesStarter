using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class PaymentTransaction
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? OrderId { get; set; }
        public Guid? UserId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentProvider { get; set; }
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Metadata { get; set; }
    }
}
