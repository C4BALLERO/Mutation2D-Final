#if UNITY_EDITOR
using System.IO;
using MutationSwarm.Combat;
using MutationSwarm.Entities;
using UnityEditor;
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Jugador Argos: sin fondo, spritesheet walk/idle y Animator.
    /// </summary>
    public static class MutationSwarmPlayerArtSetup
    {
        private const string SpritesPath = "Assets/_Art/Sprites/Player";
        private const string PrefabsPath = "Assets/_Prefabs/Player";
        private const string SourceFile = "Spr_Player_ArgosArmor_source.png";
        private const string SheetFile = "Spr_Player_ArgosArmor_sheet.png";

        public const int TargetWidth = 96;
        public const int TargetHeight = 128;
        public const float PixelsPerUnit = 96f;

        [MenuItem("Tools/Mutation Swarm/Build Player Sprite (Argos Armor)")]
        public static void BuildAll()
        {
            EnsureFolder(SpritesPath);
            EnsureFolder(PrefabsPath);

            string sourcePath = $"{SpritesPath}/{SourceFile}";
            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"[MutationSwarm] Falta {sourcePath}.");
                return;
            }

            Texture2D source = LoadTexture(sourcePath);
            Texture2D cropped = CropCharacterRegion(source);
            Texture2D scaled = ResizeNearest(cropped, TargetWidth, TargetHeight);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(cropped);

            PlayerSpriteProcessing.RemoveBackground(scaled);

            Texture2D sheet = PlayerWalkFrameGenerator.BuildSheet(scaled, TargetWidth, TargetHeight);
            Object.DestroyImmediate(scaled);

            string sheetPath = $"{SpritesPath}/{SheetFile}";
            File.WriteAllBytes(sheetPath, sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);

            AssetDatabase.ImportAsset(sheetPath);
            RuntimeAnimatorController controller = MutationSwarmPlayerAnimatorSetup.BuildFromSheet(
                sheetPath, TargetWidth, TargetHeight);

            Sprite idleSprite = LoadFirstSheetSprite(sheetPath);
            GameObject prefabGo = BuildPlayerPrefab(idleSprite, controller);
            string prefabPath = $"{PrefabsPath}/Prefab_Player.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefabGo, prefabPath);
            Object.DestroyImmediate(prefabGo);

            UpdateGeoPrefab(controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] Jugador: fondo transparente + idle/walk + AC_Player listo.");
        }

        private static Sprite LoadFirstSheetSprite(string sheetPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            foreach (Object o in assets)
            {
                if (o is Sprite s && s.name.Contains("frame_00"))
                    return s;
            }

            foreach (Object o in assets)
            {
                if (o is Sprite s)
                    return s;
            }

            return null;
        }

        private static Texture2D CropCharacterRegion(Texture2D src)
        {
            int w = src.width;
            int h = src.height;
            int minX = w, minY = h, maxX = 0, maxY = 0;
            bool any = false;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (src.GetPixel(x, y).a < 0.15f)
                        continue;
                    any = true;
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (!any)
                return src;

            int pad = Mathf.RoundToInt((maxX - minX) * 0.04f);
            minX = Mathf.Max(0, minX - pad);
            minY = Mathf.Max(0, minY - pad);
            maxX = Mathf.Min(w - 1, maxX + pad);
            maxY = Mathf.Min(h - 1, maxY + pad);

            int cw = maxX - minX + 1;
            int ch = maxY - minY + 1;
            Texture2D crop = new(cw, ch, TextureFormat.RGBA32, false);
            crop.SetPixels(src.GetPixels(minX, minY, cw, ch));
            crop.Apply();
            return crop;
        }

        private static Texture2D ResizeNearest(Texture2D src, int targetW, int targetH)
        {
            Texture2D dst = new(targetW, targetH, TextureFormat.RGBA32, false);
            dst.filterMode = FilterMode.Point;
            Color[] outPx = new Color[targetW * targetH];
            for (int y = 0; y < targetH; y++)
            {
                float v = y / (float)(targetH - 1);
                int sy = Mathf.Clamp(Mathf.RoundToInt(v * (src.height - 1)), 0, src.height - 1);
                for (int x = 0; x < targetW; x++)
                {
                    float u = x / (float)(targetW - 1);
                    int sx = Mathf.Clamp(Mathf.RoundToInt(u * (src.width - 1)), 0, src.width - 1);
                    outPx[y * targetW + x] = src.GetPixel(sx, sy);
                }
            }

            dst.SetPixels(outPx);
            dst.Apply();
            return dst;
        }

        private static Texture2D LoadTexture(string assetPath)
        {
            string full = Path.GetFullPath(assetPath);
            byte[] data = File.ReadAllBytes(full);
            Texture2D tex = new(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(data);
            return tex;
        }

        private static GameObject BuildPlayerPrefab(Sprite sprite, RuntimeAnimatorController controller)
        {
            GameObject go = new("Prefab_Player");
            go.tag = "Player";
            go.layer = LayerMask.NameToLayer("Player");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;

            Animator animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.updateMode = AnimatorUpdateMode.Normal;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            float colW = TargetWidth / PixelsPerUnit * 0.45f;
            float colH = TargetHeight / PixelsPerUnit * 0.88f;
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(colW, colH);
            box.offset = new Vector2(0f, colH * 0.02f);

            CapsuleCollider2D ground = go.AddComponent<CapsuleCollider2D>();
            ground.isTrigger = true;
            ground.size = new Vector2(colW * 0.7f, colH * 0.12f);
            ground.offset = new Vector2(0f, -colH * 0.42f);

            go.AddComponent<Script_12_PlayerStats>();
            Script_38_PlayerLoadout loadout = go.AddComponent<Script_38_PlayerLoadout>();
            Script_11_PlayerController controllerScript = go.AddComponent<Script_11_PlayerController>();

            float feetY = -colH * 0.42f;
            GameObject groundCheck = new("GroundCheck");
            groundCheck.transform.SetParent(go.transform);
            groundCheck.transform.localPosition = new Vector3(0f, feetY, 0f);

            GameObject firePoint = new("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(colW * 0.55f, colH * 0.15f, 0f);

            SO_WeaponData starter = AssetDatabase.LoadAssetAtPath<SO_WeaponData>(
                "Assets/_ScriptableObjects/Combat/Weapons/SO_Weapon_glock_p80.asset");
            SerializedObject soL = new(loadout);
            soL.FindProperty("_startingWeapon").objectReferenceValue = starter;
            soL.FindProperty("_weaponAttachPoint").objectReferenceValue = go.transform;
            soL.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soC = new(controllerScript);
            soC.FindProperty("_groundCheckPoint").objectReferenceValue = groundCheck.transform;
            int platformLayer = LayerMask.NameToLayer("Platform");
            soC.FindProperty("_groundMask").intValue = 1 << platformLayer;
            soC.FindProperty("_wallMask").intValue = 1 << platformLayer;
            soC.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        private static void UpdateGeoPrefab(RuntimeAnimatorController controller)
        {
            string geoPath = $"{PrefabsPath}/Prefab_Player_Geo.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(geoPath);
            if (existing == null)
                return;

            GameObject instance = PrefabUtility.InstantiatePrefab(existing) as GameObject;
            if (instance == null)
                return;

            Animator anim = instance.GetComponent<Animator>();
            if (anim == null)
                anim = instance.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            PrefabUtility.SaveAsPrefabAsset(instance, geoPath);
            Object.DestroyImmediate(instance);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name) && AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
