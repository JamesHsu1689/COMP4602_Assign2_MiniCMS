using Ganss.Xss;
using AngleSharp.Dom;

namespace MiniCMS.Web.Services;

public sealed class HtmlSanitizationService : IHtmlSanitizationService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizationService()
    {
        _sanitizer = new HtmlSanitizer();

        // Start from a conservative allowlist and explicitly add only what we want.
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedCssProperties.Clear(); // we are not allowing style="" anyway

        // ---- Allowed tags (Quill-friendly but safe) ----
        // Basic formatting
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("u");
        _sanitizer.AllowedTags.Add("s");
        _sanitizer.AllowedTags.Add("blockquote");

        // Headings
        _sanitizer.AllowedTags.Add("h1");
        _sanitizer.AllowedTags.Add("h2");
        _sanitizer.AllowedTags.Add("h3");
        _sanitizer.AllowedTags.Add("h4");
        _sanitizer.AllowedTags.Add("h5");
        _sanitizer.AllowedTags.Add("h6");

        // Lists
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");

        // Code blocks
        _sanitizer.AllowedTags.Add("pre");
        _sanitizer.AllowedTags.Add("code");

        // Links
        _sanitizer.AllowedTags.Add("a");

        // Quill often uses spans/classes for formatting (alignment/size/etc.).
        // We allow span/div BUT restrict attributes tightly (no style, no events).
        _sanitizer.AllowedTags.Add("span");
        _sanitizer.AllowedTags.Add("div");

        // ---- Allowed attributes ----
        // Quill uses class names like ql-align-center, ql-size-large, etc.
        _sanitizer.AllowedAttributes.Add("class");

        // Links (no javascript: because of AllowedSchemes below)
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("target");
        _sanitizer.AllowedAttributes.Add("rel");

        // Optional Quill list metadata (safe data-* allowlist)
        _sanitizer.AllowedAttributes.Add("data-list");
        _sanitizer.AllowedAttributes.Add("data-checked");

        // ---- Allowed URL schemes ----
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        // Add a small safety hardening for links opening new tabs.
        _sanitizer.PostProcessNode += (sender, e) =>
        {
            if (e.Node is not IElement el)
                return;

            // TagName exists on IElement (for HTML tags)
            if (!string.Equals(el.TagName, "A", StringComparison.OrdinalIgnoreCase))
                return;

            var target = el.GetAttribute("target");
            if (string.Equals(target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                // Keep any existing rel tokens and ensure noopener/noreferrer exist
                var rel = (el.GetAttribute("rel") ?? string.Empty)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                rel.Add("noopener");
                rel.Add("noreferrer");

                el.SetAttribute("rel", string.Join(' ', rel));
            }
        };
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _sanitizer.Sanitize(html);
    }
}