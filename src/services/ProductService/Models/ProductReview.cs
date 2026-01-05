using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class ProductReview
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? ProductId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Title { get; set; }
        public string? ReviewText { get; set; }
        public bool? IsVerifiedPurchase { get; set; }
        public bool? IsApproved { get; set; }
        public int? HelpfulCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
