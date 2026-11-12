using System.ComponentModel;

namespace Pom;

public class ToggleSwitch : Control
{
    private bool _checked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public event EventHandler? CheckedChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string OffText { get; set; } = "Light";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string OnText { get; set; } = "Dark";

    public ToggleSwitch()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Size = new Size(70, 26);
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        int radius = Height / 2;

        var trackColor = _checked ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
        using (var brush = new SolidBrush(trackColor))
        {
            DrawRoundedRect(g, brush, rect, radius);
        }

        using (var pen = new Pen(Color.FromArgb(120, 120, 120), 1))
        {
            DrawRoundedRectOutline(g, pen, rect, radius);
        }

        int knobDiameter = Height - 6;
        int knobX = _checked ? Width - knobDiameter - 4 : 3;
        int knobY = 3;

        var knobColor = _checked ? Color.FromArgb(240, 240, 240) : Color.FromArgb(80, 80, 80);
        using (var brush = new SolidBrush(knobColor))
        {
            g.FillEllipse(brush, knobX, knobY, knobDiameter, knobDiameter);
        }

        var labelText = _checked ? OnText : OffText;
        var textColor = _checked ? Color.FromArgb(200, 200, 200) : Color.FromArgb(60, 60, 60);
        using var font = new Font("Segoe UI", 7.5f);
        using var textBrush = new SolidBrush(textColor);
        var textSize = g.MeasureString(labelText, font);

        float textX = _checked
            ? 6
            : Width - textSize.Width - 6;
        float textY = (Height - textSize.Height) / 2;

        g.DrawString(labelText, font, textBrush, textX, textY);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        Checked = !Checked;
    }

    private static void DrawRoundedRect(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }

    private static void DrawRoundedRectOutline(Graphics g, Pen pen, Rectangle rect, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        g.DrawPath(pen, path);
    }
}
