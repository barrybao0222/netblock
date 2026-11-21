// ============================================================
//  NetBlock · 一键禁止联网工具
//  作者：鬼哥Coding   博客：https://gui.ge
//  单文件 WinForms · .NET Framework 4.x
// ============================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NetBlock
{
    // ------------------------------------------------ 主题
    internal static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(245, 247, 250);
        public static readonly Color Card = Color.White;
        public static readonly Color Border = Color.FromArgb(230, 235, 242);
        public static readonly Color Primary = Color.FromArgb(37, 99, 235);
        public static readonly Color PrimaryDark = Color.FromArgb(29, 78, 216);
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);
        public static readonly Color DangerDark = Color.FromArgb(220, 38, 38);
        public static readonly Color Success = Color.FromArgb(16, 185, 129);
        public static readonly Color Text = Color.FromArgb(17, 24, 39);
        public static readonly Color Sub = Color.FromArgb(107, 114, 128);
        public static readonly Color SubLight = Color.FromArgb(148, 163, 184);
        public static readonly Color RowHover = Color.FromArgb(247, 250, 253);
        public static readonly Color RowSel = Color.FromArgb(239, 246, 255);
        public static readonly Color Warn = Color.FromArgb(217, 119, 6);

        public static Font F(float size, FontStyle style)
        {
            return new Font("Microsoft YaHei UI", size, style, GraphicsUnit.Point);
        }
    }

    // ------------------------------------------------ 绘图/圆角
    internal static class Gfx
    {
        public static GraphicsPath Round(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            if (r.Width <= 0 || r.Height <= 0) return p;
            int d = radius * 2;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            if (d <= 1) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d - 1, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d - 1, r.Bottom - d - 1, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d - 1, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
        public static void FillRound(Graphics g, Rectangle r, int radius, Color c)
        {
            using (GraphicsPath p = Round(r, radius))
            using (SolidBrush b = new SolidBrush(c)) g.FillPath(b, p);
        }
        public static void DrawRound(Graphics g, Rectangle r, int radius, Color c, float w)
        {
            using (GraphicsPath p = Round(r, radius))
            using (Pen pen = new Pen(c, w)) g.DrawPath(pen, p);
        }
        public static Color Light(Color c, int d)
        {
            return Color.FromArgb(c.A, Math.Min(255, c.R + d), Math.Min(255, c.G + d), Math.Min(255, c.B + d));
        }
        public static Color Dark(Color c, int d)
        {
            return Color.FromArgb(c.A, Math.Max(0, c.R - d), Math.Max(0, c.G - d), Math.Max(0, c.B - d));
        }
        public static void DrawNoNet(Graphics g, RectangleF r, Color ring, Color slash)
        {
            float w = Math.Max(2f, r.Width / 18f);
            using (Pen p = new Pen(ring, w))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawEllipse(p, r);
                g.DrawEllipse(p, r.X + r.Width * 0.30f, r.Y, r.Width * 0.40f, r.Height);
                g.DrawLine(p, r.X, r.Y + r.Height / 2f, r.Right, r.Y + r.Height / 2f);
                g.DrawArc(p, r.X, r.Y + r.Height * 0.20f, r.Width, r.Height * 0.60f, 0, 180);
                g.DrawArc(p, r.X, r.Y + r.Height * 0.20f, r.Width, r.Height * 0.60f, 180, 180);
            }
            using (Pen sp = new Pen(Color.White, w * 2.1f))
            {
                sp.StartCap = LineCap.Round; sp.EndCap = LineCap.Round;
                g.DrawLine(sp, r.X + r.Width * 0.12f, r.Bottom - r.Height * 0.12f, r.Right - r.Width * 0.12f, r.Y + r.Height * 0.12f);
            }
            using (Pen sp = new Pen(slash, w * 1.35f))
            {
                sp.StartCap = LineCap.Round; sp.EndCap = LineCap.Round;
                g.DrawLine(sp, r.X + r.Width * 0.12f, r.Bottom - r.Height * 0.12f, r.Right - r.Width * 0.12f, r.Y + r.Height * 0.12f);
            }
        }
    }

    // ------------------------------------------------ 卡片
    internal class Card : Panel
    {
        public Card()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle body = new Rectangle(0, 0, Width - 1, Height - 3);
            for (int i = 3; i >= 1; i--)
            {
                Rectangle sr = new Rectangle(i, i + 1, Width - 1 - i * 2, Height - 3 - i * 2);
                Gfx.FillRound(g, sr, 14, Color.FromArgb(7, 100, 130, 170));
            }
            Gfx.FillRound(g, body, 14, Theme.Card);
            Gfx.DrawRound(g, body, 14, Theme.Border, 1f);
            base.OnPaint(e);
        }
    }

    // ------------------------------------------------ 按钮
    internal class Btn : Control
    {
        public Color Fill1 = Theme.Primary;
        public Color Fill2 = Theme.PrimaryDark;
        public Color TextColor = Color.White;
        public bool Outline;
        private bool hover, down;

        public Btn()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Font = Theme.F(10.5f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            TabStop = false;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            if (r.Width <= 2 || r.Height <= 2) return;

            if (Outline)
            {
                Color bg = !Enabled ? Color.FromArgb(244, 246, 249) : (hover ? Color.FromArgb(240, 245, 253) : Color.White);
                Gfx.FillRound(g, r, 10, bg);
                Gfx.DrawRound(g, r, 10, !Enabled ? Theme.Border : Color.FromArgb(203, 213, 225), 1.2f);
                TextRenderer.DrawText(g, Text, Font, r, !Enabled ? Theme.SubLight : Theme.Primary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                return;
            }

            Color c1 = Fill1, c2 = Fill2;
            if (!Enabled) { c1 = Color.FromArgb(203, 209, 217); c2 = Color.FromArgb(186, 193, 202); }
            else if (down) { c1 = Gfx.Dark(Fill1, 24); c2 = Gfx.Dark(Fill2, 24); }
            else if (hover) { c1 = Gfx.Light(Fill1, 16); c2 = Gfx.Light(Fill2, 10); }

            using (GraphicsPath p = Gfx.Round(r, 10))
            using (LinearGradientBrush br = new LinearGradientBrush(r, c1, c2, 90f))
                g.FillPath(br, p);
            TextRenderer.DrawText(g, Text, Font, r, TextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    // ------------------------------------------------ 窗口小按钮
    internal class Glyph : Control
    {
        public bool IsClose;
        private bool hover;
        public Glyph()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            TabStop = false;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (hover)
                Gfx.FillRound(g, new Rectangle(0, 0, Width - 1, Height - 1), 7,
                    IsClose ? Color.FromArgb(239, 68, 68) : Color.FromArgb(235, 239, 245));
            using (Pen pen = new Pen(hover && IsClose ? Color.White : Theme.Sub, 1.7f))
            {
                pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                int cx = Width / 2, cy = Height / 2;
                if (IsClose)
                {
                    g.DrawLine(pen, cx - 5, cy - 5, cx + 5, cy + 5);
                    g.DrawLine(pen, cx + 5, cy - 5, cx - 5, cy + 5);
                }
                else g.DrawLine(pen, cx - 6, cy + 1, cx + 6, cy + 1);
            }
        }
    }

    // ------------------------------------------------ 程序条目
    internal class ProgItem
    {
        public string Path = "";
        public string Name = "";
        public bool Blocked;
        public bool Exists = true;
        public Bitmap Icon;
        public string Key = "";
    }

    // ------------------------------------------------ 程序列表
    internal class ProgramList : Control
    {
        public const int RowH = 64;
        public List<ProgItem> Items = new List<ProgItem>();
        public int Selected = -1;
        public event Action<int> ToggleRequested;
        public event Action<int> SelectionChanged;
        public event Action<int> RemoveRequested;

        private int offset;
        private int hover = -1;
        private bool downOnToggle;
        private VScrollBar bar;
        private ContextMenuStrip menu;
        private int menuIndex = -1;

        public ProgramList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.White;
            AllowDrop = true;

            bar = new VScrollBar();
            bar.Width = 14;
            bar.SmallChange = RowH;
            bar.LargeChange = RowH * 4;
            bar.Visible = false;
            bar.ValueChanged += delegate { offset = bar.Value; Invalidate(); };
            Controls.Add(bar);

            menu = new ContextMenuStrip();
            menu.Items.Add("打开文件所在位置", null, delegate { OpenFolder(); });
            menu.Items.Add("从列表移除", null, delegate
            {
                if (menuIndex >= 0 && RemoveRequested != null) RemoveRequested(menuIndex);
            });
            ContextMenuStrip = menu;
        }

        private void OpenFolder()
        {
            if (menuIndex < 0 || menuIndex >= Items.Count) return;
            try
            {
                string p = Items[menuIndex].Path;
                if (File.Exists(p)) Process.Start("explorer.exe", "/select,\"" + p + "\"");
                else if (Directory.Exists(Path.GetDirectoryName(p))) Process.Start("explorer.exe", "\"" + Path.GetDirectoryName(p) + "\"");
            }
            catch { }
        }

        public void RefreshScroll()
        {
            bar.SetBounds(Width - 16, 4, 14, Math.Max(20, Height - 8));
            int total = Items.Count * RowH + 8;
            int max = Math.Max(0, total - Height);
            bar.Maximum = max + bar.LargeChange - 1;
            bar.LargeChange = Math.Max(1, Height);
            bar.Visible = max > 0;
            if (offset > max) offset = max;
            bar.Value = Math.Min(offset, Math.Max(0, max));
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RefreshScroll();
        }

        private Rectangle ToggleRect(int y)
        {
            return new Rectangle(Width - 96, y + 19, 48, 26);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.White);

            if (Items.Count == 0) { DrawEmpty(g); return; }

            int y = 4 - offset;
            for (int i = 0; i < Items.Count; i++, y += RowH)
            {
                if (y + RowH < 0) continue;
                if (y > Height) break;
                ProgItem it = Items[i];
                bool sel = (i == Selected), hov = (i == hover && !sel);

                Rectangle row = new Rectangle(4, y, Width - (bar.Visible ? 26 : 12), RowH - 6);
                if (sel) Gfx.FillRound(g, row, 10, Theme.RowSel);
                else if (hov) Gfx.FillRound(g, row, 10, Theme.RowHover);
                if (sel)
                {
                    Rectangle accent = new Rectangle(4, y + 12, 3, RowH - 30);
                    Gfx.FillRound(g, accent, 2, Theme.Primary);
                }

                if (it.Icon != null)
                    g.DrawImage(it.Icon, new Rectangle(18, y + 14, 34, 34));
                else
                    Gfx.FillRound(g, new Rectangle(18, y + 14, 34, 34), 9, Color.FromArgb(232, 238, 246));

                int tx = 64;
                int tw = Width - tx - 110 - (bar.Visible ? 26 : 0);
                if (tw < 40) tw = 40;
                Color nameColor = it.Exists ? Theme.Text : Theme.SubLight;
                using (Font f1 = Theme.F(10f, FontStyle.Bold))
                    TextRenderer.DrawText(g, it.Name, f1, new Rectangle(tx, y + 10, tw, 20), nameColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                using (Font f2 = Theme.F(8f, FontStyle.Regular))
                    TextRenderer.DrawText(g, it.Exists ? Short(it.Path) : "文件已不存在：" + Short(it.Path), f2,
                        new Rectangle(tx, y + 32, tw, 18), Theme.Sub,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                // 状态文字
                using (Font f3 = Theme.F(8f, FontStyle.Bold))
                    TextRenderer.DrawText(g, it.Blocked ? "已禁止联网" : "可联网", f3,
                        new Rectangle(Width - 172, y + 19, 70, 26),
                        it.Blocked ? Theme.Danger : Theme.SubLight,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

                DrawToggle(g, ToggleRect(y), it.Blocked);

                if (i < Items.Count - 1)
                    using (Pen sep = new Pen(Color.FromArgb(242, 245, 249), 1f))
                        g.DrawLine(sep, 18, y + RowH - 4, Width - 24, y + RowH - 4);
            }
        }

        private static string Short(string p)
        {
            if (string.IsNullOrEmpty(p)) return "";
            if (p.Length <= 78) return p;
            return p.Substring(0, 26) + " … " + p.Substring(p.Length - 46);
        }

        private void DrawToggle(Graphics g, Rectangle r, bool on)
        {
            Color back = on ? Theme.Danger : Color.FromArgb(212, 218, 227);
            Gfx.FillRound(g, r, r.Height / 2, back);
            int d = r.Height - 6;
            int cx = on ? r.Right - d - 3 : r.X + 3;
            g.FillEllipse(Brushes.White, new Rectangle(cx, r.Y + 3, d, d));
        }

        private void DrawEmpty(Graphics g)
        {
            Rectangle box = new Rectangle(20, 22, Width - 50, Height - 46);
            Gfx.FillRound(g, box, 12, Color.FromArgb(250, 252, 255));
            using (GraphicsPath p = Gfx.Round(box, 12))
            using (Pen pen = new Pen(Color.FromArgb(196, 211, 228), 1.6f))
            {
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 5f, 4f };
                g.DrawPath(pen, p);
            }

            float icon = 58f;
            float cx = Width / 2f;
            float cy = Height / 2f;
            Gfx.DrawNoNet(g, new RectangleF(cx - icon / 2f, cy - 78f, icon, icon), Theme.Primary, Theme.Danger);

            using (Font f1 = Theme.F(12.5f, FontStyle.Bold))
            using (Font f2 = Theme.F(8.5f, FontStyle.Regular))
            using (Font f3 = Theme.F(8.5f, FontStyle.Regular))
            {
                TextRenderer.DrawText(g, "把程序图标拖进来", f1,
                    new Rectangle(0, (int)(cy - 8f), Width, 26), Theme.Text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, "支持 .lnk 快捷方式 / .exe 程序 / 文件夹（取其下的 EXE）", f2,
                    new Rectangle(0, (int)(cy + 22f), Width, 20), Theme.Sub,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, "也可以点下方「添加程序」手动选择", f3,
                    new Rectangle(0, (int)(cy + 44f), Width, 20), Theme.SubLight,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private int HitTest(int y)
        {
            int idx = (y - 4 + offset) / RowH;
            if (idx < 0 || idx >= Items.Count) return -1;
            return idx;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = HitTest(e.Y);
            if (idx != hover) { hover = idx; Invalidate(); }
            if (idx >= 0)
            {
                bool onToggle = ToggleRect(idx * RowH + 4 - offset).Contains(e.Location);
                Cursor = onToggle ? Cursors.Hand : Cursors.Default;
            }
            else Cursor = Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hover != -1) { hover = -1; Invalidate(); }
            Cursor = Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            int idx = HitTest(e.Y);
            if (idx < 0) return;
            downOnToggle = ToggleRect(idx * RowH + 4 - offset).Contains(e.Location);
            if (!downOnToggle)
            {
                Selected = idx;
                if (SelectionChanged != null) SelectionChanged(idx);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left || !downOnToggle) return;
            int idx = HitTest(e.Y);
            if (idx >= 0 && ToggleRect(idx * RowH + 4 - offset).Contains(e.Location))
            {
                downOnToggle = false;
                if (ToggleRequested != null) ToggleRequested(idx);
            }
            downOnToggle = false;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Right)
            {
                int idx = HitTest(e.Y);
                if (idx >= 0) { menuIndex = idx; Selected = idx; Invalidate(); }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int max = Math.Max(0, Items.Count * RowH + 8 - Height);
            offset -= Math.Sign(e.Delta) * RowH;
            if (offset < 0) offset = 0;
            if (offset > max) offset = max;
            RefreshScroll();
        }
    }

    // ------------------------------------------------ 防火墙/持久化引擎
    internal static class Engine
    {
        public const string Prefix = "NetBlock";

        public static string DataDir
        {
            get
            {
                string d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NetBlock");
                try { if (!Directory.Exists(d)) Directory.CreateDirectory(d); } catch { }
                return d;
            }
        }
        public static string ListPath { get { return Path.Combine(DataDir, "programs.txt"); } }

        public static string KeyOf(string path)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] h = md5.ComputeHash(Encoding.UTF8.GetBytes(path.ToLowerInvariant()));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 5; i++) sb.Append(h[i].ToString("x2"));
                return sb.ToString();
            }
        }

        public static string Run(string file, string args, Encoding enc, out int code)
        {
            code = -1;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(file, args);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                if (enc != null) { psi.StandardOutputEncoding = enc; psi.StandardErrorEncoding = enc; }
                using (Process p = new Process())
                {
                    p.StartInfo = psi;
                    p.Start();
                    string so = p.StandardOutput.ReadToEnd();
                    string se = p.StandardError.ReadToEnd();
                    if (!p.WaitForExit(120000)) { try { p.Kill(); } catch { } }
                    code = p.HasExited ? p.ExitCode : -1;
                    return (so + se).Trim();
                }
            }
            catch (Exception ex) { return "EXCEPTION: " + ex.Message; }
        }

        public static bool AddRules(string path, out string err)
        {
            err = "";
            string key = KeyOf(path);
            RemoveRules(path);
            int code;
            string a1 = "advfirewall firewall add rule name=\"" + Prefix + "_OUT_" + key + "\" dir=out action=block program=\"" +
                        path + "\" enable=yes profile=any description=\"NetBlock\"";
            string o1 = Run("netsh", a1, Encoding.Default, out code);
            if (code != 0) err = o1;

            string a2 = "advfirewall firewall add rule name=\"" + Prefix + "_IN_" + key + "\" dir=in action=block program=\"" +
                        path + "\" enable=yes profile=any description=\"NetBlock\"";
            string o2 = Run("netsh", a2, Encoding.Default, out code);
            if (code != 0 && err.Length == 0) err = o2;
            return err.Length == 0;
        }

        public static bool RemoveRules(string path)
        {
            int code;
            string key = KeyOf(path);
            Run("netsh", "advfirewall firewall delete rule name=\"" + Prefix + "_OUT_" + key + "\"", Encoding.Default, out code);
            Run("netsh", "advfirewall firewall delete rule name=\"" + Prefix + "_IN_" + key + "\"", Encoding.Default, out code);
            return true;
        }

        public static int RemoveAll()
        {
            int code;
            string ps = "$ErrorActionPreference='SilentlyContinue';" +
                        "$r=@(Get-NetFirewallRule | Where-Object { $_.DisplayName -like '" + Prefix + "*' });" +
                        "$r | Remove-NetFirewallRule -ErrorAction SilentlyContinue;" +
                        "$r.Count";
            string outp = Run("powershell", "-NoProfile -NonInteractive -Command \"" + ps + "\"", Encoding.UTF8, out code);
            int n = 0;
            if (outp != null) int.TryParse(outp.Trim(), out n);
            return n;
        }

        public static HashSet<string> RuleNames()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int code;
            string ps = "$ErrorActionPreference='SilentlyContinue';" +
                        "Get-NetFirewallRule | Where-Object { $_.DisplayName -like '" + Prefix + "*' } | ForEach-Object { $_.DisplayName }";
            string outp = Run("powershell", "-NoProfile -NonInteractive -Command \"" + ps + "\"", Encoding.UTF8, out code);
            if (outp != null)
            {
                foreach (string line in outp.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string s = line.Trim();
                    if (s.Length > 0) set.Add(s);
                }
            }
            return set;
        }

        public static void Load(List<ProgItem> items)
        {
            items.Clear();
            try
            {
                if (!File.Exists(ListPath)) return;
                foreach (string raw in File.ReadAllLines(ListPath))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    ProgItem it = new ProgItem();
                    it.Path = line;
                    it.Name = Path.GetFileName(line);
                    it.Exists = File.Exists(line);
                    it.Key = KeyOf(line);
                    it.Icon = IconOf(line);
                    items.Add(it);
                }
            }
            catch { }
        }

        public static void Save(List<ProgItem> items)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# NetBlock 程序列表（一行一个路径）");
                foreach (ProgItem it in items) sb.AppendLine(it.Path);
                File.WriteAllText(ListPath, sb.ToString(), new UTF8Encoding(false));
            }
            catch { }
        }

        private static Dictionary<string, Bitmap> iconCache = new Dictionary<string, Bitmap>();

        public static Bitmap IconOf(string path)
        {
            if (iconCache.ContainsKey(path)) return iconCache[path];
            Bitmap bmp = null;
            try
            {
                if (File.Exists(path))
                {
                    using (Icon ic = Icon.ExtractAssociatedIcon(path))
                    {
                        if (ic != null) bmp = ic.ToBitmap();
                    }
                }
            }
            catch { }
            iconCache[path] = bmp;
            return bmp;
        }

        public static string ResolveTarget(string path)
        {
            try
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".exe") return path;
                if (ext == ".lnk")
                {
                    Type t = Type.GetTypeFromProgID("WScript.Shell");
                    if (t == null) return null;
                    object shell = Activator.CreateInstance(t);
                    object sc = t.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { path });
                    object val = sc.GetType().InvokeMember("TargetPath", System.Reflection.BindingFlags.GetProperty, null, sc, null);
                    string target = val as string;
                    if (!string.IsNullOrEmpty(target)) return target;
                }
            }
            catch { }
            return null;
        }
    }

    // ------------------------------------------------ 主窗体
    public class MainForm : Form
    {
        private const string BlogUrl = "https://gui.ge";
        private const string RepoUrl = "https://github.com/barrybao0222/netblock";
        private const string Version = "v1.0.0";

        private Card cardList;
        private ProgramList list;
        private Btn btnBlock, btnRestore, btnAdd;
        private Label lblCount, lblSummary, lblStatus, lblEmptyHint;
        private NotifyIcon tray;
        private bool busy;

        public MainForm()
        {
            Text = "一键禁止联网工具 · NetBlock";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 664);
            BackColor = Theme.Bg;
            DoubleBuffered = true;
            Font = Theme.F(9f, FontStyle.Regular);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96f, 96f);
            AllowDrop = true;
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            BuildHeader();
            BuildList();
            BuildButtons();
            BuildFooter();
            BuildTray();

            DragEnter += OnDragEnter;
            DragDrop += OnDragDrop;
            KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (busy) return;
                if (e.KeyCode == Keys.Delete && list.Selected >= 0) RemoveAt(list.Selected);
                else if (e.KeyCode == Keys.F5) ApplyAll();
                else if (e.KeyCode == Keys.F9) RestoreAll();
                else if (e.KeyCode == Keys.Escape) Close();
            };

            Load += delegate
            {
                Engine.Load(list.Items);
                if (Startup.Preload.Count > 0) AddPaths(Startup.Preload.ToArray());
                list.RefreshScroll();
                RefreshCounts();
                RefreshStates(false);
                SetStatus(list.Items.Count == 0 ? "把程序拖进来，或点右下角「添加程序」" : "就绪", Theme.Sub);
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }

        // ---------------- 顶栏 ----------------
        private void BuildHeader()
        {
            Panel head = new Panel();
            head.SetBounds(0, 0, ClientSize.Width, 72);
            head.BackColor = Color.White;
            head.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Theme.Border, 1f))
                    g.DrawLine(pen, 0, head.Height - 1, head.Width, head.Height - 1);

                Rectangle logo = new Rectangle(20, 16, 40, 40);
                using (GraphicsPath p = Gfx.Round(logo, 11))
                using (LinearGradientBrush br = new LinearGradientBrush(logo, Theme.Primary, Color.FromArgb(59, 130, 246), 45f))
                    g.FillPath(br, p);
                Gfx.DrawNoNet(g, new RectangleF(28f, 24f, 24f, 24f), Color.White, Color.FromArgb(255, 190, 190));

                using (Font f1 = Theme.F(15f, FontStyle.Bold))
                using (Font f2 = Theme.F(8.5f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "一键禁止联网工具", f1, new Point(72, 12), Theme.Text);
                    TextRenderer.DrawText(g, "NetBlock · 把程序拖进来，一键断开它的外网访问", f2, new Point(74, 42), Theme.Sub);
                }
            };

            Label blog = new Label();
            blog.Text = "博客 gui.ge";
            blog.Font = Theme.F(9f, FontStyle.Regular);
            blog.ForeColor = Theme.Primary;
            blog.Cursor = Cursors.Hand;
            blog.AutoSize = false;
            blog.SetBounds(ClientSize.Width - 300, 26, 92, 22);
            blog.TextAlign = ContentAlignment.MiddleRight;
            blog.Click += delegate { OpenUrl(BlogUrl); };
            head.Controls.Add(blog);

            Label repo = new Label();
            repo.Text = "GitHub";
            repo.Font = Theme.F(9f, FontStyle.Regular);
            repo.ForeColor = Theme.Sub;
            repo.Cursor = Cursors.Hand;
            repo.AutoSize = false;
            repo.SetBounds(ClientSize.Width - 200, 26, 62, 22);
            repo.TextAlign = ContentAlignment.MiddleRight;
            repo.Click += delegate { OpenUrl(RepoUrl); };
            head.Controls.Add(repo);

            Glyph min = new Glyph();
            min.SetBounds(ClientSize.Width - 122, 20, 32, 32);
            min.Click += delegate { HideToTray(); };
            head.Controls.Add(min);

            Glyph close = new Glyph();
            close.IsClose = true;
            close.SetBounds(ClientSize.Width - 82, 20, 32, 32);
            close.Click += delegate { if (!busy) Close(); };
            head.Controls.Add(close);

            head.MouseDown += delegate(object s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Native.ReleaseCapture();
                    Native.SendMessage(Handle, 0xA1, (IntPtr)2, IntPtr.Zero);
                }
            };
            Controls.Add(head);
        }

        private void OpenUrl(string url)
        {
            try { Process.Start(url); } catch { }
        }

        // ---------------- 列表卡片 ----------------
        private void BuildList()
        {
            cardList = new Card();
            cardList.SetBounds(16, 84, 968, 424);
            Controls.Add(cardList);

            cardList.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Font f1 = Theme.F(10.5f, FontStyle.Bold))
                    TextRenderer.DrawText(g, "程序列表", f1, new Point(20, 14), Theme.Text);
                // 数量胶囊
                string cnt = lblCount.Text;
                using (Font f2 = Theme.F(8f, FontStyle.Bold))
                {
                    Size sz = TextRenderer.MeasureText(cnt, f2);
                    Rectangle pill = new Rectangle(96, 16, sz.Width + 18, 20);
                    Gfx.FillRound(g, pill, 10, Color.FromArgb(238, 244, 253));
                    TextRenderer.DrawText(g, cnt, f2, pill, Theme.Primary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                using (Pen pen = new Pen(Theme.Border, 1f))
                    g.DrawLine(pen, 20, 44, cardList.Width - 20, 44);
            };

            lblCount = new Label();
            lblCount.Text = "0";
            lblCount.AutoSize = false;
            lblCount.SetBounds(0, 0, 1, 1);
            cardList.Controls.Add(lblCount);

            list = new ProgramList();
            list.SetBounds(12, 48, 944, 336);
            list.ToggleRequested += delegate(int i) { ToggleItem(i); };
            list.RemoveRequested += delegate(int i) { RemoveAt(i); };
            list.SelectionChanged += delegate { };
            cardList.Controls.Add(list);

            lblEmptyHint = new Label();
            lblEmptyHint.Text = "提示：拖入桌面快捷方式也可以；同一程序重复拖入会自动去重。";
            lblEmptyHint.Font = Theme.F(8f, FontStyle.Regular);
            lblEmptyHint.ForeColor = Theme.SubLight;
            lblEmptyHint.BackColor = Theme.Card;
            lblEmptyHint.AutoSize = false;
            lblEmptyHint.SetBounds(20, 392, 700, 20);
            cardList.Controls.Add(lblEmptyHint);
        }

        // ---------------- 按钮 ----------------
        private void BuildButtons()
        {
            btnBlock = new Btn();
            btnBlock.Text = "一键禁止联网";
            btnBlock.Font = Theme.F(12f, FontStyle.Bold);
            btnBlock.Fill1 = Theme.Danger;
            btnBlock.Fill2 = Theme.DangerDark;
            btnBlock.SetBounds(16, 524, 250, 56);
            btnBlock.Click += delegate { ApplyAll(); };
            Controls.Add(btnBlock);

            btnRestore = new Btn();
            btnRestore.Text = "全部恢复联网";
            btnRestore.Fill1 = Color.FromArgb(120, 134, 150);
            btnRestore.Fill2 = Color.FromArgb(96, 110, 126);
            btnRestore.SetBounds(276, 524, 190, 56);
            btnRestore.Click += delegate { RestoreAll(); };
            Controls.Add(btnRestore);

            btnAdd = new Btn();
            btnAdd.Text = "＋ 添加程序";
            btnAdd.Outline = true;
            btnAdd.SetBounds(476, 524, 160, 56);
            btnAdd.Click += delegate { AddByDialog(); };
            Controls.Add(btnAdd);

            lblSummary = new Label();
            lblSummary.Text = "已禁止 0 / 共 0";
            lblSummary.Font = Theme.F(10f, FontStyle.Bold);
            lblSummary.ForeColor = Theme.Sub;
            lblSummary.AutoSize = false;
            lblSummary.SetBounds(652, 524, 332, 56);
            lblSummary.TextAlign = ContentAlignment.MiddleRight;
            Controls.Add(lblSummary);
        }

        // ---------------- 底栏 ----------------
        private void BuildFooter()
        {
            Panel foot = new Panel();
            foot.SetBounds(0, 596, ClientSize.Width, 68);
            foot.BackColor = Theme.Bg;
            foot.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using (Pen pen = new Pen(Theme.Border, 1f))
                    g.DrawLine(pen, 16, 2, foot.Width - 16, 2);

                using (Font f1 = Theme.F(8.5f, FontStyle.Bold))
                using (Font f2 = Theme.F(8.5f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "作者：鬼哥Coding", f1, new Point(18, 16), Theme.Text);
                    TextRenderer.DrawText(g, "博客", f2, new Point(140, 16), Theme.Sub);
                    TextRenderer.DrawText(g, "gui.ge", f2, new Point(172, 16), Theme.Primary);
                    TextRenderer.DrawText(g, "· MIT License · " + Version + " · 本地防火墙实现，不上传任何数据", f2,
                        new Point(232, 16), Theme.Sub);
                }

                // 状态
                using (SolidBrush b = new SolidBrush(Theme.Success))
                    g.FillEllipse(b, new Rectangle(18, 40, 8, 8));
            };
            Controls.Add(foot);

            Label blog2 = new Label();
            blog2.Text = "gui.ge";
            blog2.Font = Theme.F(8.5f, FontStyle.Underline);
            blog2.ForeColor = Theme.Primary;
            blog2.BackColor = Theme.Bg;
            blog2.Cursor = Cursors.Hand;
            blog2.AutoSize = false;
            blog2.SetBounds(170, 608, 56, 20);
            blog2.TextAlign = ContentAlignment.MiddleLeft;
            blog2.Click += delegate { OpenUrl(BlogUrl); };
            Controls.Add(blog2);
            blog2.BringToFront();

            lblStatus = new Label();
            lblStatus.Text = "就绪";
            lblStatus.Font = Theme.F(8.5f, FontStyle.Regular);
            lblStatus.ForeColor = Theme.Sub;
            lblStatus.BackColor = Theme.Bg;
            lblStatus.AutoSize = false;
            lblStatus.SetBounds(34, 634, 720, 20);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(lblStatus);
            lblStatus.BringToFront();
        }

        private void BuildTray()
        {
            tray = new NotifyIcon();
            try { tray.Icon = Icon; } catch { tray.Icon = SystemIcons.Application; }
            tray.Text = "一键禁止联网工具 · NetBlock";
            tray.Visible = true;
            tray.DoubleClick += delegate { ShowFromTray(); };

            ContextMenuStrip m = new ContextMenuStrip();
            m.Items.Add("显示主界面", null, delegate { ShowFromTray(); });
            m.Items.Add("一键禁止联网", null, delegate { ApplyAll(); });
            m.Items.Add("全部恢复联网", null, delegate { RestoreAll(); });
            m.Items.Add(new ToolStripSeparator());
            m.Items.Add("退出", null, delegate { CloseApp(); });
            tray.ContextMenuStrip = m;
        }

        private void HideToTray()
        {
            Hide();
        }
        private void ShowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }
        private void CloseApp()
        {
            if (tray != null) { tray.Visible = false; tray.Dispose(); tray = null; }
            Application.Exit();
        }

        // ---------------- 拖放 ----------------
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (busy) { e.Effect = DragDropEffects.None; return; }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                SetStatus("松手即可加入列表", Theme.Primary);
            }
            else e.Effect = DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            if (busy) return;
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null) AddPaths(files);
            }
            catch (Exception ex) { SetStatus("拖放失败：" + ex.Message, Theme.Danger); }
        }

        private void AddByDialog()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择要禁止联网的程序";
            dlg.Filter = "程序 (*.exe)|*.exe|快捷方式 (*.lnk)|*.lnk|所有文件 (*.*)|*.*";
            dlg.Multiselect = true;
            if (dlg.ShowDialog(this) == DialogResult.OK) AddPaths(dlg.FileNames);
        }

        private void AddPaths(string[] paths)
        {
            int added = 0, dup = 0, fail = 0;
            List<string> newOnes = new List<string>();

            foreach (string p in paths)
            {
                string target = null;
                if (Directory.Exists(p))
                {
                    string[] exes = Directory.GetFiles(p, "*.exe");
                    if (exes.Length == 0) { fail++; continue; }
                    foreach (string ex in exes)
                        if (AddOne(ex)) { added++; newOnes.Add(Path.GetFileName(ex)); }
                    continue;
                }
                if (!File.Exists(p)) { fail++; continue; }
                target = Engine.ResolveTarget(p);
                if (string.IsNullOrEmpty(target) || !File.Exists(target)) { fail++; continue; }
                if (Find(target) >= 0) { dup++; continue; }
                if (AddOne(target)) { added++; newOnes.Add(Path.GetFileName(target)); }
            }

            list.RefreshScroll();
            RefreshCounts();
            Engine.Save(list.Items);
            if (added > 0) RefreshStates(false);

            if (added > 0)
                SetStatus("已添加 " + added + " 个程序：" + JoinNames(newOnes), Theme.Success);
            else if (dup > 0)
                SetStatus("这些程序已经在列表里了", Theme.Sub);
            else
                SetStatus("没有可以添加的可执行程序", Theme.Danger);
        }

        private static string JoinNames(List<string> names)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < names.Count && i < 4; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(names[i]);
            }
            if (names.Count > 4) sb.Append(" 等 " + names.Count + " 个");
            return sb.ToString();
        }

        private bool AddOne(string exe)
        {
            if (Find(exe) >= 0) return false;
            ProgItem it = new ProgItem();
            it.Path = exe;
            it.Name = Path.GetFileName(exe);
            it.Exists = File.Exists(exe);
            it.Key = Engine.KeyOf(exe);
            it.Icon = Engine.IconOf(exe);
            list.Items.Add(it);
            return true;
        }

        private int Find(string path)
        {
            for (int i = 0; i < list.Items.Count; i++)
                if (string.Equals(list.Items[i].Path, path, StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }

        private void RemoveAt(int i)
        {
            if (i < 0 || i >= list.Items.Count) return;
            string name = list.Items[i].Name;
            try { Engine.RemoveRules(list.Items[i].Path); } catch { }
            list.Items.RemoveAt(i);
            if (list.Selected >= list.Items.Count) list.Selected = list.Items.Count - 1;
            list.RefreshScroll();
            RefreshCounts();
            Engine.Save(list.Items);
            RefreshStates(false);
            SetStatus("已移除：" + name, Theme.Sub);
        }

        // ---------------- 状态 ----------------
        private void RefreshCounts()
        {
            int total = list.Items.Count;
            int blocked = 0;
            foreach (ProgItem it in list.Items) if (it.Blocked) blocked++;
            lblCount.Text = total.ToString();
            lblSummary.Text = "已禁止 " + blocked + " / 共 " + total;
            lblSummary.ForeColor = blocked > 0 ? Theme.Danger : Theme.Sub;
            cardList.Invalidate();
        }

        private void RefreshStates(bool announce)
        {
            Thread t = new Thread(delegate()
            {
                HashSet<string> names = Engine.RuleNames();
                BeginInvoke((MethodInvoker)delegate
                {
                    foreach (ProgItem it in list.Items)
                    {
                        string k = it.Key.Length > 0 ? it.Key : Engine.KeyOf(it.Path);
                        it.Key = k;
                        it.Blocked = names.Contains(Engine.Prefix + "_OUT_" + k) || names.Contains(Engine.Prefix + "_IN_" + k);
                        it.Exists = File.Exists(it.Path);
                    }
                    list.Invalidate();
                    RefreshCounts();
                    if (announce) SetStatus("状态已刷新", Theme.Sub);
                });
            });
            t.IsBackground = true;
            t.Start();
        }

        private void SetStatus(string s, Color c)
        {
            if (lblStatus.InvokeRequired) { lblStatus.BeginInvoke((MethodInvoker)delegate { SetStatus(s, c); }); return; }
            lblStatus.Text = s;
            lblStatus.ForeColor = c;
        }

        private void SetBusy(bool v)
        {
            busy = v;
            btnBlock.Enabled = !v;
            btnRestore.Enabled = !v;
            btnAdd.Enabled = !v;
            Cursor = v ? Cursors.AppStarting : Cursors.Default;
        }

        // ---------------- 操作 ----------------
        private void ToggleItem(int idx)
        {
            if (busy || idx < 0 || idx >= list.Items.Count) return;
            ProgItem it = list.Items[idx];
            bool want = !it.Blocked;
            SetBusy(true);
            SetStatus((want ? "正在禁止：" : "正在恢复：") + it.Name, Theme.Primary);
            Thread t = new Thread(delegate()
            {
                string err;
                bool ok;
                if (want) ok = Engine.AddRules(it.Path, out err);
                else { Engine.RemoveRules(it.Path); ok = true; err = ""; }

                HashSet<string> names = Engine.RuleNames();
                BeginInvoke((MethodInvoker)delegate
                {
                    if (!ok)
                    {
                        SetStatus("操作失败：" + err.Replace("\r", " ").Replace("\n", " "), Theme.Danger);
                    }
                    else
                    {
                        SetStatus((want ? "已禁止联网：" : "已恢复联网：") + it.Name, want ? Theme.Danger : Theme.Success);
                    }
                    it.Blocked = names.Contains(Engine.Prefix + "_OUT_" + it.Key) || names.Contains(Engine.Prefix + "_IN_" + it.Key);
                    list.Invalidate();
                    RefreshCounts();
                    SetBusy(false);
                });
            });
            t.IsBackground = true;
            t.Start();
        }

        private void ApplyAll()
        {
            if (busy) return;
            if (list.Items.Count == 0) { SetStatus("列表是空的，先把程序拖进来", Theme.Danger); return; }
            SetBusy(true);
            SetStatus("正在添加防火墙规则 …", Theme.Primary);
            Thread t = new Thread(delegate()
            {
                int ok = 0, bad = 0;
                string firstErr = "";
                foreach (ProgItem it in list.Items)
                {
                    if (it.Blocked) { ok++; continue; }
                    string err;
                    if (Engine.AddRules(it.Path, out err)) ok++;
                    else { bad++; if (firstErr.Length == 0) firstErr = err; }
                }
                HashSet<string> names = Engine.RuleNames();
                BeginInvoke((MethodInvoker)delegate
                {
                    foreach (ProgItem it in list.Items)
                        it.Blocked = names.Contains(Engine.Prefix + "_OUT_" + it.Key) || names.Contains(Engine.Prefix + "_IN_" + it.Key);
                    list.Invalidate();
                    RefreshCounts();
                    SetBusy(false);
                    if (bad == 0)
                        SetStatus("已禁止 " + ok + " 个程序联网（出站 + 入站）", Theme.Danger);
                    else
                        SetStatus("完成 " + ok + " 个，失败 " + bad + " 个：" + firstErr.Replace("\r", " ").Replace("\n", " "), Theme.Warn);
                });
            });
            t.IsBackground = true;
            t.Start();
        }

        private void RestoreAll()
        {
            if (busy) return;
            if (MessageBox.Show("将删除 NetBlock 添加的全部防火墙规则，恢复这些程序的网络访问。\r\n\r\n继续？",
                    "全部恢复联网", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;

            SetBusy(true);
            SetStatus("正在删除规则 …", Theme.Primary);
            Thread t = new Thread(delegate()
            {
                int n = Engine.RemoveAll();
                BeginInvoke((MethodInvoker)delegate
                {
                    foreach (ProgItem it in list.Items) it.Blocked = false;
                    list.Invalidate();
                    RefreshCounts();
                    SetBusy(false);
                    SetStatus("已删除 " + n + " 条规则，网络访问已恢复", Theme.Success);
                });
            });
            t.IsBackground = true;
            t.Start();
        }

        // ---------------- 关闭 ----------------
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (tray != null) { tray.Visible = false; tray.Dispose(); tray = null; }
            base.OnFormClosing(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Color.FromArgb(214, 222, 232), 1f))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                int pref = 2;
                Native.DwmSetWindowAttribute(Handle, 33, ref pref, 4);
            }
            catch { }
        }
    }

    internal static class Native
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    }

    // ------------------------------------------------ 自检
    internal static class SelfTest
    {
        public static void Run(string outPath)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                string dir = Path.Combine(Path.GetTempPath(), "netblock-selftest");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                string me = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string a = Path.Combine(dir, "AppA.exe");
                string b = Path.Combine(dir, "AppB.exe");
                File.Copy(me, a, true);
                File.Copy(me, b, true);

                string err;
                bool ok1 = Engine.AddRules(a, out err);
                bool ok2 = Engine.AddRules(b, out err);
                sb.AppendLine("ADD_A=" + ok1 + " ADD_B=" + ok2);
                HashSet<string> names = Engine.RuleNames();
                sb.AppendLine("RULES=" + names.Count);
                sb.AppendLine("HAS_A=" + names.Contains("NetBlock_OUT_" + Engine.KeyOf(a)));
                Engine.RemoveRules(a);
                names = Engine.RuleNames();
                sb.AppendLine("AFTER_REMOVE_A=" + names.Count);
                int n = Engine.RemoveAll();
                names = Engine.RuleNames();
                sb.AppendLine("AFTER_REMOVE_ALL=" + names.Count + " removed=" + n);
                sb.AppendLine("RESULT=OK");
            }
            catch (Exception ex) { sb.AppendLine("RESULT=FAIL " + ex.Message); }
            try { File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(true)); } catch { }
        }
    }

    internal static class Startup
    {
        public static List<string> Preload = new List<string>();
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length >= 2 && args[0] == "--selftest")
            {
                SelfTest.Run(args[1]);
                return;
            }
            for (int i = 0; i + 1 < args.Length; i++)
                if (args[i] == "--add") Startup.Preload.Add(args[i + 1]);
            bool admin = false;
            try
            {
                System.Security.Principal.WindowsPrincipal p = new System.Security.Principal.WindowsPrincipal(
                    System.Security.Principal.WindowsIdentity.GetCurrent());
                admin = p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch { }
            if (!admin)
            {
                try
                {
                    StringBuilder q = new StringBuilder();
                    foreach (string a in args) q.Append('"').Append(a.Replace("\"", "\\\"")).Append("\" ");
                    ProcessStartInfo psi = new ProcessStartInfo(Application.ExecutablePath, q.ToString().Trim());
                    psi.UseShellExecute = true;
                    psi.Verb = "runas";
                    Process.Start(psi);
                    return;
                }
                catch { }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
