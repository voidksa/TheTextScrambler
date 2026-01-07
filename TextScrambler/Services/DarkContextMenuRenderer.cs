using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TextScrambler.Services
{
    public class DarkContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkContextMenuRenderer() : base(new DarkColorTable())
        {
            this.RoundedEdges = true;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Enabled)
            {
                if (e.Item.Selected)
                {
                    // Windows 11 Style: Rounded selection rectangle, not full width
                    Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
                    // Add some margin for the floating look
                    rc.Inflate(-4, -2);

                    using (var brush = new SolidBrush(Color.FromArgb(255, 60, 60, 60)))
                    using (var path = GetRoundedPath(rc, 4))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            // Clean flat background for image area, same as menu
            using (var brush = new SolidBrush(Color.FromArgb(32, 32, 32)))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // Subtle thin border
            using (var pen = new Pen(Color.FromArgb(60, 60, 60)))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Color.White;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.White;
            base.OnRenderItemText(e);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        // Backgrounds
        public override Color ToolStripDropDownBackground => Color.FromArgb(32, 32, 32); // Darker, cleaner grey
        public override Color MenuBorder => Color.FromArgb(60, 60, 60);
        public override Color MenuItemBorder => Color.Transparent;

        // Image Margin (Make it invisible/blend in)
        public override Color ImageMarginGradientBegin => Color.FromArgb(32, 32, 32);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(32, 32, 32);
        public override Color ImageMarginGradientEnd => Color.FromArgb(32, 32, 32);

        // Selection (Handled by OnRenderMenuItemBackground, but setting these to transparent prevents default artifacts)
        public override Color MenuItemSelected => Color.Transparent;
        public override Color MenuItemSelectedGradientBegin => Color.Transparent;
        public override Color MenuItemSelectedGradientEnd => Color.Transparent;
        public override Color MenuItemPressedGradientBegin => Color.Transparent;
        public override Color MenuItemPressedGradientEnd => Color.Transparent;
    }

    public class LightContextMenuRenderer : ToolStripProfessionalRenderer
    {
        public LightContextMenuRenderer() : base(new LightColorTable())
        {
            this.RoundedEdges = true;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Enabled)
            {
                if (e.Item.Selected)
                {
                    Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
                    rc.Inflate(-4, -2);

                    using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240))) // Light grey selection
                    using (var path = GetRoundedPath(rc, 4))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillPath(brush, path);
                    }
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb(255, 255, 255)))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Color.Black;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.Black;
            base.OnRenderItemText(e);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class LightColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color MenuBorder => Color.FromArgb(220, 220, 220);
        public override Color MenuItemBorder => Color.Transparent;
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
        public override Color MenuItemSelected => Color.Transparent;
        public override Color MenuItemSelectedGradientBegin => Color.Transparent;
        public override Color MenuItemSelectedGradientEnd => Color.Transparent;
        public override Color MenuItemPressedGradientBegin => Color.Transparent;
        public override Color MenuItemPressedGradientEnd => Color.Transparent;
    }
}
