using OrderService.Models;

namespace OrderService.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request);
        Task<RefundResult> ProcessRefundAsync(ProcessRefundRequest request);
        Task<PaymentTransaction?> GetPaymentTransactionAsync(Guid paymentTransactionId);
        Task<List<PaymentTransaction>> GetUserPaymentsAsync(Guid userId);
        Task<List<PaymentTransaction>> GetOrderPaymentsAsync(Guid orderId);
    }

    public class ProcessPaymentRequest
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentProvider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? PaymentIntentId { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public Guid? PaymentTransactionId { get; set; }
        public string? TransactionId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public int? ResultCode { get; set; }
    }

    public class ProcessRefundRequest
    {
        public Guid PaymentTransactionId { get; set; }
        public Guid OrderId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public string RefundType { get; set; } = "FULL"; // FULL or PARTIAL
        public Guid ProcessedBy { get; set; }
    }

    public class RefundResult
    {
        public bool Success { get; set; }
        public Guid? RefundId { get; set; }
        public string? RefundTransactionId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }
}
