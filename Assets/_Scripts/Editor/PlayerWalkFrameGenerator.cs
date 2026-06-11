#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Editor
{
    internal static class PlayerWalkFrameGenerator
    {
        public const int IdleFrames = 2;
        public const int WalkFrames = 8;

        public static Texture2D BuildSheet(Texture2D baseFrame, int frameW, int frameH)
        {
            int total = IdleFrames + WalkFrames;
            Texture2D sheet = new(frameW * total, frameH, TextureFormat.RGBA32, false);
            sheet.filterMode = FilterMode.Point;
            Clear(sheet);

            for (int i = 0; i < IdleFrames; i++)
            {
                float breathe = i == 0 ? 0f : 1f;
                Texture2D frame = BakeFrame(baseFrame, breathe, 0f, 0f, 0f);
                Blit(sheet, frame, i * frameW, 0);
                Object.DestroyImmediate(frame);
            }

            for (int i = 0; i < WalkFrames; i++)
            {
                float phase = i / (float)WalkFrames * Mathf.PI * 2f;
                float leg = Mathf.Sin(phase);
                float arm = Mathf.Cos(phase);
                float bob = Mathf.Abs(Mathf.Sin(phase));
                Texture2D frame = BakeFrame(baseFrame, 0f, leg, arm, bob);
                Blit(sheet, frame, (IdleFrames + i) * frameW, 0);
                Object.DestroyImmediate(frame);
            }

            sheet.Apply();
            return sheet;
        }

        private static Texture2D BakeFrame(Texture2D src, float idleBreath, float legPhase, float armPhase, float bodyBob)
        {
            int w = src.width;
            int h = src.height;
            Texture2D dst = new(w, h, TextureFormat.RGBA32, false);
            Clear(dst);

            int legShiftL = Mathf.RoundToInt(legPhase * 4f);
            int legShiftR = Mathf.RoundToInt(-legPhase * 4f);
            int armShiftL = Mathf.RoundToInt(-armPhase * 3f);
            int armShiftR = Mathf.RoundToInt(armPhase * 3f);
            int bob = Mathf.RoundToInt(bodyBob * 2f + idleBreath * 1f);

            float legY = h * 0.38f;
            float armYMin = h * 0.42f;
            float armYMax = h * 0.78f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = src.GetPixel(x, y);
                    if (c.a < 0.08f)
                        continue;

                    int ox = x;
                    int oy = y + bob;

                    if (y < legY)
                    {
                        bool leftLeg = x < w * 0.52f;
                        oy = y + (leftLeg ? legShiftL : legShiftR) + bob;
                    }
                    else if (y >= armYMin && y <= armYMax)
                    {
                        if (x < w * 0.28f)
                            oy = y + armShiftL + bob;
                        else if (x > w * 0.72f)
                            oy = y + armShiftR + bob;
                    }

                    Plot(dst, ox, oy, c);
                }
            }

            dst.Apply();
            return dst;
        }

        private static void Plot(Texture2D tex, int x, int y, Color c)
        {
            if (x < 0 || x >= tex.width || y < 0 || y >= tex.height)
                return;
            Color existing = tex.GetPixel(x, y);
            if (existing.a < c.a)
                tex.SetPixel(x, y, c);
            else if (c.a > 0.5f)
                tex.SetPixel(x, y, AlphaBlend(existing, c));
        }

        private static Color AlphaBlend(Color under, Color over)
        {
            float a = over.a + under.a * (1f - over.a);
            if (a <= 0.001f)
                return Color.clear;
            return new Color(
                (over.r * over.a + under.r * under.a * (1f - over.a)) / a,
                (over.g * over.a + under.g * under.a * (1f - over.a)) / a,
                (over.b * over.a + under.b * under.a * (1f - over.a)) / a,
                a);
        }

        private static void Blit(Texture2D sheet, Texture2D frame, int offsetX, int offsetY)
        {
            for (int y = 0; y < frame.height; y++)
            {
                for (int x = 0; x < frame.width; x++)
                {
                    Color c = frame.GetPixel(x, y);
                    if (c.a < 0.05f)
                        continue;
                    sheet.SetPixel(offsetX + x, offsetY + y, c);
                }
            }
        }

        private static void Clear(Texture2D tex)
        {
            Color[] clear = new Color[tex.width * tex.height];
            tex.SetPixels(clear);
            tex.Apply();
        }
    }
}
#endif
