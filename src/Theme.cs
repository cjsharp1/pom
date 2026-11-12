using System.Drawing.Drawing2D;

namespace Pom;

/// <summary>
/// Central palette and drawing helpers so every control renders from one
/// source of truth in both light and dark themes.
/// </summary>
public static class Theme
{
    // Brand accent, drawn from the tomato icon.
    public static readonly Color Accent = Color.FromArgb(226, 74, 52);
    public static readonly Color AccentHover = Color.FromArgb(240, 92, 70);
    public static readonly Color AccentPressed = Color.FromArgb(198, 58, 38);

    // "Running" / go state for the primary button.
    public static readonly Color Go = Color.FromArgb(46, 160, 97);
    public static readonly Color GoHover = Color.FromArgb(56, 178, 110);
    public static readonly Color GoPressed = Color.FromArgb(38, 138, 83);

    public static Color Background(bool dark) =>
        dark ? Color.FromArgb(28, 28, 30) : Color.FromArgb(246, 246, 248);

    public static Color Surface(bool dark) =>
        dark ? Color.FromArgb(44, 44, 48) : Color.FromArgb(255, 255, 255);

    public static Color SurfaceHover(bool dark) =>
        dark ? Color.FromArgb(58, 58, 63) : Color.FromArgb(238, 238, 242);

    public static Color SurfacePressed(bool dark) =>
        dark ? Color.FromArgb(68, 68, 74) : Color.FromArgb(226, 226, 231);

    public static Color Border(bool dark) =>
        dark ? Color.FromArgb(70, 70, 76) : Color.FromArgb(214, 214, 220);

    public static Color Text(bool dark) =>
        dark ? Color.FromArgb(240, 240, 243) : Color.FromArgb(28, 28, 32);

    public static Color TextMuted(bool dark) =>
        dark ? Color.FromArgb(150, 150, 158) : Color.FromArgb(110, 110, 120);

    public static Color TrackColor(bool dark) =>
        dark ? Color.FromArgb(60, 60, 66) : Color.FromArgb(220, 220, 226);

    /// <summary>Text that sits on top of a filled accent/go surface.</summary>
    public static readonly Color OnAccent = Color.White;

    public static GraphicsPath RoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = Math.Max(1, radius * 2);

        // Degenerate to a plain rectangle if the radius cannot fit.
        if (d > rect.Width || d > rect.Height)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
