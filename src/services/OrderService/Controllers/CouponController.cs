using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;
    private readonly ILogger<CouponController> _logger;

    public CouponController(ICouponService couponService, ILogger<CouponController> logger)
    {
        _couponService = couponService;
        _logger = logger;
    }

    /// <summary>
    /// Apply coupon to order
    /// Complex business logic: Validates coupon via stored procedure, calculates discount, publishes events
    /// </summary>
    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            request.UserId = userId;

            var result = await _couponService.ApplyCouponAsync(request);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    discountAmount = result.DiscountAmount,
                    finalAmount = result.FinalAmount,
                    couponId = result.CouponId,
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
            _logger.LogError(ex, "Error applying coupon");
            return StatusCode(500, new { success = false, message = "An error occurred applying coupon" });
        }
    }

    /// <summary>
    /// Get coupon by code
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetCouponByCode(string code)
    {
        try
        {
            var coupon = await _couponService.GetCouponByCodeAsync(code);

            if (coupon == null)
            {
                return NotFound(new { message = "Coupon not found" });
            }

            return Ok(coupon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coupon");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create new coupon (Admin only)
    /// Complex business logic: Validates coupon rules, creates coupon with business validations
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        try
        {
            // Business validations
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { message = "Coupon code is required" });
            }

            if (request.DiscountType != "PERCENTAGE" && request.DiscountType != "FIXED_AMOUNT")
            {
                return BadRequest(new { message = "Discount type must be PERCENTAGE or FIXED_AMOUNT" });
            }

            if (request.DiscountValue <= 0)
            {
                return BadRequest(new { message = "Discount value must be greater than 0" });
            }

            if (request.DiscountType == "PERCENTAGE" && request.DiscountValue > 100)
            {
                return BadRequest(new { message = "Percentage discount cannot exceed 100%" });
            }

            // Check if coupon code already exists
            var existingCoupon = await _couponService.GetCouponByCodeAsync(request.Code);
            if (existingCoupon != null)
            {
                return Conflict(new { message = "Coupon code already exists" });
            }

            var createdBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            request.CreatedBy = createdBy;

            var coupon = await _couponService.CreateCouponAsync(request);

            if (coupon == null)
            {
                return StatusCode(500, new { message = "Failed to create coupon" });
            }

            return CreatedAtAction(nameof(GetCouponByCode), new { code = coupon.Code }, coupon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coupon");
            return StatusCode(500, new { message = "An error occurred creating coupon" });
        }
    }

    /// <summary>
    /// Get active coupons
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCoupons()
    {
        try
        {
            var coupons = await _couponService.GetActiveCouponsAsync();
            return Ok(coupons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active coupons");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get coupon usage history (Admin only)
    /// </summary>
    [HttpGet("{couponId}/usage")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCouponUsageHistory(Guid couponId)
    {
        try
        {
            var usageHistory = await _couponService.GetCouponUsageHistoryAsync(couponId);
            return Ok(usageHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coupon usage history");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
