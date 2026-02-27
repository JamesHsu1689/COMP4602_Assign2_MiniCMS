namespace MiniCMS.Web.Services;

public interface IHtmlSanitizationService
{
    string Sanitize(string html);
}