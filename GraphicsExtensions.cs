using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZooTycoonCardFlipReimpl;

internal static class GraphicsExtensions
{
	public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
	{
		using GraphicsPath path = RoundedRectangle(bounds, radius);
		graphics.FillPath(brush, path);
	}

	public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
	{
		using GraphicsPath path = RoundedRectangle(bounds, radius);
		graphics.DrawPath(pen, path);
	}

	private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
	{
		int num = radius * 2;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(bounds.Left, bounds.Top, num, num, 180f, 90f);
		graphicsPath.AddArc(bounds.Right - num, bounds.Top, num, num, 270f, 90f);
		graphicsPath.AddArc(bounds.Right - num, bounds.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(bounds.Left, bounds.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}
}
