using MassTransit;
using MassTransit.Transports;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging;
using System.Data;

namespace OrderService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly OrderDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            OrderDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            try
            {
                // Call stored procedure for complex payment processing
                var orderIdParam = new SqlParameter("@OrderId", request.OrderId);
                var userIdParam = new SqlParameter("@UserId", request.UserId);
                var paymentMethodParam = new SqlParameter("@PaymentMethod", request.PaymentMethod);
                var paymentProviderParam = new SqlParameter("@PaymentProvider", request.PaymentProvider);
                var amountParam = new SqlParameter("@Amount", request.Amount);
                var currencyParam = new SqlParameter("@Currency", request.Currency ?? "USD");
                var paymentIntentIdParam = new SqlParameter("@PaymentIntentId", (object?)request.PaymentIntentId ?? DBNull.Value);

                var transactionIdParam = new SqlParameter("@TransactionId", SqlDbType.NVarChar, 200)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                var paymentTransactionIdParam = new SqlParameter("@PaymentTransactionId", SqlDbType.UniqueIdentifier)
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
                    "EXEC [dbo].[sp_ProcessOrderPayment] @OrderId, @UserId, @PaymentMethod, @PaymentProvider, " +
                    "@Amount, @Currency, @PaymentIntentId, @TransactionId OUTPUT, @PaymentTransactionId OUTPUT, " +
                    "@ResultCode OUTPUT, @ResultMessage OUTPUT",
                    orderIdParam, userIdParam, paymentMethodParam, paymentProviderParam,
                    amountParam, currencyParam, paymentIntentIdParam, transactionIdParam,
                    paymentTransactionIdParam, resultCodeParam, resultMessageParam);

                var resultCode = (int?)resultCodeParam.Value ?? 99;
                var resultMessage = resultMessageParam.Value?.ToString() ?? "Unknown error";
                var transactionId = transactionIdParam.Value?.ToString();
                var paymentTransactionId = paymentTransactionIdParam.Value as Guid?;

                var success = resultCode == 0;

                if (success && paymentTransactionId.HasValue)
                {
                    // Get the payment transaction to publish event
                    var payment = await _context.PaymentTransactions
                        .FirstOrDefaultAsync(p => p.Id == paymentTransactionId.Value);

                    if (payment != null)
                    {
                        // Publish payment processed event via RabbitMQ
                        var paymentEvent = new PaymentProcessedEvent
                        {
                            PaymentTransactionId = payment.Id,
                            OrderId = payment.OrderId ?? Guid.Empty,
                            UserId = payment.UserId ?? Guid.Empty,
                            PaymentMethod = payment.PaymentMethod ?? string.Empty,
                            PaymentProvider = payment.PaymentProvider ?? string.Empty,
                            TransactionId = payment.TransactionId ?? string.Empty,
                            Amount = payment.Amount ?? 0,
                            Currency = payment.Currency ?? "USD",
                            Status = payment.Status ?? "PENDING",
                            ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow,
                            CreatedAt = payment.CreatedAt ?? DateTime.UtcNow
                        };

                        await _publishEndpoint.Publish(paymentEvent);
                        _logger.LogInformation("Payment processed event published for OrderId: {OrderId}", request.OrderId);
                    }
                }
                else
                {
                    // Publish payment failed event
                    if (paymentTransactionId.HasValue)
                    {
                        var failedEvent = new PaymentFailedEvent
                        {
                            PaymentTransactionId = paymentTransactionId.Value,
                            OrderId = request.OrderId,
                            UserId = request.UserId,
                            FailureReason = resultMessage,
                            Amount = request.Amount,
                            FailedAt = DateTime.UtcNow
                        };

                        await _publishEndpoint.Publish(failedEvent);
                    }
                }

                return new PaymentResult
                {
                    Success = success,
                    PaymentTransactionId = paymentTransactionId,
                    TransactionId = transactionId,
                    Status = success ? "COMPLETED" : "FAILED",
                    Message = resultMessage,
                    ResultCode = resultCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for OrderId: {OrderId}", request.OrderId);
                return new PaymentResult
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    ResultCode = 99
                };
            }
        }

        public async Task<RefundResult> ProcessRefundAsync(ProcessRefundRequest request)
        {
            try
            {
                var payment = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(p => p.Id == request.PaymentTransactionId);

                if (payment == null)
                {
                    return new RefundResult
                    {
                        Success = false,
                        Message = "Payment transaction not found"
                    };
                }

                // Create refund record
                var refund = new Refund
                {
                    Id = Guid.NewGuid(),
                    PaymentTransactionId = request.PaymentTransactionId,
                    OrderId = request.OrderId,
                    RefundAmount = request.RefundAmount,
                    RefundReason = request.RefundReason,
                    RefundType = request.RefundType,
                    Status = "PROCESSING",
                    ProcessedBy = request.ProcessedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Refunds.Add(refund);
                await _context.SaveChangesAsync();

                // Simulate refund processing (in real scenario, call payment gateway)
                refund.Status = "COMPLETED";
                refund.ProcessedAt = DateTime.UtcNow;
                refund.RefundTransactionId = "REF-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                // Update payment status
                if (request.RefundType == "FULL")
                {
                    payment.Status = "REFUNDED";
                }
                else
                {
                    payment.Status = "PARTIALLY_REFUNDED";
                }
                payment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Publish refund processed event
                var refundEvent = new RefundProcessedEvent
                {
                    RefundId = refund.Id,
                    PaymentTransactionId = request.PaymentTransactionId,
                    OrderId = request.OrderId,
                    RefundAmount = request.RefundAmount,
                    RefundType = request.RefundType,
                    Status = refund.Status,
                    ProcessedAt = refund.ProcessedAt ?? DateTime.UtcNow
                };

                await _publishEndpoint.Publish(refundEvent);
                _logger.LogInformation("Refund processed event published for PaymentTransactionId: {PaymentTransactionId}",
                    request.PaymentTransactionId);

                return new RefundResult
                {
                    Success = true,
                    RefundId = refund.Id,
                    RefundTransactionId = refund.RefundTransactionId,
                    Status = refund.Status,
                    Message = "Refund processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for PaymentTransactionId: {PaymentTransactionId}",
                    request.PaymentTransactionId);
                return new RefundResult
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<PaymentTransaction?> GetPaymentTransactionAsync(Guid paymentTransactionId)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(p => p.Id == paymentTransactionId);
        }

        public async Task<List<PaymentTransaction>> GetUserPaymentsAsync(Guid userId)
        {
            return await _context.PaymentTransactions
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PaymentTransaction>> GetOrderPaymentsAsync(Guid orderId)
        {
            return await _context.PaymentTransactions
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
