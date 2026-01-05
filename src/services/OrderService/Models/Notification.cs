using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } // PK - Not nullable
        public Guid? UserId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? Priority { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
