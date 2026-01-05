using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class InventoryMovement
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? ProductId { get; set; }
        public string? MovementType { get; set; }
        public int? Quantity { get; set; }
        public int? PreviousStock { get; set; }
        public int? NewStock { get; set; }
        public string? Reason { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class StockReservation
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? ProductId { get; set; }
        public Guid? OrderId { get; set; }
        public int? Quantity { get; set; }
        public DateTime? ReservedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
