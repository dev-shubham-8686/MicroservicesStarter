using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using Shared.Contracts.Events;
using MassTransit;
using System.Data;

namespace ProductService.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ProductDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            ProductDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<InventoryService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<ReservationResult> ReserveInventoryAsync(ReserveInventoryRequest request)
        {
            try
            {
                // Call stored procedure for inventory reservation
                var productIdParam = new SqlParameter("@ProductId", request.ProductId);
                var orderIdParam = new SqlParameter("@OrderId", request.OrderId);
                var quantityParam = new SqlParameter("@Quantity", request.Quantity);
                var expiryMinutesParam = new SqlParameter("@ReservationExpiryMinutes", request.ReservationExpiryMinutes);

                var reservationIdParam = new SqlParameter("@ReservationId", SqlDbType.UniqueIdentifier)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                var resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                var resultMessageParam = new SqlParameter("@ResultMessage", SqlDbType.NVarChar, 500)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_ReserveInventory] @ProductId, @OrderId, @Quantity, @ReservationExpiryMinutes, " +
                    "@ReservationId OUTPUT, @ResultCode OUTPUT, @ResultMessage OUTPUT",
                    productIdParam, orderIdParam, quantityParam, expiryMinutesParam,
                    reservationIdParam, resultCodeParam, resultMessageParam);

                var resultCode = (int?)resultCodeParam.Value ?? 99;
                var resultMessage = resultMessageParam.Value?.ToString() ?? "Unknown error";
                var reservationId = reservationIdParam.Value as Guid?;

                var success = resultCode == 0;

                if (success && reservationId.HasValue)
                {
                    // Get reservation details
                    var reservation = await _context.StockReservations
                        .FirstOrDefaultAsync(r => r.Id == reservationId.Value);

                    if (reservation != null)
                    {
                        // Publish stock reserved event
                        var reservedEvent = new StockReservedEvent
                        {
                            ReservationId = reservation.Id,
                            ProductId = reservation.ProductId ?? Guid.Empty,
                            OrderId = reservation.OrderId ?? Guid.Empty,
                            Quantity = reservation.Quantity ?? 0,
                            ReservedAt = reservation.ReservedAt ?? DateTime.UtcNow,
                            ExpiresAt = reservation.ExpiresAt
                        };

                        await _publishEndpoint.Publish(reservedEvent);
                        _logger.LogInformation("Stock reserved event published for ProductId: {ProductId}, OrderId: {OrderId}",
                            request.ProductId, request.OrderId);
                    }
                }

                return new ReservationResult
                {
                    Success = success,
                    ReservationId = reservationId,
                    Message = resultMessage,
                    ResultCode = resultCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving inventory for ProductId: {ProductId}", request.ProductId);
                return new ReservationResult
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    ResultCode = 99
                };
            }
        }

        public async Task<bool> CommitReservationAsync(Guid reservationId)
        {
            try
            {
                // Call stored procedure to commit reservation
                var reservationIdParam = new SqlParameter("@ReservationId", reservationId);

                var resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                var resultMessageParam = new SqlParameter("@ResultMessage", SqlDbType.NVarChar, 500)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_CommitInventoryReservation] @ReservationId, @ResultCode OUTPUT, @ResultMessage OUTPUT",
                    reservationIdParam, resultCodeParam, resultMessageParam);

                var resultCode = (int?)resultCodeParam.Value ?? 99;
                var success = resultCode == 0;

                if (success)
                {
                    // Get updated product and reservation info
                    var reservation = await _context.StockReservations
                        .FirstOrDefaultAsync(r => r.Id == reservationId);

                    if (reservation != null)
                    {
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.Id == reservation.ProductId);

                        if (product != null)
                        {
                            // Publish inventory updated event
                            var inventoryEvent = new InventoryUpdatedEvent
                            {
                                ProductId = product.Id,
                                MovementType = "OUT",
                                Quantity = reservation.Quantity ?? 0,
                                PreviousStock = (product.Stock) + (reservation.Quantity ?? 0),
                                NewStock = product.Stock,
                                ReferenceId = reservation.OrderId,
                                ReferenceType = "ORDER",
                                Reason = "Stock committed for order",
                                CreatedAt = DateTime.UtcNow
                            };

                            await _publishEndpoint.Publish(inventoryEvent);
                            _logger.LogInformation("Inventory updated event published for ProductId: {ProductId}", product.Id);
                        }
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing reservation: {ReservationId}", reservationId);
                return false;
            }
        }

        public async Task<bool> ReleaseReservationAsync(Guid reservationId)
        {
            try
            {
                var reservation = await _context.StockReservations
                    .FirstOrDefaultAsync(r => r.Id == reservationId);

                if (reservation == null || reservation.Status != "ACTIVE")
                {
                    return false;
                }

                reservation.Status = "RELEASED";
                reservation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Publish inventory updated event
                if (reservation.ProductId.HasValue)
                {
                    var inventoryEvent = new InventoryUpdatedEvent
                    {
                        ProductId = reservation.ProductId.Value,
                        MovementType = "RELEASED",
                        Quantity = reservation.Quantity ?? 0,
                        ReferenceId = reservation.OrderId,
                        ReferenceType = "ORDER",
                        Reason = "Stock reservation released",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _publishEndpoint.Publish(inventoryEvent);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing reservation: {ReservationId}", reservationId);
                return false;
            }
        }

        public async Task<List<InventoryMovement>> GetInventoryHistoryAsync(Guid productId)
        {
            return await _context.InventoryMovements
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StockReservation>> GetActiveReservationsAsync(Guid productId)
        {
            return await _context.StockReservations
                .Where(r => r.ProductId == productId
                    && r.Status == "ACTIVE"
                    && (r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow))
                .OrderBy(r => r.ExpiresAt)
                .ToListAsync();
        }

        public async Task ProcessExpiredReservationsAsync()
        {
            try
            {
                var expiredReservations = await _context.StockReservations
                    .Where(r => r.Status == "ACTIVE"
                        && r.ExpiresAt != null
                        && r.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var reservation in expiredReservations)
                {
                    reservation.Status = "EXPIRED";
                    reservation.UpdatedAt = DateTime.UtcNow;

                    // Publish expired reservation event
                    if (reservation.ProductId.HasValue && reservation.OrderId.HasValue)
                    {
                        var expiredEvent = new StockReservationExpiredEvent
                        {
                            ReservationId = reservation.Id,
                            ProductId = reservation.ProductId.Value,
                            OrderId = reservation.OrderId.Value,
                            Quantity = reservation.Quantity ?? 0,
                            ExpiredAt = DateTime.UtcNow
                        };

                        await _publishEndpoint.Publish(expiredEvent);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Processed {Count} expired reservations", expiredReservations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired reservations");
            }
        }
    }

}
