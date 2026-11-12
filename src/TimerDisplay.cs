using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Pom;

/// <summary>
/// The large MM:SS readout inside a circular progress ring that drains as time elapses.
/// </summary>
public class TimerDisplay : Control
{
    private double _progress = 1.0;
    private Color _accent = Theme.Accent;
    private bool _isDark;

    /// <summary>Remaining fraction, 1.0 = full, 0.0 = done.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Progress
    {
        get => _progress;
        set
        {
            var clamped = Math.Clamp(value, 0.0, 1.0);
            if (Math.Abs(clamped - _progress) < 0.0001) return;
            _progress = clamped;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color Accent
    {
        get => _accent;
        set { _accent = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsDark
    {
        get => _isDark;
        set { _isDark = value; Invalidate(); }
    }

    /// <summary>Small caption under the time, e.g. the current mode.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Caption { get; set; } = string.Empty;

    public TimerDisplay()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                 | ControlStyles.OptimizedDoubleBuffer, true);
        Text = "25:00";
    }

    public void SetTime(string text)
    {
        Text = text;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using (var bg = new SolidBrush(BackColor))
            g.FillRectangle(bg, ClientRectangle);

        // Circular progress ring around the readout - a tomato is round.
        const float thickness = 7f;
        int diameter = Math.Min(Width, Height) - (int)thickness - 8;
        if (diameter < 40) return;

        var ring = new RectangleF(
            (Width - diameter) / 2f,
            (Height - diameter) / 2f,
            diameter,
            diameter);

        // Track.
        using (var pen = new Pen(Theme.TrackColor(_isDark), thickness)
               { StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            g.DrawArc(pen, ring, -90f + 6f, 360f - 12f);
        }

        // Remaining-time sweep. A small gap at 12 o'clock gives the arc a visible
        // start and end, so "full" and "almost full" are not identical.
        const float gap = 6f;
        float sweep = (float)((360.0 - gap * 2) * _progress);
        if (sweep > 0.5f)
        {
            using var brush = new LinearGradientBrush(
                ring, Lighten(_accent, 1.22f), _accent, LinearGradientMode.ForwardDiagonal);
            using var pen = new Pen(brush, thickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            g.DrawArc(pen, ring, -90f + gap, sweep);
        }

        // Time readout, centred in the ring.
        var inner = Rectangle.Round(ring);
        using var font = new Font("Segoe UI", 30f, FontStyle.Bold, GraphicsUnit.Point);
        var timeArea = new Rectangle(inner.X, inner.Y - 6, inner.Width, inner.Height);
        TextRenderer.DrawText(
            g, Text, font, timeArea, Theme.Text(_isDark),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

        // Mode caption, tucked under the time inside the ring.
        if (!string.IsNullOrEmpty(Caption))
        {
            using var capFont = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            var capArea = new Rectangle(inner.X, inner.Y + inner.Height / 2 + 12, inner.Width, 16);
            TextRenderer.DrawText(
                g, Caption.ToUpperInvariant(), capFont, capArea, Theme.TextMuted(_isDark),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }
    }

    private static Color Lighten(Color c, float factor) => Color.FromArgb(
        c.A,
        Math.Min(255, (int)(c.R * factor)),
        Math.Min(255, (int)(c.G * factor)),
        Math.Min(255, (int)(c.B * factor)));
}
