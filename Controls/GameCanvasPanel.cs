using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public class GameCanvasPanel : DoubleBufferedPanel
    {
        private GameState state;
        private GameResourceManager resources;

        public void Bind(GameState gameState, GameResourceManager gameResources)
        {
            state = gameState;
            resources = gameResources;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.FromArgb(8, 10, 12));

            if (state == null || state.Tanks.Count == 0)
            {
                DrawCenteredText(g, "等待游戏状态", ClientRectangle, Brushes.Silver, 18);
                return;
            }

            float mapScale = GetMapScale();
            Rectangle mapRect = GetMapRectangle(mapScale);
            SolidBrush mapBrush = new SolidBrush(Color.FromArgb(20, 23, 26));
            g.FillRectangle(mapBrush, mapRect);
            mapBrush.Dispose();
            g.DrawRectangle(Pens.DimGray, mapRect);

            g.TranslateTransform(mapRect.X, mapRect.Y);
            g.ScaleTransform(mapScale, mapScale);
            DrawBlocks(g, false);
            DrawTanks(g);
            DrawBullets(g);
            DrawBlocks(g, true);
            DrawPowerUps(g);
            g.ResetTransform();

            if (state.RoundOver || state.MatchOver)
            {
                DrawResultOverlay(g);
            }
        }

        private void DrawBlocks(Graphics g, bool drawGrass)
        {
            foreach (MapBlock block in state.Blocks)
            {
                if (!block.Visible)
                {
                    continue;
                }

                if (drawGrass && block.Type != BlockType.Grass)
                {
                    continue;
                }

                if (!drawGrass && block.Type == BlockType.Grass)
                {
                    continue;
                }

                string key = "";

                if (block.Type == BlockType.Brick)
                {
                    key = "brick";
                }
                else if (block.Type == BlockType.Steel)
                {
                    key = "steel";
                }
                else if (block.Type == BlockType.Grass)
                {
                    key = "grass";
                }
                else if (block.Type == BlockType.Water)
                {
                    key = "water";
                }

                Image image = resources == null ? null : resources.GetImage(key);

                if (image != null)
                {
                    DrawPixelImage(g, image, block.Bounds);
                }
                else
                {
                    DrawFallbackBlock(g, block);
                }
            }
        }

        private void DrawFallbackBlock(Graphics g, MapBlock block)
        {
            Color color = Color.DarkGray;

            if (block.Type == BlockType.Brick)
            {
                color = Color.FromArgb(140, 82, 50);
            }
            else if (block.Type == BlockType.Steel)
            {
                color = Color.FromArgb(130, 138, 144);
            }
            else if (block.Type == BlockType.Grass)
            {
                color = Color.FromArgb(68, 120, 70);
            }
            else if (block.Type == BlockType.Water)
            {
                color = Color.FromArgb(45, 92, 128);
            }

            SolidBrush brush = new SolidBrush(color);
            g.FillRectangle(brush, block.Bounds);
            brush.Dispose();
        }

        private void DrawTanks(Graphics g)
        {
            foreach (Tank tank in state.Tanks)
            {
                if (tank.Hp <= 0)
                {
                    continue;
                }

                if (!tank.Alive)
                {
                    DrawBornEffect(g, tank);
                    continue;
                }

                Image image = resources == null
                    ? null
                    : resources.GetTankImage(tank.PlayerId, tank.Direction, tank.UpgradeLevel, tank.TankColorArgb);

                if (image != null)
                {
                    DrawPixelImage(g, image, tank.DrawBounds);
                }
                else
                {
                    SolidBrush brush = new SolidBrush(Color.FromArgb(tank.TankColorArgb));
                    g.FillRectangle(brush, tank.DrawBounds);
                    brush.Dispose();
                }

                DrawTankName(g, tank);
            }
        }

        private void DrawTankName(Graphics g, Tank tank)
        {
            string text = "P" + tank.PlayerId.ToString();
            int top = Math.Max(0, tank.DrawBounds.Y - 18);
            Rectangle rect = new Rectangle(tank.DrawBounds.X, top, tank.DrawBounds.Width, 16);
            Rectangle shadowRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width, rect.Height);
            SolidBrush shadowBrush = new SolidBrush(Color.Black);
            SolidBrush brush = new SolidBrush(Color.FromArgb(tank.TankColorArgb));
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString(text, Font, shadowBrush, shadowRect, format);
            g.DrawString(text, Font, brush, rect, format);
            format.Dispose();
            shadowBrush.Dispose();
            brush.Dispose();
        }

        private void DrawBullets(Graphics g)
        {
            foreach (Bullet bullet in state.Bullets)
            {
                if (!bullet.Alive)
                {
                    continue;
                }

                Image image = resources == null ? null : resources.GetImage("bullet");

                if (image != null)
                {
                    DrawPixelImage(g, image, bullet.Bounds);
                }
                else
                {
                    g.FillEllipse(Brushes.Gold, bullet.Bounds);
                }
            }
        }

        private void DrawPowerUps(Graphics g)
        {
            foreach (PowerUp powerUp in state.PowerUps)
            {
                if (!powerUp.Visible)
                {
                    continue;
                }

                Image image = resources == null ? null : resources.GetImage("star");

                if (image != null)
                {
                    DrawPixelImage(g, image, powerUp.Bounds);
                }
                else
                {
                    g.FillEllipse(Brushes.Gold, powerUp.Bounds);
                }
            }
        }

        private void DrawResultOverlay(Graphics g)
        {
            int rectWidth = Math.Max(280, Math.Min(560, Width - 64));
            int rectHeight = Math.Max(120, Math.Min(160, Height - 64));
            Rectangle rect = new Rectangle((Width - rectWidth) / 2, (Height - rectHeight) / 2, rectWidth, rectHeight);
            SolidBrush bg = new SolidBrush(Color.FromArgb(210, 10, 12, 16));
            g.FillRectangle(bg, rect);
            bg.Dispose();
            g.DrawRectangle(Pens.Gold, rect);

            string title = state.MatchOver ? "GAME OVER" : "ROUND OVER";
            Rectangle titleRect = new Rectangle(rect.X + 18, rect.Y + 20, rect.Width - 36, 40);
            Rectangle textRect = new Rectangle(rect.X + 18, rect.Y + 66, rect.Width - 36, rect.Height - 84);
            DrawCenteredText(g, title, titleRect, Brushes.Gold, 18);
            DrawCenteredText(g, state.ResultText, textRect, Brushes.White, 13);
        }

        private void DrawBornEffect(Graphics g, Tank tank)
        {
            int frame = Math.Max(1, Math.Min(4, 4 - tank.RespawnTicks / 8));
            Image image = resources == null ? null : resources.GetImage("born" + frame.ToString());

            if (image != null)
            {
                DrawPixelImage(g, image, tank.DrawBounds);
                return;
            }

            SolidBrush respawnBrush = new SolidBrush(Color.FromArgb(90, Color.Gold));
            g.FillEllipse(respawnBrush, tank.DrawBounds);
            respawnBrush.Dispose();
        }

        private void DrawPixelImage(Graphics g, Image image, Rectangle target)
        {
            InterpolationMode oldInterpolation = g.InterpolationMode;
            PixelOffsetMode oldPixelOffset = g.PixelOffsetMode;
            SmoothingMode oldSmoothing = g.SmoothingMode;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            g.DrawImage(image, target);

            g.InterpolationMode = oldInterpolation;
            g.PixelOffsetMode = oldPixelOffset;
            g.SmoothingMode = oldSmoothing;
        }

        private void DrawCenteredText(Graphics g, string text, Rectangle rect, Brush brush, int fontSize)
        {
            Font font = new Font("微软雅黑", fontSize, FontStyle.Bold);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.Trimming = StringTrimming.EllipsisWord;
            format.FormatFlags = StringFormatFlags.LineLimit;
            g.DrawString(text, font, brush, rect, format);
            format.Dispose();
            font.Dispose();
        }

        private float GetMapScale()
        {
            if (state == null || state.MapWidth <= 0 || state.MapHeight <= 0 || Width <= 0 || Height <= 0)
            {
                return 1F;
            }

            float scaleX = Width / (float)state.MapWidth;
            float scaleY = Height / (float)state.MapHeight;
            return Math.Min(1F, Math.Min(scaleX, scaleY));
        }

        private Rectangle GetMapRectangle(float scale)
        {
            int width = Math.Max(1, (int)Math.Floor(state.MapWidth * scale));
            int height = Math.Max(1, (int)Math.Floor(state.MapHeight * scale));
            int x = Math.Max(0, (Width - width) / 2);
            int y = Math.Max(0, (Height - height) / 2);
            return new Rectangle(x, y, width, height);
        }
    }
}
