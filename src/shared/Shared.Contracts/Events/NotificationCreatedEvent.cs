using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class NotificationCreatedEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty; // 'ORDER_CONFIRMED', 'SHIPMENT_SENT', 'PAYMENT_RECEIVED', etc.
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public string Priority { get; set; } = "MEDIUM";
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewRequestEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public List<OrderItemForReview> Items { get; set; } = new();
        public DateTime OrderDate { get; set; }
    }

    public class OrderItemForReview
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
