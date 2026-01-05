using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Coupon
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsageCount { get; set; }
        public int? UsageLimitPerUser { get; set; }
        public bool? IsActive { get; set; }
        public string? ApplicableProductIds { get; set; } // JSON
        public string? ApplicableCategoryIds { get; set; } // JSON
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }

    public class CouponUsage
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? CouponId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? UserId { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
