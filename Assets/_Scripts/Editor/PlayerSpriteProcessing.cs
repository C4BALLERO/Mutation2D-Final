#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Editor
{
    internal static class PlayerSpriteProcessing
    {
        public static void RemoveBackground(Texture2D tex, float colorTolerance = 0.12f)
        {
            int w = tex.width;
            int h = tex.height;
            bool[,] isBg = new bool[w, h];
            bool[,] visited = new bool[w, h];
            Queue<(int x, int y)> queue = new();

            void Enqueue(int x, int y)
            {
                if (x < 0 || x >= w || y < 0 || y >= h || visited[x, y])
                    return;
                visited[x, y] = true;
                Color c = tex.GetPixel(x, y);
                if (!IsBackgroundLike(c, colorTolerance))
                    return;
                isBg[x, y] = true;
                queue.Enqueue((x, y));
            }

            for (int x = 0; x < w; x++)
            {
                Enqueue(x, 0);
                Enqueue(x, h - 1);
            }

            for (int y = 0; y < h; y++)
            {
                Enqueue(0, y);
                Enqueue(w - 1, y);
            }

            while (queue.Count > 0)
            {
                (int x, int y) = queue.Dequeue();
                Enqueue(x + 1, y);
                Enqueue(x - 1, y);
                Enqueue(x, y + 1);
                Enqueue(x, y - 1);
            }

            Color[] px = tex.GetPixels();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!isBg[x, y])
                        continue;
                    int i = y * w + x;
                    px[i] = new Color(px[i].r, px[i].g, px[i].b, 0f);
                }
            }

            tex.SetPixels(px);
            tex.Apply();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = tex.GetPixel(x, y);
                    if (c.a < 0.05f)
                        continue;
                    if (IsEmberSpark(c))
                        tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply();
        }

        private static bool IsBackgroundLike(Color c, float tolerance)
        {
            if (c.a < 0.2f)
                return true;

            float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            float min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            float sat = max <= 0.001f ? 0f : (max - min) / max;
            float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;

            if (sat < 0.22f && lum > 0.28f && lum < 0.92f)
                return true;

            Color refBg = new(0.55f, 0.52f, 0.48f);
            return ColorDistance(c, refBg) < tolerance;
        }

        private static bool IsEmberSpark(Color c)
        {
            return c.r > 0.75f && c.g > 0.35f && c.g < 0.7f && c.b < 0.35f && c.a > 0.5f;
        }

        private static float ColorDistance(Color a, Color b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db);
        }
    }
}
#endif
