using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}