using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Genera sprites geométricos simples (cuadrado, círculo, triángulo) para prototipo.
    /// </summary>
    public static class GeometricSpriteFactory
    {
        public enum Shape { Square, Circle, Triangle }

        public static Sprite Create(Shape shape, Color color, int size = 64, float ppu = 64f)
        {
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = Color.clear;
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f) / size;
                    float ny = (y + 0.5f) / size;
                    pixels[y * size + x] = IsInside(shape, nx, ny) ? color : clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        }

        private static bool IsInside(Shape shape, float nx, float ny)
        {
            switch (shape)
            {
                case Shape.Square:
                    return nx > 0.12f && nx < 0.88f && ny > 0.12f && ny < 0.88f;
                case Shape.Circle:
                {
                    float dx = nx - 0.5f;
                    float dy = ny - 0.5f;
                    return dx * dx + dy * dy < 0.2f;
                }
                case Shape.Triangle:
                    return ny < 0.2f + nx * 0.65f && ny < 0.2f + (1f - nx) * 0.65f && ny > 0.1f;
                default:
                    return false;
            }
        }
    }
}
