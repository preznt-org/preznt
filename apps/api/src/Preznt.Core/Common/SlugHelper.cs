using System.Text.RegularExpressions;

namespace Preznt.Core.Common;

/// <summary>
/// Utility class for generating URL-friendly slugs from text.
/// </summary>
public static partial class SlugHelper
{
    /// <summary>
    /// Generates a URL-friendly slug from the input text.
    /// Converts to lowercase, replaces non-alphanumeric characters with hyphens,
    /// and trims leading/trailing hyphens.
    /// </summary>
    /// <param name="input">The text to convert to a slug.</param>
    /// <returns>A URL-friendly slug, or "portfolio" if input results in empty string.</returns>
    public static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "portfolio";

        var slug = input.ToLowerInvariant();
        slug = NonAlphanumericRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return string.IsNullOrEmpty(slug) ? "portfolio" : slug;
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}
