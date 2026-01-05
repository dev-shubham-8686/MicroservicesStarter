using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class ShipmentCreatedEvent
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ShipmentStatusUpdatedEvent
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ShipmentDeliveredEvent
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime DeliveredAt { get; set; }
    }
}
