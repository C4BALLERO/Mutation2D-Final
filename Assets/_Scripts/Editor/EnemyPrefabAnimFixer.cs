#if UNITY_EDITOR
using System.Linq;
using MutationSwarm.Entities;
using UnityEditor;
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Configura los 4 prefabs de enemigos con EnemySpriteAnimator, Rigidbody2D,
    /// CapsuleCollider2D y Script_13_EnemyBase. Asigna sus hojas de sprites.
    ///
    /// Ejecutar: Tools > Mutation Swarm > Setup Enemy Sprite Animations
    /// </summary>
    public static class EnemyPrefabAnimFixer
    {
        private const string AnimBase = "Assets/_Art/Animations/Enemies";
        private const string PrefabBase = "Assets/_Prefabs/Enemies";

        private static readonly EnemyConfig[] Configs =
        {
            new("Enemi_Diablito",
                idle:   $"{AnimBase}/Diablito_volando.png",
                walk:   $"{AnimBase}/Diablito_volando.png",
                attack: $"{AnimBase}/Diablito_atacando.png",
                death:  $"{AnimBase}/Diablito_dead.png",
                gravity: 0f,
                colliderSize: new Vector2(0.7f, 0.7f),
                colliderOffset: Vector2.zero),

            new("Enemi_Mono",
                idle:   null,
                walk:   $"{AnimBase}/Mono_walk.png",
                attack: $"{AnimBase}/Mono_Attack.png",
                death:  $"{AnimBase}/Mono_Dead.png",
                gravity: 1f,
                colliderSize: new Vector2(0.6f, 1.0f),
                colliderOffset: new Vector2(0f, 0.5f)),

            new("Enemi_Dino",
                idle:   null,
                walk:   $"{AnimBase}/Dino_Walk.png",
                attack: $"{AnimBase}/Dino_Attack.png",
                death:  $"{AnimBase}/Dino_Death.png",
                gravity: 1f,
                colliderSize: new Vector2(0.8f, 1.2f),
                colliderOffset: new Vector2(0f, 0.6f)),

            new("Enemi_3",
                idle:   null,
                walk:   $"{AnimBase}/enemy3_walk.png",
                attack: $"{AnimBase}/enemy3_attack.png",
                death:  $"{AnimBase}/enemy3_death.png",
                gravity: 1f,
                colliderSize: new Vector2(0.6f, 1.0f),
                colliderOffset: new Vector2(0f, 0.5f)),
        };

        /// <summary>
        /// Clears the spritesheet on all enemy animation PNGs and re-slices
        /// them with the improved density-based algorithm, then re-runs the setup.
        /// Use this if the auto-slicer produced too many (fragmented) or too few frames.
        /// </summary>
        [MenuItem("Tools/Mutation Swarm/Re-Slice and Setup Enemy Animations")]
        public static void ResliceAndSetupAll()
        {
            // Step 1: clear existing sprite data so EnemyAnimationImporter re-runs
            Debug.Log("[EnemyFixer] Re-sliceando animaciones de enemigos…");
            string[] pngGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { AnimBase });
            foreach (string guid in pngGuids)
            {
                string pngPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!pngPath.EndsWith(".png")) continue;
                var ti = AssetImporter.GetAtPath(pngPath) as TextureImporter;
                if (ti == null) continue;
                ti.spritesheet = System.Array.Empty<SpriteMetaData>(); // clear existing slice
                ti.spriteImportMode = SpriteImportMode.Single;          // force re-detect on next import
                ti.SaveAndReimport();
            }
            AssetDatabase.Refresh();
            Debug.Log("[EnemyFixer] Meta files reseteados, re-importando…");
            SetupAll(); // now run the full setup (LoadSprites will trigger the new importer)
        }

        [MenuItem("Tools/Mutation Swarm/Setup Enemy Sprite Animations")]
        public static void SetupAll()
        {
            int done = 0;
            foreach (var cfg in Configs)
            {
                string path = $"{PrefabBase}/{cfg.PrefabName}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    Debug.LogWarning($"[EnemyFixer] Prefab no encontrado: {path}");
                    continue;
                }

                using var scope = new PrefabUtility.EditPrefabContentsScope(path);
                GameObject root = scope.prefabContentsRoot;

                try
                {
                    SetupPrefab(root, cfg);
                    done++;
                    Debug.Log($"[EnemyFixer] ✅ {cfg.PrefabName} configurado.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[EnemyFixer] Error en {cfg.PrefabName}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[EnemyFixer] Completado: {done}/{Configs.Length} prefabs.");
        }

        // ─────────────────────────────────────────────────────────────────────

        private static void SetupPrefab(GameObject root, EnemyConfig cfg)
        {
            // Tag
            root.tag = "Enemy";

            // Reset root transform
            root.transform.localPosition = Vector3.zero;
            root.transform.localScale    = Vector3.one;

            // ── Rigidbody2D ──────────────────────────────────────────────────
            Rigidbody2D rb = EnsureComponent<Rigidbody2D>(root);
            rb.gravityScale          = cfg.Gravity;
            rb.linearDamping         = 0f;
            rb.angularDamping        = 0f;
            rb.constraints           = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation         = RigidbodyInterpolation2D.Interpolate;

            // ── CapsuleCollider2D ─────────────────────────────────────────────
            CapsuleCollider2D col = EnsureComponent<CapsuleCollider2D>(root);
            col.size      = cfg.ColliderSize;
            col.offset    = cfg.ColliderOffset;
            col.isTrigger = false;

            // ── SpriteRenderer ────────────────────────────────────────────────
            SpriteRenderer sr = EnsureComponent<SpriteRenderer>(root);
            sr.sortingOrder = 5;

            // ── Script_13_EnemyBase ────────────────────────────────────────────
            EnsureComponent<Script_13_EnemyBase>(root);

            // ── EnemySpriteAnimator ────────────────────────────────────────────
            EnemySpriteAnimator anim = EnsureComponent<EnemySpriteAnimator>(root);
            var soAnim = new SerializedObject(anim);

            soAnim.FindProperty("spriteRenderer").objectReferenceValue = sr;

            if (!string.IsNullOrEmpty(cfg.IdlePath))
                SetSpriteArray(soAnim, "idleFrames", LoadSprites(cfg.IdlePath));
            else
                ClearSpriteArray(soAnim, "idleFrames");

            SetSpriteArray(soAnim, "walkFrames",   LoadSprites(cfg.WalkPath));
            SetSpriteArray(soAnim, "attackFrames", LoadSprites(cfg.AttackPath));
            SetSpriteArray(soAnim, "deathFrames",  LoadSprites(cfg.DeathPath));

            soAnim.FindProperty("idleFps").floatValue   = 8f;
            soAnim.FindProperty("walkFps").floatValue   = 12f;
            soAnim.FindProperty("attackFps").floatValue = 14f;
            soAnim.FindProperty("deathFps").floatValue  = 10f;
            soAnim.FindProperty("flipXWithMovement").boolValue = true;
            soAnim.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─────────────────────────────────────────────────────────────────────

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static Sprite[] LoadSprites(string path)
        {
            if (string.IsNullOrEmpty(path)) return System.Array.Empty<Sprite>();

            // If texture isn't sliced yet (or has no meta), force import.
            // EnemyAnimationImporter runs synchronously inside ImportAsset
            // and sets all sprite rects in one pass.
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            bool needsImport = ti == null
                            || ti.spriteImportMode != SpriteImportMode.Multiple
                            || ti.spritesheet      == null
                            || ti.spritesheet.Length == 0;
            if (needsImport)
            {
                Debug.Log($"[EnemyFixer] Importando sprites: {System.IO.Path.GetFileName(path)}");
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }

            var all     = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = all.OfType<Sprite>().ToList();

            sprites.Sort((a, b) => ParseTrailingInt(a.name).CompareTo(ParseTrailingInt(b.name)));

            if (sprites.Count == 0)
                Debug.LogWarning($"[EnemyFixer] Sin sprites en: {path} — " +
                                 "Usa el Sprite Editor → Slice → Automatic si el PNG no tiene transparencia entre frames.");
            else
                Debug.Log($"[EnemyFixer] {sprites.Count} frames cargados de {System.IO.Path.GetFileName(path)}");

            return sprites.ToArray();
        }

        private static int ParseTrailingInt(string name)
        {
            int i = name.Length - 1;
            while (i >= 0 && char.IsDigit(name[i])) i--;
            string num = name.Substring(i + 1);
            return num.Length > 0 ? int.Parse(num) : 0;
        }

        private static void SetSpriteArray(SerializedObject so, string prop, Sprite[] sprites)
        {
            var p = so.FindProperty(prop);
            if (p == null) { Debug.LogWarning($"[EnemyFixer] Propiedad '{prop}' no encontrada."); return; }
            p.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void ClearSpriteArray(SerializedObject so, string prop)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.arraySize = 0;
        }

        // ─────────────────────────────────────────────────────────────────────

        private class EnemyConfig
        {
            public readonly string  PrefabName;
            public readonly string  IdlePath;
            public readonly string  WalkPath;
            public readonly string  AttackPath;
            public readonly string  DeathPath;
            public readonly float   Gravity;
            public readonly Vector2 ColliderSize;
            public readonly Vector2 ColliderOffset;

            public EnemyConfig(string prefabName, string idle, string walk,
                               string attack, string death,
                               float gravity, Vector2 colliderSize, Vector2 colliderOffset)
            {
                PrefabName     = prefabName;
                IdlePath       = idle;
                WalkPath       = walk;
                AttackPath     = attack;
                DeathPath      = death;
                Gravity        = gravity;
                ColliderSize   = colliderSize;
                ColliderOffset = colliderOffset;
            }
        }
    }
}
#endif
