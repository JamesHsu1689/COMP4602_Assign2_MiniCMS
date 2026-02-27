using System;
using System.ComponentModel.DataAnnotations;

namespace MiniCMS.Web.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        // Will store sanitized HTML later (next milestone)
        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}