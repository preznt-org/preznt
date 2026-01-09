using Preznt.Core.Common;
using Preznt.Core.Entities;

namespace Preznt.Core.Interfaces.Services;

public interface IThemeRenderService
{
    /// <summary>
    /// Renders a portfolio using the specified theme.
    /// </summary>
    Task<Result<RenderedPortfolio>> RenderAsync(Portfolio portfolio, string themeId, CancellationToken ct = default);

    /// <summary>
    /// Renders a sample portfolio for theme preview.
    /// </summary>
    Task<Result<string>> RenderPreviewAsync(string themeId, CancellationToken ct = default);

    /// <summary>
    /// Gets the list of files needed for the rendered portfolio (HTML, CSS, assets).
    /// </summary>
    Task<Result<IReadOnlyList<RenderedFile>>> GetRenderedFilesAsync(Portfolio portfolio, string themeId, CancellationToken ct = default);
}

/// <summary>
/// Represents a fully rendered portfolio ready for deployment.
/// </summary>
public sealed record RenderedPortfolio(
    string Html,
    string Css,
    IReadOnlyList<RenderedFile> Files);

/// <summary>
/// Represents a single file in the rendered output.
/// </summary>
public sealed record RenderedFile(
    string Path,
    string Content,
    bool IsBinary = false);
