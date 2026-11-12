using System.ComponentModel;

namespace Pom;

public class SliderControl : Control
{
    private int _value = 100;
    private bool _dragging;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Minimum { get; set; } = 30;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Maximum { get; set; } = 100;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, Minimum, Maximum);
            if (_value == clamped) return;
            _value = clamped;
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public event EventHandler? ValueChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsDark { get; set; }

    public SliderControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Size = new Size(150, 26);
        Cursor = Cursors.Hand;
    }

    private int LabelWidth => 52;
    private int TrackLeft => LabelWidth + 4;
    private int TrackRight => Width - 8;
    private int TrackWidth => TrackRight - TrackLeft;
    private int KnobDiameter => Height - 8;

    private int ValueToX(int val)
    {
        float ratio = (float)(val - Minimum) / (Maximum - Minimum);
        return TrackLeft + (int)(ratio * (TrackWidth - KnobDiameter));
    }

    private int XToValue(int x)
    {
        int knobRadius = KnobDiameter / 2;
        float ratio = (float)(x - TrackLeft - knobRadius) / (TrackWidth - KnobDiameter);
        ratio = Math.Clamp(ratio, 0, 1);
        return Minimum + (int)(ratio * (Maximum - Minimum));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var outerRect = new Rectangle(0, 0, Width - 1, Height - 1);
        int outerRadius = Height / 2;

        // Pill background (matches toggle switch style)
        var pillColor = IsDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
        using (var brush = new SolidBrush(pillColor))
        {
            DrawRoundedRect(g, brush, outerRect, outerRadius);
        }

        // Pill border
        using (var pen = new Pen(Color.FromArgb(120, 120, 120), 1))
        {
            DrawRoundedRectOutline(g, pen, outerRect, outerRadius);
        }

        // "Opacity" label
        var textColor = IsDark ? Color.FromArgb(200, 200, 200) : Color.FromArgb(60, 60, 60);
        using var font = new Font("Segoe UI", 7.5f);
        using var textBrush = new SolidBrush(textColor);
        var textSize = g.MeasureString("Opacity", font);
        float textY = (Height - textSize.Height) / 2;
        g.DrawString("Opacity", font, textBrush, 8, textY);

        // Track
        int trackHeight = 6;
        var trackRect = new Rectangle(TrackLeft, (Height - trackHeight) / 2, TrackWidth, trackHeight);
        int trackRadius = 3;

        var trackColor = IsDark ? Color.FromArgb(40, 40, 40) : Color.FromArgb(160, 160, 160);
        using (var brush = new SolidBrush(trackColor))
        {
            DrawRoundedRect(g, brush, trackRect, trackRadius);
        }

        // Fill
        float fillRatio = (float)(_value - Minimum) / (Maximum - Minimum);
        var fillRect = new Rectangle(trackRect.X, trackRect.Y, (int)(trackRect.Width * fillRatio), trackRect.Height);
        if (fillRect.Width > 0)
        {
            using var brush = new SolidBrush(Color.FromArgb(80, 140, 200));
            DrawRoundedRect(g, brush, fillRect, trackRadius);
        }

        // Knob
        int knobX = ValueToX(_value);
        int knobY = 4;
        using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
        {
            g.FillEllipse(brush, knobX, knobY, KnobDiameter, KnobDiameter);
        }
        using (var pen = new Pen(Color.FromArgb(120, 120, 120), 1))
        {
            g.DrawEllipse(pen, knobX, knobY, KnobDiameter, KnobDiameter);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.X >= TrackLeft)
        {
            _dragging = true;
            Value = XToValue(e.X);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
            Value = XToValue(e.X);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
    }

    private static void DrawRoundedRect(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        if (rect.Width < 1 || rect.Height < 1) return;
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }

    private static void DrawRoundedRectOutline(Graphics g, Pen pen, Rectangle rect, int radius)
    {
        if (rect.Width < 1 || rect.Height < 1) return;
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        g.DrawPath(pen, path);
    }
}
