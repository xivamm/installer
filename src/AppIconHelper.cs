using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TechInstaller
{
    public static class AppIconHelper
    {
        private static Dictionary<string, Image> _cache24 = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, Image> _cache48 = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Image GetAppIcon(string id, string category, int size)
        {
            Dictionary<string, Image> cache = (size <= 24) ? _cache24 : _cache48;
            string key = (id ?? "") + "_" + (category ?? "");

            if (cache.ContainsKey(key))
            {
                return cache[key];
            }

            Bitmap bmp = GenerateIcon(id, category, size);
            cache[key] = bmp;
            return bmp;
        }

        private static Bitmap GenerateIcon(string id, string category, int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle rect = new Rectangle(1, 1, size - 2, size - 2);
                string cleanId = (id ?? "").ToLowerInvariant();

                if (cleanId.Contains("driver") || cleanId.Contains("booster"))
                {
                    DrawBadge(g, rect, Color.FromArgb(239, 68, 68), Color.FromArgb(185, 28, 28), "⚡", Color.White, size);
                }
                else if (cleanId.Contains("crystal") || cleanId.Contains("disk"))
                {
                    DrawBadge(g, rect, Color.FromArgb(56, 189, 248), Color.FromArgb(2, 132, 199), "💿", Color.White, size);
                }
                else if (cleanId.Contains("cpuz") || cleanId.Contains("cpu"))
                {
                    DrawBadge(g, rect, Color.FromArgb(168, 85, 247), Color.FromArgb(126, 34, 206), "CPU", Color.White, size, true);
                }
                else if (cleanId.Contains("vcredist") || cleanId.Contains("visualcpp"))
                {
                    DrawBadge(g, rect, Color.FromArgb(147, 51, 234), Color.FromArgb(107, 33, 168), "VC+", Color.White, size, true);
                }
                else if (cleanId.Contains("directx"))
                {
                    DrawBadge(g, rect, Color.FromArgb(234, 179, 8), Color.FromArgb(161, 98, 7), "DX", Color.Black, size, true);
                }
                else if (cleanId.Contains("dotnet8") || cleanId.Contains("dotnet"))
                {
                    DrawBadge(g, rect, Color.FromArgb(99, 102, 241), Color.FromArgb(67, 56, 202), ".NET", Color.White, size, true);
                }
                else if (cleanId.Contains("enable_net35"))
                {
                    DrawBadge(g, rect, Color.FromArgb(14, 165, 233), Color.FromArgb(3, 105, 161), "3.5", Color.White, size, true);
                }
                else if (cleanId.Contains("chrome"))
                {
                    DrawChromeIcon(g, rect, size);
                }
                else if (cleanId.Contains("brave"))
                {
                    DrawBadge(g, rect, Color.FromArgb(249, 115, 22), Color.FromArgb(194, 65, 12), "🦁", Color.White, size);
                }
                else if (cleanId.Contains("firefox"))
                {
                    DrawBadge(g, rect, Color.FromArgb(249, 115, 22), Color.FromArgb(234, 88, 12), "🦊", Color.White, size);
                }
                else if (cleanId.Contains("winrar"))
                {
                    DrawBadge(g, rect, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105), "RAR", Color.White, size, true);
                }
                else if (cleanId.Contains("7zip") || cleanId.Contains("7-zip"))
                {
                    DrawBadge(g, rect, Color.FromArgb(30, 41, 59), Color.FromArgb(15, 23, 42), "7z", Color.White, size, true);
                }
                else if (cleanId.Contains("anydesk"))
                {
                    DrawBadge(g, rect, Color.FromArgb(239, 68, 68), Color.FromArgb(185, 28, 28), "◆", Color.White, size);
                }
                else if (cleanId.Contains("vlc"))
                {
                    DrawBadge(g, rect, Color.FromArgb(249, 115, 22), Color.FromArgb(217, 119, 6), "▲", Color.White, size);
                }
                else if (cleanId.Contains("sumatra"))
                {
                    DrawBadge(g, rect, Color.FromArgb(234, 179, 8), Color.FromArgb(202, 138, 4), "PDF", Color.Black, size, true);
                }
                else if (cleanId.Contains("revo"))
                {
                    DrawBadge(g, rect, Color.FromArgb(59, 130, 246), Color.FromArgb(29, 78, 216), "⚙", Color.White, size);
                }
                else if (cleanId.Contains("office"))
                {
                    DrawBadge(g, rect, Color.FromArgb(234, 88, 12), Color.FromArgb(194, 65, 12), "O", Color.White, size, true);
                }
                else if (cleanId.Contains("steam"))
                {
                    DrawBadge(g, rect, Color.FromArgb(15, 23, 42), Color.FromArgb(30, 41, 59), "♨", Color.White, size);
                }
                else if (cleanId.Contains("discord"))
                {
                    DrawBadge(g, rect, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229), "💬", Color.White, size);
                }
                else
                {
                    string cat = (category ?? "").ToLowerInvariant();
                    Color bg1 = Color.FromArgb(37, 99, 235);
                    Color bg2 = Color.FromArgb(29, 78, 216);
                    string label = "APP";

                    if (cat.Contains("driver")) { bg1 = Color.FromArgb(239, 68, 68); bg2 = Color.FromArgb(185, 28, 28); label = "DRV"; }
                    else if (cat.Contains("runtime")) { bg1 = Color.FromArgb(147, 51, 234); bg2 = Color.FromArgb(107, 33, 168); label = "RUN"; }
                    else if (cat.Contains("browser")) { bg1 = Color.FromArgb(14, 165, 233); bg2 = Color.FromArgb(2, 132, 199); label = "WEB"; }
                    else if (cat.Contains("office")) { bg1 = Color.FromArgb(249, 115, 22); bg2 = Color.FromArgb(194, 65, 12); label = "DOC"; }
                    else if (cat.Contains("media")) { bg1 = Color.FromArgb(16, 185, 129); bg2 = Color.FromArgb(5, 150, 105); label = "MED"; }

                    DrawBadge(g, rect, bg1, bg2, label, Color.White, size, true);
                }
            }

            return bmp;
        }

        private static void DrawBadge(Graphics g, Rectangle rect, Color topColor, Color bottomColor, string text, Color textColor, int size, bool isTextBadge = false)
        {
            using (GraphicsPath path = GetRoundedRect(rect, size >= 40 ? 10 : 6))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, topColor, bottomColor, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }

                using (Pen pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
                {
                    g.DrawPath(pen, path);
                }
            }

            float fontSize = isTextBadge ? (size >= 40 ? 11f : 7f) : (size >= 40 ? 16f : 9f);
            using (Font font = new Font(isTextBadge ? "Segoe UI" : "Segoe UI Emoji", fontSize, FontStyle.Bold))
            {
                using (Brush brush = new SolidBrush(textColor))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString(text, font, brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
                }
            }
        }

        private static void DrawChromeIcon(Graphics g, Rectangle rect, int size)
        {
            using (GraphicsPath path = GetRoundedRect(rect, size >= 40 ? 10 : 6))
            {
                using (Brush b = new SolidBrush(Color.FromArgb(15, 23, 42)))
                {
                    g.FillPath(b, path);
                }
            }

            int inset = size >= 40 ? 6 : 3;
            Rectangle circleRect = new Rectangle(rect.X + inset, rect.Y + inset, rect.Width - (inset * 2), rect.Height - (inset * 2));

            using (Brush redBrush = new SolidBrush(Color.FromArgb(239, 68, 68)))
            using (Brush greenBrush = new SolidBrush(Color.FromArgb(34, 197, 94)))
            using (Brush yellowBrush = new SolidBrush(Color.FromArgb(234, 179, 8)))
            {
                g.FillPie(redBrush, circleRect, 210, 120);
                g.FillPie(greenBrush, circleRect, 90, 120);
                g.FillPie(yellowBrush, circleRect, 330, 120);
            }

            int centerInset = size >= 40 ? 14 : 7;
            Rectangle centerRect = new Rectangle(rect.X + centerInset, rect.Y + centerInset, rect.Width - (centerInset * 2), rect.Height - (centerInset * 2));
            using (Brush whiteBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(whiteBrush, centerRect);
            }

            int innerInset = size >= 40 ? 17 : 8;
            Rectangle innerRect = new Rectangle(rect.X + innerInset, rect.Y + innerInset, rect.Width - (innerInset * 2), rect.Height - (innerInset * 2));
            using (Brush blueBrush = new SolidBrush(Color.FromArgb(59, 130, 246)))
            {
                g.FillEllipse(blueBrush, innerRect);
            }
        }

        public static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
