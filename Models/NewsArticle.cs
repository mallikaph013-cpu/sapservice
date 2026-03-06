using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class NewsArticle
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a title.")]
        [StringLength(200, ErrorMessage = "The title must be less than 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Image URL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Published Date")]
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;
        
        [Display(Name = "Is Featured?")]
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(256)]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
