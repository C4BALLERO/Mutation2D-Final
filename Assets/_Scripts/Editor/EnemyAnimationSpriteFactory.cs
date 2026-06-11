#if UNITY_EDITOR
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Generates pixel-art animation frames from the base enemy sprite using
    /// simple offset / scale / alpha transforms. Each state produces a Color[][]
    /// (array of per-frame pixel arrays) ready for spritesheet assembly.
    /// </summary>
    public static class EnemyAnimationSpriteFactory
    {
        public enum AnimState { Idle, Move, Attack, Hit, Die }

        // Frame counts per enemy type × state ---------------------------------
        private static readonly int[,] FrameCounts =
        {
            //            Idle Move Attack Hit Die
            /* Drone   */ { 4,  4,   3,    2,  3 },
            /* Boss    */ { 4,  6,   4,    2,  4 },
            /* Queen   */ { 4,  4,   4,    2,  4 },
            /* Mimic   */ { 3,  4,   3,    2,  3 },
            /* Parasite*/ { 3,  4,   3,    2,  3 },
        };

        public static int GetFrameCount(Core.EnemySpriteFactory.Archetype archetype, AnimState state)
            => FrameCounts[(int)archetype, (int)state];

        /// <summary>Generates all animation frames for a given archetype + state.</summary>
        public static Color[][] GenerateFrames(
            Core.EnemySpriteFactory.Archetype archetype,
            AnimState state,
            int size = 64)
        {
            // Obtain base pixel array from the sprite factory
            Color[] basePx = GetBasePixels(archetype, size);
            int frameCount = GetFrameCount(archetype, state);
            Color[][] frames = new Color[frameCount][];

            switch (state)
            {
                case AnimState.Idle:
                    frames = BuildIdleFrames(basePx, size, frameCount);
                    break;
                case AnimState.Move:
                    frames = BuildMoveFrames(basePx, size, frameCount, archetype);
                    break;
                case AnimState.Attack:
                    frames = BuildAttackFrames(basePx, size, frameCount);
                    break;
                case AnimState.Hit:
                    frames = BuildHitFrames(basePx, size);
                    break;
                case AnimState.Die:
                    frames = BuildDieFrames(basePx, size, frameCount);
                    break;
            }

            return frames;
        }

        // ── Frame builders ───────────────────────────────────────────────────

        // Subtle vertical bob
        private static Color[][] BuildIdleFrames(Color[] src, int s, int count)
        {
            int[] yOffsets = { 0, -1, 0, 1 };
            Color[][] result = new Color[count][];
            for (int i = 0; i < count; i++)
                result[i] = ShiftPixels(src, s, 0, yOffsets[i % yOffsets.Length]);
            return result;
        }

        // Horizontal waddle with slight vertical dip
        private static Color[][] BuildMoveFrames(Color[] src, int s, int count,
            Core.EnemySpriteFactory.Archetype archetype)
        {
            // Boss has heavier, wider movement
            bool heavy = archetype == Core.EnemySpriteFactory.Archetype.Boss;
            int amp = heavy ? 3 : 2;
            int[] xOff = { amp, 0, -amp, 0, amp, 0 };
            int[] yOff = { -1, 0, -1, 0, -1, 0 };

            Color[][] result = new Color[count][];
            for (int i = 0; i < count; i++)
                result[i] = ShiftPixels(src, s, xOff[i % xOff.Length], yOff[i % yOff.Length]);
            return result;
        }

        // Wind-up → strike (scale + brightness) → recoil
        private static Color[][] BuildAttackFrames(Color[] src, int s, int count)
        {
            float[] scaleX = { 0.95f, 1.15f, 0.92f, 1.0f };
            float[] scaleY = { 1.0f,  0.88f, 1.05f, 1.0f };
            float[] bright = { 1.0f,  1.35f, 1.0f,  1.0f };

            Color[][] result = new Color[count][];
            for (int i = 0; i < count; i++)
            {
                Color[] scaled = ScalePixels(src, s, scaleX[i % scaleX.Length], scaleY[i % scaleY.Length]);
                result[i] = BrightenPixels(scaled, bright[i % bright.Length]);
            }
            return result;
        }

        // 2-frame white flash
        private static Color[][] BuildHitFrames(Color[] src, int s)
        {
            return new[]
            {
                src,                              // F0: normal
                WhiteFlashPixels(src, 0.85f),    // F1: white flash
            };
        }

        // Fall + fade out
        private static Color[][] BuildDieFrames(Color[] src, int s, int count)
        {
            int[] yDrop  = { 0,  2,  6,  11 };
            float[] alpha = { 1f, 0.8f, 0.45f, 0.1f };

            Color[][] result = new Color[count][];
            for (int i = 0; i < count; i++)
            {
                Color[] shifted = ShiftPixels(src, s, 0, yDrop[i % yDrop.Length]);
                result[i] = AlphaPixels(shifted, alpha[i % alpha.Length]);
            }
            return result;
        }

        // ── Pixel transforms ────────────────────────────────────────────────

        private static Color[] GetBasePixels(Core.EnemySpriteFactory.Archetype archetype, int size)
        {
            Sprite baseSprite = Core.EnemySpriteFactory.Create(archetype, size, 64f);
            Color[] pixels = baseSprite.texture.GetPixels();
            Object.DestroyImmediate(baseSprite.texture);
            Object.DestroyImmediate(baseSprite);
            return pixels;
        }

        private static Color[] ShiftPixels(Color[] src, int s, int dx, int dy)
        {
            Color[] dst = new Color[src.Length];
            for (int y = 0; y < s; y++)
            {
                int srcY = y - dy;
                if (srcY < 0 || srcY >= s) continue;
                for (int x = 0; x < s; x++)
                {
                    int srcX = x - dx;
                    if (srcX < 0 || srcX >= s) continue;
                    dst[y * s + x] = src[srcY * s + srcX];
                }
            }
            return dst;
        }

        private static Color[] ScalePixels(Color[] src, int s, float sx, float sy)
        {
            Color[] dst = new Color[src.Length];
            float cx = s * 0.5f, cy = s * 0.5f;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    int srcX = Mathf.RoundToInt(cx + (x - cx) / sx);
                    int srcY = Mathf.RoundToInt(cy + (y - cy) / sy);
                    if (srcX < 0 || srcX >= s || srcY < 0 || srcY >= s) continue;
                    dst[y * s + x] = src[srcY * s + srcX];
                }
            }
            return dst;
        }

        private static Color[] BrightenPixels(Color[] src, float factor)
        {
            Color[] dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                Color c = src[i];
                dst[i] = new Color(
                    Mathf.Min(1f, c.r * factor),
                    Mathf.Min(1f, c.g * factor),
                    Mathf.Min(1f, c.b * factor),
                    c.a);
            }
            return dst;
        }

        private static Color[] AlphaPixels(Color[] src, float alpha)
        {
            Color[] dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                Color c = src[i];
                dst[i] = new Color(c.r, c.g, c.b, c.a * alpha);
            }
            return dst;
        }

        private static Color[] WhiteFlashPixels(Color[] src, float blend)
        {
            Color[] dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                Color c = src[i];
                if (c.a > 0f)
                    dst[i] = new Color(
                        Mathf.Lerp(c.r, 1f, blend),
                        Mathf.Lerp(c.g, 1f, blend),
                        Mathf.Lerp(c.b, 1f, blend),
                        c.a);
                else
                    dst[i] = c;
            }
            return dst;
        }

        /// <summary>Assembles frame arrays into a single horizontal spritesheet Texture2D.</summary>
        public static Texture2D BuildSpritesheet(Color[][] frames, int frameSize)
        {
            int totalWidth = frameSize * frames.Length;
            Texture2D sheet = new(totalWidth, frameSize, TextureFormat.RGBA32, false);
            sheet.filterMode = FilterMode.Point;

            Color[] clear = new Color[totalWidth * frameSize];
            sheet.SetPixels(clear);

            for (int f = 0; f < frames.Length; f++)
            {
                int xOffset = f * frameSize;
                sheet.SetPixels(xOffset, 0, frameSize, frameSize, frames[f]);
            }

            sheet.Apply();
            return sheet;
        }
    }
}
#endif
