using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Pom;

public static class IconGenerator
{
    public static Icon CreateTomatoIcon()
    {
        var bmp = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            // Tomato body
            var bodyRect = new Rectangle(3, 6, 26, 24);
            using (var brush = new SolidBrush(Color.FromArgb(220, 50, 30)))
                g.FillEllipse(brush, bodyRect);

            // Highlight
            using (var brush = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                g.FillEllipse(brush, 9, 9, 9, 9);

            // Stem
            using (var brush = new SolidBrush(Color.FromArgb(60, 140, 40)))
                g.FillRectangle(brush, 14, 1, 4, 6);

            // Leaf
            using (var brush = new SolidBrush(Color.FromArgb(80, 170, 50)))
                g.FillEllipse(brush, 11, 3, 10, 5);
        }

        var handle = bmp.GetHicon();
        return Icon.FromHandle(handle);
    }
}
