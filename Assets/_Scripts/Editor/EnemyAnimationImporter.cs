#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Auto-slices enemy animation PNGs in a single OnPreprocessTexture pass.
    /// Uses density-based column detection to avoid fragmenting sprites that have
    /// fine transparent details between body parts (thin necks, antennae, etc.).
    /// </summary>
    public sealed class EnemyAnimationImporter : AssetPostprocessor
    {
        private const string WatchFolder = "Assets/_Art/Animations/Enemies/";

        // Pixel must exceed this alpha to count as "opaque"
        private const float AlphaThresh = 0.08f;

        // A ROW is active if at least this fraction of its width has opaque pixels.
        private const float RowDensity = 0.003f;   // 0.3 % → ~11px out of 3840

        // A COLUMN is active if at least this fraction of the band height has opaque pixels.
        // Higher = fewer false splits from thin connecting details.
        private const float ColDensity = 0.015f;   // 1.5 % → ~6px out of 400

        // Minimum sprite rect dimensions — filters leftover noise after merging.
        private const int MinRectW = 60;
        private const int MinRectH = 60;

        // Padding added around each detected rect.
        private const int Padding = 3;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(WatchFolder)) return;

            var ti = (TextureImporter)assetImporter;

            // Skip if already sliced (guard against reimport loops)
            if (ti.spriteImportMode == SpriteImportMode.Multiple &&
                ti.spritesheet        != null &&
                ti.spritesheet.Length  > 0)
                return;

            // ── Base import settings ──────────────────────────────────────────
            ti.textureType         = TextureImporterType.Sprite;
            ti.spriteImportMode    = SpriteImportMode.Multiple;
            ti.spritePixelsPerUnit = 100;
            ti.filterMode          = FilterMode.Point;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;
            ti.isReadable          = false;
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize      = 8192;
            var platformDefault = ti.GetDefaultPlatformTextureSettings();
            platformDefault.maxTextureSize = 8192;
            platformDefault.overridden = false;
            ti.SetPlatformTextureSettings(platformDefault);

            // ── Load raw PNG into a temp texture (bypasses the import pipeline) ─
            byte[] raw = File.ReadAllBytes(assetPath);
            var   tmp  = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tmp.LoadImage(raw))
            {
                Object.DestroyImmediate(tmp);
                Debug.LogWarning($"[EnemyAnimImporter] No se pudo leer {Path.GetFileName(assetPath)}");
                return;
            }

            List<Rect> rects = DetectSpriteRects(tmp);
            Object.DestroyImmediate(tmp);

            if (rects.Count == 0)
            {
                Debug.LogWarning($"[EnemyAnimImporter] Sin frames en " +
                                 $"{Path.GetFileName(assetPath)} — usa Sprite Editor → Slice → Automatic manualmente.");
                return;
            }

            string baseName = Path.GetFileNameWithoutExtension(assetPath);
            var    sheet    = new SpriteMetaData[rects.Count];
            for (int i = 0; i < rects.Count; i++)
            {
                sheet[i] = new SpriteMetaData
                {
                    name      = baseName + "_" + i,
                    rect      = rects[i],
                    alignment = (int)SpriteAlignment.Center,
                    pivot     = new Vector2(0.5f, 0.5f)
                };
            }
            ti.spritesheet = sheet;

            Debug.Log($"[EnemyAnimImporter] {rects.Count} sprites → {Path.GetFileName(assetPath)}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Sprite detection — density-based row/column bands
        // ─────────────────────────────────────────────────────────────────────

        private static List<Rect> DetectSpriteRects(Texture2D tex)
        {
            int       W   = tex.width;
            int       H   = tex.height;
            Color32[] px  = tex.GetPixels32();
            byte      thr = (byte)(AlphaThresh * 255f);

            // ── Row activity: row is "active" if enough of its pixels are opaque ─
            int rowMin = Mathf.Max(1, (int)(W * RowDensity));

            bool[] rowActive = new bool[H];
            for (int y = 0; y < H; y++)
            {
                int cnt = 0, baseIdx = y * W;
                for (int x = 0; x < W; x++)
                    if (px[baseIdx + x].a >= thr && ++cnt >= rowMin) { rowActive[y] = true; break; }
            }

            var hBands = FindBands(rowActive, H);
            if (hBands.Count == 0) return new List<Rect>();

            var result = new List<Rect>(hBands.Count * 12);

            foreach (var (r0, r1) in hBands)
            {
                int bandH  = r1 - r0 + 1;

                // ── Column activity: column is "active" if enough pixels in this band are opaque ──
                // Using density threshold avoids false splits from thin transparent details.
                int colMin = Mathf.Max(5, (int)(bandH * ColDensity));

                bool[] colActive = new bool[W];
                for (int x = 0; x < W; x++)
                {
                    int cnt = 0;
                    for (int y = r0; y <= r1; y++)
                        if (px[y * W + x].a >= thr && ++cnt >= colMin) { colActive[x] = true; break; }
                }

                foreach (var (c0, c1) in FindBands(colActive, W))
                {
                    int rw = c1 - c0 + 1;
                    int rh = bandH;
                    if (rw < MinRectW || rh < MinRectH) continue; // filter noise

                    int rx  = Mathf.Max(0,  c0 - Padding);
                    int ry  = Mathf.Max(0,  r0 - Padding);
                    int rx2 = Mathf.Min(W,  c1 + 1 + Padding);
                    int ry2 = Mathf.Min(H,  r1 + 1 + Padding);

                    // Convert image Y (0=top) → Unity sprite Y (0=bottom)
                    result.Add(new Rect(rx, H - ry2, rx2 - rx, ry2 - ry));
                }
            }

            // Sort top→bottom (largest Unity Y first), then left→right
            result.Sort((a, b) =>
            {
                int c = b.y.CompareTo(a.y);
                return c != 0 ? c : a.x.CompareTo(b.x);
            });

            return result;
        }

        private static List<(int, int)> FindBands(bool[] flags, int len)
        {
            var  bands = new List<(int, int)>();
            int  start = -1;
            for (int i = 0; i < len; i++)
            {
                if ( flags[i] && start < 0)  start = i;
                if (!flags[i] && start >= 0) { bands.Add((start, i - 1)); start = -1; }
            }
            if (start >= 0) bands.Add((start, len - 1));
            return bands;
        }
    }
}
#endif
