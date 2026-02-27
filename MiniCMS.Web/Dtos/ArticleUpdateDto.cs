using System.ComponentModel.DataAnnotations;

namespace MiniCMS.Web.Dtos
{
    public class ArticleUpdateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}