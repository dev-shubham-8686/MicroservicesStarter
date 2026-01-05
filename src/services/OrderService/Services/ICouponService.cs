using OrderService.Models;
using static Azure.Core.HttpHeader;

namespace OrderService.Services
{
    public interface ICouponService
    {
        Task<CouponApplicationResult> ApplyCouponAsync(ApplyCouponRequest request);
        Task<Coupon?> GetCouponByCodeAsync(string code);
        Task<Coupon?> CreateCouponAsync(CreateCouponRequest request);
        Task<List<Coupon>> GetActiveCouponsAsync();
        Task<List<CouponUsage>> GetCouponUsageHistoryAsync(Guid couponId);
    }

    public class ApplyCouponRequest
    {
        public string CouponCode { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal OrderTotal { get; set; }
        public List<Guid>? ProductIds { get; set; }
    }

    public class CouponApplicationResult
    {
        public bool Success { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public Guid? CouponId { get; set; }
        public string? Message { get; set; }
        public int? ResultCode { get; set; }
    }

    public class CreateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = string.Empty; // PERCENTAGE or FIXED_AMOUNT
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsageLimitPerUser { get; set; }
        public List<Guid>? ApplicableProductIds { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
