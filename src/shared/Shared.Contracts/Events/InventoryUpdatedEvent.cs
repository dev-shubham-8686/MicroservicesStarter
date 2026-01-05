using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class InventoryUpdatedEvent
    {
        public Guid ProductId { get; set; }
        public string MovementType { get; set; } = string.Empty; // 'IN', 'OUT', 'RESERVED', 'RELEASED', 'ADJUSTMENT'
        public int Quantity { get; set; }
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public Guid? ReferenceId { get; set; } // OrderId, AdjustmentId, etc.
        public string? ReferenceType { get; set; } // 'ORDER', 'ADJUSTMENT', 'RETURN', etc.
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StockReservedEvent
    {
        public Guid ReservationId { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class StockReservationExpiredEvent
    {
        public Guid ReservationId { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
