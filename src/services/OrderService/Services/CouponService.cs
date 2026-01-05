using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging;
using System.Data;
using System.Text.Json;

namespace OrderService.Services
{
    public class CouponService : ICouponService
    {
        private readonly OrderDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CouponService> _logger;

        public CouponService(
            OrderDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<CouponService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<CouponApplicationResult> ApplyCouponAsync(ApplyCouponRequest request)
        {
            try
            {
                // Prepare product IDs as JSON string for stored procedure
                string? productIdsJson = null;
                if (request.ProductIds != null && request.ProductIds.Any())
                {
                    productIdsJson = JsonSerializer.Serialize(request.ProductIds);
                }

                // Call stored procedure to apply coupon
                var couponCodeParam = new SqlParameter("@CouponCode", request.CouponCode);
                var orderIdParam = new SqlParameter("@OrderId", request.OrderId);
                var userIdParam = new SqlParameter("@UserId", request.UserId);
                var orderTotalParam = new SqlParameter("@OrderTotal", request.OrderTotal);
                var productIdsParam = new SqlParameter("@ProductIds", (object?)productIdsJson ?? DBNull.Value);

                var discountAmountParam = new SqlParameter("@DiscountAmount", SqlDbType.Decimal)
                {
                    Precision = 18,
                    Scale = 2,
                    Direction = System.Data.ParameterDirection.Output
                };
                var finalAmountParam = new SqlParameter("@FinalAmount", SqlDbType.Decimal)
                {
                    Precision = 18,
                    Scale = 2,
                    Direction = System.Data.ParameterDirection.Output
                };
                var couponIdParam = new SqlParameter("@CouponId", SqlDbType.UniqueIdentifier)
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
                    "EXEC [dbo].[sp_ApplyCoupon] @CouponCode, @OrderId, @UserId, @OrderTotal, @ProductIds, " +
                    "@DiscountAmount OUTPUT, @FinalAmount OUTPUT, @CouponId OUTPUT, @ResultCode OUTPUT, @ResultMessage OUTPUT",
                    couponCodeParam, orderIdParam, userIdParam, orderTotalParam, productIdsParam,
                    discountAmountParam, finalAmountParam, couponIdParam, resultCodeParam, resultMessageParam);

                var resultCode = (int?)resultCodeParam.Value ?? 99;
                var resultMessage = resultMessageParam.Value?.ToString() ?? "Unknown error";
                var discountAmount = discountAmountParam.Value as decimal?;
                var finalAmount = finalAmountParam.Value as decimal?;
                var couponId = couponIdParam.Value as Guid?;

                var success = resultCode == 0;

                if (success && couponId.HasValue && discountAmount.HasValue)
                {
                    // Get coupon details
                    var coupon = await _context.Coupons
                        .FirstOrDefaultAsync(c => c.Id == couponId.Value);

                    if (coupon != null)
                    {
                        // Get coupon usage record
                        var couponUsage = await _context.CouponUsages
                            .Where(cu => cu.CouponId == couponId.Value && cu.OrderId == request.OrderId)
                            .OrderByDescending(cu => cu.CreatedAt)
                            .FirstOrDefaultAsync();

                        if (couponUsage != null)
                        {
                            // Publish coupon applied event
                            var couponEvent = new CouponAppliedEvent
                            {
                                CouponUsageId = couponUsage.Id,
                                CouponId = couponId.Value,
                                CouponCode = coupon.Code ?? string.Empty,
                                OrderId = request.OrderId,
                                UserId = request.UserId,
                                DiscountAmount = discountAmount.Value,
                                OrderTotalBeforeDiscount = request.OrderTotal,
                                OrderTotalAfterDiscount = finalAmount ?? request.OrderTotal,
                                AppliedAt = couponUsage.UsedAt ?? DateTime.UtcNow
                            };

                            await _publishEndpoint.Publish(couponEvent);
                            _logger.LogInformation("Coupon applied event published for CouponCode: {CouponCode}, OrderId: {OrderId}",
                                request.CouponCode, request.OrderId);
                        }
                    }
                }

                return new CouponApplicationResult
                {
                    Success = success,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    CouponId = couponId,
                    Message = resultMessage,
                    ResultCode = resultCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon: {CouponCode}", request.CouponCode);
                return new CouponApplicationResult
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    ResultCode = 99
                };
            }
        }

        public async Task<Coupon?> GetCouponByCodeAsync(string code)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<Coupon?> CreateCouponAsync(CreateCouponRequest request)
        {
            try
            {
                var coupon = new Coupon
                {
                    Id = Guid.NewGuid(),
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    DiscountType = request.DiscountType,
                    DiscountValue = request.DiscountValue,
                    MinimumOrderAmount = request.MinimumOrderAmount,
                    MaximumDiscountAmount = request.MaximumDiscountAmount,
                    ValidFrom = request.ValidFrom,
                    ValidTo = request.ValidTo,
                    UsageLimit = request.UsageLimit,
                    UsageLimitPerUser = request.UsageLimitPerUser,
                    IsActive = true,
                    UsageCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                // Serialize applicable product IDs to JSON
                if (request.ApplicableProductIds != null && request.ApplicableProductIds.Any())
                {
                    coupon.ApplicableProductIds = JsonSerializer.Serialize(request.ApplicableProductIds);
                }

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();

                return coupon;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon: {Code}", request.Code);
                return null;
            }
        }

        public async Task<List<Coupon>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Coupons
                .Where(c => c.IsActive == true
                    && (c.ValidFrom == null || c.ValidFrom <= now)
                    && (c.ValidTo == null || c.ValidTo >= now))
                .OrderBy(c => c.ValidTo)
                .ToListAsync();
        }

        public async Task<List<CouponUsage>> GetCouponUsageHistoryAsync(Guid couponId)
        {
            return await _context.CouponUsages
                .Where(cu => cu.CouponId == couponId)
                .OrderByDescending(cu => cu.UsedAt)
                .ToListAsync();
        }
    }
}
