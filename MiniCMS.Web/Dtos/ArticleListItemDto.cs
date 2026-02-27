using System;

namespace MiniCMS.Web.Dtos
{
    public class ArticleListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}