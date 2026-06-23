using UnityEngine;

namespace MutationSwarm
{
    // Creates procedural pixel-art sprites at runtime
    public class SpriteFactory : MonoBehaviour
    {
        public static SpriteFactory Instance { get; private set; }

        public Sprite PixelSprite    { get; private set; }
        public Sprite CircleSprite   { get; private set; }
        public Sprite DiamondSprite  { get; private set; }
        public Sprite BulletSprite   { get; private set; }
        public Sprite DroneSprite    { get; private set; }
        public Sprite PlayerSprite   { get; private set; }
        public Sprite PlatformSprite { get; private set; }
        public Sprite FireballSprite { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            PixelSprite    = MakeSquare(4, Color.white);
            CircleSprite   = MakeCircle(32, Color.white);
            DiamondSprite  = MakeDiamond(16, Color.white);
            BulletSprite   = MakeBullet();
            DroneSprite    = MakeDrone();
            PlayerSprite   = MakePlayer();
            PlatformSprite = MakePlatform();
            FireballSprite = MakeFireball();
        }

        static Sprite MakeSquare(int size, Color c)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), Vector2.one*0.5f, size);
        }

        static Sprite MakeCircle(int size, Color c)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point };
            float r = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                tex.SetPixel(x, y, dx*dx + dy*dy <= r*r ? c : Color.clear);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), Vector2.one*0.5f, size);
        }

        static Sprite MakeDiamond(int size, Color c)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point };
            float h = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Mathf.Abs(x-h)+Mathf.Abs(y-h) < h ? c : Color.clear);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), Vector2.one*0.5f, size);
        }

        static Sprite MakeBullet()
        {
            int s = 12;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            float r = s / 2f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float d2 = dx * dx + dy * dy;
                Color c = d2 <= 1.5f * 1.5f  ? Color.white
                        : d2 <= 3.5f * 3.5f  ? new Color(1f, 0.97f, 0.55f, 1f)
                        : d2 <= r * r        ? new Color(1f, 0.85f, 0.2f, 0.6f)
                        :                      Color.clear;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), Vector2.one * 0.5f, 16);
        }

        static Sprite MakeFireball()
        {
            int s = 16;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            float r = s / 2f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float d2 = dx * dx + dy * dy;
                Color c = d2 <= 2f * 2f      ? Color.white
                        : d2 <= 4f * 4f      ? new Color(1f, 0.92f, 0.2f, 1f)
                        : d2 <= 5.5f * 5.5f  ? new Color(1f, 0.38f, 0.05f, 0.9f)
                        : d2 <= r * r        ? new Color(0.8f, 0.1f, 0f, 0.45f)
                        :                      Color.clear;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), Vector2.one * 0.5f, 16);
        }

        static Sprite MakeDrone()
        {
            int s = 16;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                bool body = x >= 4 && x <= 11 && y >= 6 && y <= 9;
                bool eye  = x >= 6 && x <= 9  && y >= 7 && y <= 8;
                tex.SetPixel(x, y, eye ? Color.cyan : body ? new Color(0.3f,0.6f,1f) : Color.clear);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,s,s), Vector2.one*0.5f, s);
        }

        // Chunky pixel-art player (hazmat suit)
        static Sprite MakePlayer()
        {
            int w = 16, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color suit  = new(0.78f, 0.84f, 0.78f);
            Color visor = new(0.13f, 0.73f, 0.93f);
            Color dark  = new(0.20f, 0.20f, 0.25f);
            Color gun   = new(0.25f, 0.30f, 0.30f);
            Color seam  = new(0.54f, 0.60f, 0.54f);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, Color.clear);

            // Legs
            Fill(tex, 3, 0, 6, 8,  dark);
            Fill(tex, 9, 0, 12, 8, dark);
            // Body
            Fill(tex, 2, 8,  14, 17, suit);
            // Seams
            Fill(tex, 2, 11, 14, 12, seam);
            Fill(tex, 2, 14, 14, 15, seam);
            // Head
            Fill(tex, 3, 17, 13, 24, suit);
            // Visor
            Fill(tex, 4, 18, 12, 22, visor);
            Fill(tex, 4, 21, 12, 22, new Color(1f,1f,1f,0.4f));
            // Gun arm
            Fill(tex, 12, 13, 14, 15, seam);
            Fill(tex, 13, 13, 16, 15, gun);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0f), w);
        }

        static void Fill(Texture2D t, int x0, int y0, int x1, int y1, Color c)
        {
            for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++) t.SetPixel(x, y, c);
        }

        static Sprite MakePlatform()
        {
            int w = 64, h = 8;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color top   = new(0.23f, 0.19f, 0.30f);
            Color body  = new(0.16f, 0.13f, 0.21f);
            Color dark  = new(0.09f, 0.07f, 0.12f);
            Color bolt  = new(0.31f, 0.25f, 0.38f);
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, h-1, top);
                tex.SetPixel(x, h-2, top);
                for (int y = 1; y < h-2; y++) tex.SetPixel(x, y, body);
                tex.SetPixel(x, 0, dark);
            }
            // Bolts
            tex.SetPixel(3, h-1, bolt); tex.SetPixel(4, h-1, bolt);
            tex.SetPixel(w-5, h-1, bolt); tex.SetPixel(w-4, h-1, bolt);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f, 1f), 8);
        }
    }
}
