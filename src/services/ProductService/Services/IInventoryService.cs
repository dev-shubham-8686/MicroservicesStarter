using ProductService.Models;

namespace ProductService.Services
{
    public interface IInventoryService
    {
        Task<ReservationResult> ReserveInventoryAsync(ReserveInventoryRequest request);
        Task<bool> CommitReservationAsync(Guid reservationId);
        Task<bool> ReleaseReservationAsync(Guid reservationId);
        Task<List<InventoryMovement>> GetInventoryHistoryAsync(Guid productId);
        Task<List<StockReservation>> GetActiveReservationsAsync(Guid productId);
        Task ProcessExpiredReservationsAsync();
    }

    public class ReserveInventoryRequest
    {
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public int ReservationExpiryMinutes { get; set; } = 30;
    }

    public class ReservationResult
    {
        public bool Success { get; set; }
        public Guid? ReservationId { get; set; }
        public string? Message { get; set; }
        public int? ResultCode { get; set; }
    }

}
