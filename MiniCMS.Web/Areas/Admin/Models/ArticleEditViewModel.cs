using System.ComponentModel.DataAnnotations;

namespace MiniCMS.Web.Areas.Admin.Models
{
    public class ArticleEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}