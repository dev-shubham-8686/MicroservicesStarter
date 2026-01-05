using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Process payment for an order
    /// Complex business logic: Validates order, processes payment via stored procedure, publishes events
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

            // Verify user owns the order
            if (request.UserId != userId)
            {
                return Forbid("You can only process payments for your own orders");
            }

            var result = await _paymentService.ProcessPaymentAsync(request);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    paymentTransactionId = result.PaymentTransactionId,
                    transactionId = result.TransactionId,
                    status = result.Status,
                    message = result.Message
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.Message,
                resultCode = result.ResultCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(500, new { success = false, message = "An error occurred processing payment" });
        }
    }

    /// <summary>
    /// Process refund for a payment
    /// Complex business logic: Validates payment, processes refund, updates payment status, publishes events
    /// </summary>
    [HttpPost("refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessRefund([FromBody] ProcessRefundRequest request)
    {
        try
        {
            var processedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            request.ProcessedBy = processedBy;

            var result = await _paymentService.ProcessRefundAsync(request);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    refundId = result.RefundId,
                    refundTransactionId = result.RefundTransactionId,
                    status = result.Status,
                    message = result.Message
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund");
            return StatusCode(500, new { success = false, message = "An error occurred processing refund" });
        }
    }

    /// <summary>
    /// Get payment transaction details
    /// </summary>
    [HttpGet("{paymentTransactionId}")]
    public async Task<IActionResult> GetPaymentTransaction(Guid paymentTransactionId)
    {
        try
        {
            var payment = await _paymentService.GetPaymentTransactionAsync(paymentTransactionId);

            if (payment == null)
            {
                return NotFound(new { message = "Payment transaction not found" });
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

            // Verify user owns the payment or is admin
            if (payment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only view your own payments");
            }

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment transaction");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get user's payment history
    /// </summary>
    [HttpGet("my-payments")]
    public async Task<IActionResult> GetMyPayments()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user payments");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get payments for an order
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderPayments(Guid orderId)
    {
        try
        {
            var payments = await _paymentService.GetOrderPaymentsAsync(orderId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order payments");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
