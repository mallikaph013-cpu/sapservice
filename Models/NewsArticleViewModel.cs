using System;

namespace myapp.Models
{
    public class NewsArticleViewModel
    {
        public int Id { get; set; }
        public bool IsFeatured { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime PublishedDate { get; set; }
        public string Author { get; set; } = string.Empty;
        public string ArticleUrl { get; set; } = string.Empty;
    }
}
