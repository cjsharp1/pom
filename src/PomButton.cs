using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Pom;

public enum PomButtonStyle
{
    /// <summary>Outlined surface button (Reset).</summary>
    Secondary,
    /// <summary>Solid accent-filled button (Start/Pause).</summary>
    Primary,
    /// <summary>Mode selector; fills with accent when selected.</summary>
    Pill
}

public class PomButton : Control
{
    private bool _hovered;
    private bool _pressed;
    private bool _showFocusRing;

    /// <summary>
    /// Set once the user navigates with the keyboard. Until then no focus ring
    /// is drawn, so the window does not open with a ring on the first control.
    /// </summary>
    public static bool KeyboardNavigationUsed { get; set; }
    private bool _isBold;
    private bool _selected;
    private Color _accent = Theme.Accent;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PomButtonStyle ButtonStyle { get; set; } = PomButtonStyle.Secondary;

    /// <summary>Accent used when this button is filled (Primary/selected Pill).</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color Accent
    {
        get => _accent;
        set { _accent = value; Invalidate(); }
    }

    /// <summary>Selected state for <see cref="PomButtonStyle.Pill"/>.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Selected
    {
        get => _selected;
        set { _selected = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsBold
    {
        get => _isBold;
        set { _isBold = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float FontSize { get; set; } = 8.5f;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkBackground { get; set; }

    [AllowNull]
    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value ?? string.Empty;
            Invalidate();
        }
    }

    public PomButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                 | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        Size = new Size(80, 28);
        Cursor = Cursors.Hand;
        TabStop = true;
    }

    private bool IsFilled =>
        ButtonStyle == PomButtonStyle.Primary
        || (ButtonStyle == PomButtonStyle.Pill && _selected);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        // Paint the parent background so the rounded corners stay transparent.
        using (var bg = new SolidBrush(Parent?.BackColor ?? BackColor))
            g.FillRectangle(bg, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        int radius = Height / 2;

        Color fill;
        Color textColor;

        if (IsFilled)
        {
            fill = _pressed ? Darken(_accent, 0.88f)
                 : _hovered ? Lighten(_accent, 1.08f)
                 : _accent;
            textColor = Theme.OnAccent;
        }
        else
        {
            fill = _pressed ? Theme.SurfacePressed(DarkBackground)
                 : _hovered ? Theme.SurfaceHover(DarkBackground)
                 : Theme.Surface(DarkBackground);
            textColor = ButtonStyle == PomButtonStyle.Pill && !_hovered
                ? Theme.TextMuted(DarkBackground)
                : Theme.Text(DarkBackground);
        }

        using (var path = Theme.RoundedRect(rect, radius))
        {
            using (var brush = new SolidBrush(fill))
                g.FillPath(brush, path);

            if (!IsFilled)
            {
                using var pen = new Pen(Theme.Border(DarkBackground), 1);
                g.DrawPath(pen, path);
            }
        }

        // Focus ring, shown only for keyboard navigation.
        if (Focused && TabStop && _showFocusRing && KeyboardNavigationUsed)
        {
            var focusRect = Rectangle.Inflate(rect, -3, -3);
            using var path = Theme.RoundedRect(focusRect, Math.Max(1, radius - 3));
            using var pen = new Pen(IsFilled ? Color.FromArgb(160, 255, 255, 255) : _accent, 1.4f)
            {
                DashStyle = DashStyle.Dot
            };
            g.DrawPath(pen, path);
        }

        var fontStyle = (_isBold || IsFilled) ? FontStyle.Bold : FontStyle.Regular;
        using var font = new Font("Segoe UI", FontSize, fontStyle);
        TextRenderer.DrawText(
            g, Text, font, ClientRectangle, textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
    }

    private static Color Lighten(Color c, float factor) => Color.FromArgb(
        c.A,
        Math.Min(255, (int)(c.R * factor)),
        Math.Min(255, (int)(c.G * factor)),
        Math.Min(255, (int)(c.B * factor)));

    private static Color Darken(Color c, float factor) => Color.FromArgb(
        c.A,
        (int)(c.R * factor),
        (int)(c.G * factor),
        (int)(c.B * factor));

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            _showFocusRing = false;
            Focus();
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        // Only treat it as keyboard focus if the mouse is not what moved it here.
        if (MouseButtons == MouseButtons.None && !_hovered)
            _showFocusRing = true;
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        _showFocusRing = false;
        Invalidate();
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Space or Keys.Enter || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = true;
            _showFocusRing = true;
            KeyboardNavigationUsed = true;
            Invalidate();
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = false;
            Invalidate();
            OnClick(EventArgs.Empty);
        }
    }
}
