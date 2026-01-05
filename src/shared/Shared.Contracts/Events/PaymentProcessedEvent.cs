using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class PaymentProcessedEvent
    {
        public Guid PaymentTransactionId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentProvider { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = string.Empty; // 'COMPLETED', 'FAILED', 'REFUNDED'
        public DateTime ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentFailedEvent
    {
        public Guid PaymentTransactionId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime FailedAt { get; set; }
    }

    public class RefundProcessedEvent
    {
        public Guid RefundId { get; set; }
        public Guid PaymentTransactionId { get; set; }
        public Guid OrderId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundType { get; set; } = string.Empty; // 'FULL', 'PARTIAL'
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
