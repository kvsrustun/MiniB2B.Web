using System.ComponentModel.DataAnnotations;

namespace MiniB2B.Web.Models
{
    public class Banner
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string BadgeText { get; set; } = "KAMPANYA";

        [MaxLength(50)]
        public string BadgeColor { get; set; } = "warning";

        [MaxLength(200)]
        public string BackgroundGradient { get; set; } = "linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)";

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }
}