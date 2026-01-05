using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class CouponAppliedEvent
    {
        public Guid CouponUsageId { get; set; }
        public Guid CouponId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal OrderTotalBeforeDiscount { get; set; }
        public decimal OrderTotalAfterDiscount { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
