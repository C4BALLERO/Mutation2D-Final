using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Sprites pixel detallados para enemigos. El tinte de mutación aplica sobre tonos base claros.
    /// </summary>
    public static class EnemySpriteFactory
    {
        public enum Archetype
        {
            Drone,
            Boss,
            Queen,
            Mimic,
            Parasite
        }

        private static readonly Color Hi = Color.white;
        private static readonly Color Mid = new(0.9f, 0.9f, 0.92f, 1f);
        private static readonly Color Shad = new(0.62f, 0.62f, 0.68f, 1f);
        private static readonly Color Outline = new(0.06f, 0.06f, 0.08f, 1f);
        private static readonly Color Red = new(0.95f, 0.22f, 0.22f, 1f);
        private static readonly Color Purple = new(0.62f, 0.28f, 0.92f, 1f);
        private static readonly Color Cyan = new(0.35f, 0.78f, 0.98f, 1f);
        private static readonly Color Green = new(0.45f, 0.92f, 0.38f, 1f);
        private static readonly Color DarkRed = new(0.55f, 0.1f, 0.12f, 1f);

        public static Sprite Create(Archetype archetype, int size = 64, float ppu = 64f)
        {
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] px = new Color[size * size];
            for (int i = 0; i < px.Length; i++)
                px[i] = Color.clear;

            switch (archetype)
            {
                case Archetype.Drone: DrawDrone(px, size); break;
                case Archetype.Boss: DrawBoss(px, size); break;
                case Archetype.Queen: DrawQueen(px, size); break;
                case Archetype.Mimic: DrawMimic(px, size); break;
                case Archetype.Parasite: DrawParasite(px, size); break;
            }

            tex.SetPixels(px);
            tex.Apply();
            float pivotY = archetype == Archetype.Parasite ? 0.4f : 0.36f;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, pivotY), ppu);
        }

        public static Color GetDefaultTint(Archetype archetype)
        {
            return archetype switch
            {
                Archetype.Drone => new Color(1f, 0.35f, 0.35f),
                Archetype.Boss => new Color(0.85f, 0.22f, 0.22f),
                Archetype.Queen => new Color(0.75f, 0.28f, 0.95f),
                Archetype.Mimic => new Color(0.35f, 0.72f, 0.95f),
                Archetype.Parasite => new Color(0.55f, 0.9f, 0.35f),
                _ => new Color(0.9f, 0.3f, 0.3f)
            };
        }

        private static void DrawDrone(Color[] px, int s)
        {
            float cx = s * 0.5f;
            float cy = s * 0.44f;
            float rx = s * 0.26f;
            float ry = s * 0.22f;
            FillEllipse(px, s, cx, cy, rx, ry, Mid);
            FillEllipse(px, s, cx - rx * 0.25f, cy + ry * 0.15f, rx * 0.85f, ry * 0.75f, Shad);
            FillEllipse(px, s, cx + rx * 0.2f, cy - ry * 0.1f, rx * 0.7f, ry * 0.55f, Hi);

            for (int side = -1; side <= 1; side += 2)
            {
                float wx = cx + side * rx * 1.05f;
                FillTriangle(px, s, wx, cy + ry * 0.3f, wx + side * rx * 0.35f, cy - ry * 0.2f,
                    wx + side * rx * 0.15f, cy + ry * 0.9f, Mid);
                StrokeLine(px, s, wx, cy - ry * 0.2f, wx + side * rx * 0.15f, cy + ry * 0.9f, Outline);
            }

            StrokeEllipse(px, s, cx, cy, rx + 1f, ry + 1f, Outline, 2);

            for (int i = -1; i <= 1; i++)
                FillDisk(px, s, cx + i * rx * 0.35f, cy - ry * 0.15f, s * 0.045f, i == 0 ? Red : DarkRed);

            DrawLine(px, s, cx - rx * 0.5f, cy + ry * 0.55f, cx - rx * 0.15f, cy + ry * 0.75f, Outline);
            DrawLine(px, s, cx + rx * 0.5f, cy + ry * 0.55f, cx + rx * 0.15f, cy + ry * 0.75f, Outline);
            FillDisk(px, s, cx, cy + ry * 0.82f, s * 0.04f, Red);

            for (int leg = -1; leg <= 1; leg += 2)
            {
                float lx = cx + leg * rx * 0.55f;
                DrawLine(px, s, lx, cy + ry, lx + leg * 2, cy + ry + s * 0.14f, Outline);
                DrawLine(px, s, lx + leg * 2, cy + ry + s * 0.14f, lx + leg * 3, cy + ry + s * 0.2f, Shad);
            }

            DrawLine(px, s, cx, cy + ry * 1.05f, cx, cy + ry * 1.35f, Outline);
            FillDisk(px, s, cx, cy + ry * 1.38f, s * 0.035f, Purple);
            FillDisk(px, s, cx, cy + ry * 1.42f, s * 0.02f, Hi);
        }

        private static void DrawBoss(Color[] px, int s)
        {
            int m = s / 10;
            int w = s - m * 2;
            int h = (int)(w * 0.95f);
            int x0 = m;
            int y0 = s / 9;
            FillRoundedRect(px, s, x0, y0, w, h, s / 8, Mid);
            FillRect(px, s, x0 + w / 5, y0 + h / 4, w * 3 / 5, h / 2, Hi);
            FillRect(px, s, x0 + 2, y0 + 2, w / 4, h / 3, Shad);

            for (int i = 0; i < 2; i++)
            {
                int padX = x0 + (i == 0 ? 0 : w - s / 7);
                FillRoundedRect(px, s, padX, y0 + h / 6, s / 7, s / 5, 3, Shad);
                FillDisk(px, s, padX + s / 14, y0 + h / 6 + 2, 3, DarkRed);
            }

            int hornY = y0 - 1;
            for (int i = -1; i <= 1; i++)
            {
                int hx = x0 + w / 2 + i * (w / 5);
                FillTriangle(px, s, hx, hornY - s / 10, hx - 3, hornY + 2, hx + 3, hornY + 2, Shad);
                StrokeTriangle(px, s, hx, hornY - s / 10, hx - 3, hornY + 2, hx + 3, hornY + 2, Outline);
            }

            StrokeRoundedRect(px, s, x0, y0, w, h, s / 8, Outline, 2);

            int eyeY = y0 + h / 3;
            for (int i = 0; i < 5; i++)
            {
                int ex = x0 + w / 6 + i * (w / 6);
                FillDisk(px, s, ex, eyeY, i == 2 ? s * 0.05f : s * 0.038f, Red);
                FillDisk(px, s, ex - 1, eyeY - 1, s * 0.015f, Hi);
            }

            FillRect(px, s, x0 + w / 4, y0 + h * 2 / 3, w / 2, 2, DarkRed);
            for (int i = 0; i < 4; i++)
                Set(px, s, x0 + w / 4 + i * (w / 12), y0 + h * 2 / 3 + 3, Outline);

            FillRect(px, s, x0 + 4, y0 + h - 6, w - 8, 3, Shad);
            for (int i = 0; i < 3; i++)
            {
                int sx = x0 + w / 4 + i * (w / 4);
                DrawLine(px, s, sx, y0 + 4, sx, y0 + h / 5, Outline);
            }
        }

        private static void DrawQueen(Color[] px, int s)
        {
            float cx = s * 0.5f;
            float cy = s * 0.42f;
            float r = s * 0.3f;
            FillDisk(px, s, cx, cy, r, Mid);
            FillDisk(px, s, cx - r * 0.3f, cy - r * 0.1f, r * 0.75f, Shad);
            FillDisk(px, s, cx + r * 0.25f, cy + r * 0.1f, r * 0.55f, Hi);

            for (int side = -1; side <= 1; side += 2)
            {
                float bx = cx + side * r * 0.95f;
                FillEllipse(px, s, bx, cy, r * 0.35f, r * 0.55f, new Color(0.78f, 0.7f, 0.95f, 1f));
                DrawLine(px, s, bx, cy - r * 0.4f, bx + side * r * 0.2f, cy, Purple);
                DrawLine(px, s, bx, cy, bx + side * r * 0.15f, cy + r * 0.35f, Purple);
            }

            StrokeDisk(px, s, cx, cy, r + 1.5f, Outline, 2);

            for (int i = -3; i <= 3; i++)
            {
                float spread = i / 3f;
                float tipX = cx + spread * r * 1.1f;
                float tipY = cy - r * 1.15f - Mathf.Abs(i) * 2;
                float baseY = cy - r * 0.45f;
                Color spike = i == 0 ? Purple : new Color(0.5f, 0.22f, 0.75f, 1f);
                FillTriangle(px, s, tipX, tipY, tipX - r * 0.14f, baseY, tipX + r * 0.14f, baseY, spike);
                StrokeTriangle(px, s, tipX, tipY, tipX - r * 0.14f, baseY, tipX + r * 0.14f, baseY, Outline);
                if (i % 2 == 0)
                    FillDisk(px, s, tipX, tipY + 2, 2, Hi);
            }

            FillDisk(px, s, cx, cy, r * 0.22f, Purple);
            FillDisk(px, s, cx, cy, r * 0.12f, new Color(0.85f, 0.6f, 1f, 1f));
            for (int i = 0; i < 6; i++)
            {
                float ang = i / 6f * Mathf.PI * 2f;
                DrawLine(px, s, cx, cy, cx + Mathf.Cos(ang) * r * 0.5f, cy + Mathf.Sin(ang) * r * 0.5f, Purple);
            }

            for (int i = -2; i <= 2; i += 2)
                FillDisk(px, s, cx + i * r * 0.55f, cy - r * 0.55f, s * 0.03f, Red);

            for (int i = -1; i <= 1; i++)
            {
                float tx = cx + i * r * 0.4f;
                float ty = cy + r * 0.85f;
                DrawLine(px, s, tx, ty, tx + i * 3, ty + s * 0.08f, Outline);
                DrawLine(px, s, tx, ty, tx - i * 2, ty + s * 0.06f, Shad);
            }
        }

        private static void DrawMimic(Color[] px, int s)
        {
            int m = s / 9;
            int w = s - m * 2;
            int h = w;
            int x0 = m;
            int y0 = s / 10;
            FillRect(px, s, x0, y0, w, h, Mid);
            FillRect(px, s, x0 + 3, y0 + 3, w - 6, h / 3, Hi);
            FillRect(px, s, x0 + 3, y0 + h - h / 4, w - 6, h / 5, Shad);
            StrokeRect(px, s, x0, y0, w, h, Outline, 2);

            for (int row = 0; row < 3; row++)
            {
                int ly = y0 + h / 5 + row * (h / 5);
                DrawLine(px, s, x0 + 4, ly, x0 + w - 4, ly, new Color(0.75f, 0.78f, 0.82f, 1f));
            }

            FillDisk(px, s, x0 + w * 0.32f, y0 + h * 0.42f, s * 0.06f, Cyan);
            FillDisk(px, s, x0 + w * 0.32f - 2, y0 + h * 0.42f - 2, s * 0.02f, Hi);
            FillDisk(px, s, x0 + w * 0.68f, y0 + h * 0.45f, s * 0.045f, Cyan);
            FillRect(px, s, x0 + w / 2 - 2, (int)(y0 + h * 0.55f), 5, 2, DarkRed);

            int gx = x0 + w / 2;
            for (int dy = 0; dy < h / 2; dy++)
            {
                Set(px, s, gx + (dy % 4) - 1, y0 + dy, Purple);
                Set(px, s, gx - 2 - dy % 3, y0 + h - dy, Cyan);
            }

            FillRect(px, s, x0 + w - 5, y0 + h / 3, 4, h / 3, new Color(0.5f, 0.85f, 1f, 0.9f));
            DrawLine(px, s, x0 + w, y0 + h / 2, x0 + w + 4, y0 + h / 2 + 3, Outline);
            FillDisk(px, s, x0 + w + 4, y0 + h / 2 + 3, 3, Mid);

            for (int i = 0; i < 4; i++)
                Set(px, s, x0 + 2 + i * 2, y0 + 2, Outline);
        }

        private static void DrawParasite(Color[] px, int s)
        {
            float cx = s * 0.5f;
            float cy = s * 0.48f;
            float r = s * 0.24f;
            FillTriangle(px, s, cx, cy + r * 1.1f, cx - r * 1.1f, cy - r * 0.5f, cx + r * 1.1f, cy - r * 0.5f, Mid);
            FillTriangle(px, s, cx, cy + r * 0.7f, cx - r * 0.7f, cy - r * 0.2f, cx + r * 0.7f, cy - r * 0.2f, Hi);
            StrokeTriangle(px, s, cx, cy + r * 1.1f, cx - r * 1.1f, cy - r * 0.5f, cx + r * 1.1f, cy - r * 0.5f, Outline);
            StrokeTriangle(px, s, cx, cy + r * 1.05f, cx - r * 1.05f, cy - r * 0.45f, cx + r * 1.05f, cy - r * 0.45f, Outline);

            FillEllipse(px, s, cx, cy - r * 0.35f, r * 0.55f, r * 0.45f, new Color(0.82f, 0.95f, 0.78f, 1f));
            FillDisk(px, s, cx, cy - r * 0.35f, r * 0.28f, Green);
            FillDisk(px, s, cx, cy - r * 0.38f, r * 0.14f, Hi);
            StrokeDisk(px, s, cx, cy - r * 0.35f, r * 0.3f, Outline, 1);

            FillDisk(px, s, cx - r * 0.25f, cy - r * 0.2f, s * 0.035f, Red);
            FillDisk(px, s, cx + r * 0.25f, cy - r * 0.2f, s * 0.035f, Red);
            FillDisk(px, s, cx - r * 0.25f - 1, cy - r * 0.22f, 1, Hi);

            for (int i = 0; i < 4; i++)
            {
                float ang = (i / 4f) * Mathf.PI * 2f + 0.4f;
                float len = r * 0.9f;
                float x1 = cx + Mathf.Cos(ang) * r * 0.35f;
                float y1 = cy + Mathf.Sin(ang) * r * 0.2f;
                float x2 = cx + Mathf.Cos(ang) * len;
                float y2 = cy + Mathf.Sin(ang) * len + s * 0.06f;
                DrawLine(px, s, x1, y1, x2, y2, Outline);
                FillDisk(px, s, x2, y2, 2, Green);
            }

            FillEllipse(px, s, cx, cy + r * 0.95f, r * 0.35f, r * 0.12f, DarkRed);
            for (int i = -2; i <= 2; i++)
                Set(px, s, (int)(cx + i * 2), (int)(cy + r * 1.05f), Outline);

            DrawLine(px, s, cx, cy - r * 0.55f, cx, cy - r * 0.85f, Outline);
            FillDisk(px, s, cx, cy - r * 0.88f, 2, Purple);
        }

        #region Primitives

        private static void Set(Color[] px, int s, int x, int y, Color c)
        {
            if (x < 0 || x >= s || y < 0 || y >= s) return;
            px[y * s + x] = c;
        }

        private static void FillDisk(Color[] px, int s, float cx, float cy, float r, Color c)
        {
            FillEllipse(px, s, cx, cy, r, r, c);
        }

        private static void FillEllipse(Color[] px, int s, float cx, float cy, float rx, float ry, Color c)
        {
            int x0 = Mathf.Max(0, (int)(cx - rx - 1));
            int x1 = Mathf.Min(s - 1, (int)(cx + rx + 1));
            int y0 = Mathf.Max(0, (int)(cy - ry - 1));
            int y1 = Mathf.Min(s - 1, (int)(cy + ry + 1));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    if (dx * dx + dy * dy <= 1f)
                        Set(px, s, x, y, c);
                }
            }
        }

        private static void StrokeDisk(Color[] px, int s, float cx, float cy, float r, Color c, int thickness = 1)
        {
            int steps = Mathf.Max(24, (int)(r * 12f));
            for (int t = 0; t < thickness; t++)
            {
                float rt = r + t * 0.5f;
                for (int i = 0; i < steps; i++)
                {
                    float ang = i / (float)steps * Mathf.PI * 2f;
                    Set(px, s, (int)(cx + Mathf.Cos(ang) * rt), (int)(cy + Mathf.Sin(ang) * rt), c);
                }
            }
        }

        private static void StrokeEllipse(Color[] px, int s, float cx, float cy, float rx, float ry, Color c, int thickness = 1)
        {
            int steps = Mathf.Max(28, (int)((rx + ry) * 8f));
            for (int t = 0; t < thickness; t++)
            {
                float ex = rx + t * 0.4f;
                float ey = ry + t * 0.4f;
                for (int i = 0; i < steps; i++)
                {
                    float ang = i / (float)steps * Mathf.PI * 2f;
                    Set(px, s, (int)(cx + Mathf.Cos(ang) * ex), (int)(cy + Mathf.Sin(ang) * ey), c);
                }
            }
        }

        private static void FillRect(Color[] px, int s, int x0, int y0, int w, int h, Color c)
        {
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                    Set(px, s, x, y, c);
        }

        private static void StrokeRect(Color[] px, int s, int x0, int y0, int w, int h, Color c, int thickness = 1)
        {
            for (int t = 0; t < thickness; t++)
            {
                FillRect(px, s, x0 + t, y0 + t, w - t * 2, 1, c);
                FillRect(px, s, x0 + t, y0 + h - 1 - t, w - t * 2, 1, c);
                FillRect(px, s, x0 + t, y0 + t, 1, h - t * 2, c);
                FillRect(px, s, x0 + w - 1 - t, y0 + t, 1, h - t * 2, c);
            }
        }

        private static void FillRoundedRect(Color[] px, int s, int x0, int y0, int w, int h, int rad, Color c)
        {
            FillRect(px, s, x0 + rad, y0, w - rad * 2, h, c);
            FillRect(px, s, x0, y0 + rad, w, h - rad * 2, c);
            FillDisk(px, s, x0 + rad, y0 + rad, rad, c);
            FillDisk(px, s, x0 + w - rad, y0 + rad, rad, c);
            FillDisk(px, s, x0 + rad, y0 + h - rad, rad, c);
            FillDisk(px, s, x0 + w - rad, y0 + h - rad, rad, c);
        }

        private static void StrokeRoundedRect(Color[] px, int s, int x0, int y0, int w, int h, int rad, Color c, int thickness = 1)
        {
            StrokeRect(px, s, x0 + rad / 2, y0 + rad / 2, w - rad, h - rad, c, thickness);
            StrokeDisk(px, s, x0 + rad, y0 + rad, rad, c, thickness);
            StrokeDisk(px, s, x0 + w - rad, y0 + rad, rad, c, thickness);
            StrokeDisk(px, s, x0 + rad, y0 + h - rad, rad, c, thickness);
            StrokeDisk(px, s, x0 + w - rad, y0 + h - rad, rad, c, thickness);
        }

        private static void FillTriangle(Color[] px, int s, float x1, float y1, float x2, float y2, float x3, float y3, Color c)
        {
            float minX = Mathf.Min(x1, Mathf.Min(x2, x3));
            float maxX = Mathf.Max(x1, Mathf.Max(x2, x3));
            float minY = Mathf.Min(y1, Mathf.Min(y2, y3));
            float maxY = Mathf.Max(y1, Mathf.Max(y2, y3));
            for (int y = (int)minY; y <= (int)maxY; y++)
            {
                for (int x = (int)minX; x <= (int)maxX; x++)
                {
                    if (PointInTriangle(x, y, x1, y1, x2, y2, x3, y3))
                        Set(px, s, x, y, c);
                }
            }
        }

        private static void StrokeTriangle(Color[] px, int s, float x1, float y1, float x2, float y2, float x3, float y3, Color c)
        {
            DrawLine(px, s, x1, y1, x2, y2, c);
            DrawLine(px, s, x2, y2, x3, y3, c);
            DrawLine(px, s, x3, y3, x1, y1, c);
        }

        private static void DrawLine(Color[] px, int s, float x0, float y0, float x1, float y1, Color c)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1)) * 2f);
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0 : i / (float)steps;
                Set(px, s, (int)Mathf.Lerp(x0, x1, t), (int)Mathf.Lerp(y0, y1, t), c);
            }
        }

        private static void StrokeLine(Color[] px, int s, float x0, float y0, float x1, float y1, Color c)
        {
            DrawLine(px, s, x0, y0, x1, y1, c);
        }

        private static bool PointInTriangle(float px, float py, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            float d1 = Sign(px, py, x1, y1, x2, y2);
            float d2 = Sign(px, py, x2, y2, x3, y3);
            float d3 = Sign(px, py, x3, y3, x1, y1);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0;
            bool pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        private static float Sign(float px, float py, float x1, float y1, float x2, float y2)
        {
            return (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
        }

        #endregion
    }
}
