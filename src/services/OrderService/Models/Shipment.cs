using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Shipment
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? OrderId { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public string? ShippingMethod { get; set; }
        public string? Status { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public List<ShipmentTrackingHistory>? TrackingHistory { get; set; }
    }

    public class ShipmentTrackingHistory
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? ShipmentId { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
