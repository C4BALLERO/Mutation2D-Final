using System.Collections.Generic;
using System.Linq;
using MutationSwarm.Combat;
using MutationSwarm.Entities;
using UnityEditor;
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Configura _Player.prefab para el juego top-down:
    ///   - Rigidbody2D sin gravedad
    ///   - PlayerSpriteAnimator con los 4 sprite sheets del jugador
    ///   - Script_38_PlayerLoadout (manejador de armas) con FirePoint
    ///   - Arma básica por defecto (Script_20_WeaponBasic)
    ///
    /// Ejecutar: Tools > Mutation Swarm > Setup Player Prefab
    /// </summary>
    public static class MutationSwarmPlayerPrefabFixer
    {
        private const string PrefabPath = "Assets/_Prefabs/Player/_Player.prefab";

        private const string IdlePath   = "Assets/_Art/Animations/Player/Idle_Player.png";
        private const string WalkPath   = "Assets/_Art/Animations/Player/Walk_Player.png";
        private const string JumpPath   = "Assets/_Art/Animations/Player/Jump_Player.png";
        private const string AttackPath = "Assets/_Art/Animations/Player/Posicion_Arma.png";
        private const string DashPath   = "Assets/_Art/Animations/Player/Dahs_Player.png";

        [MenuItem("Tools/Mutation Swarm/Setup Player Prefab")]
        public static void SetupPlayerPrefab()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError($"[PlayerPrefabFixer] Prefab no encontrado en {PrefabPath}");
                return;
            }

            using var scope = new PrefabUtility.EditPrefabContentsScope(PrefabPath);
            GameObject root = scope.prefabContentsRoot;

            try
            {
                Configure(root);
                Debug.Log("[PlayerPrefabFixer] ✅ _Player.prefab configurado correctamente.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayerPrefabFixer] Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────

        private static void Configure(GameObject root)
        {
            // Tag
            root.tag = "Player";

            // ── Physics ──────────────────────────────────────────────────────

            // Reset root transform so the prefab has no unwanted offsets
            root.transform.localPosition = Vector3.zero;
            root.transform.localScale    = Vector3.one;

            Rigidbody2D rb = EnsureComponent<Rigidbody2D>(root);
            rb.gravityScale = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Capsule covers the character body — pivot is bottom-left so feet are at Y=0,
            // head at Y≈2.09. Offset (0, 0.9) places capsule bottom at Y=0 (feet).
            CapsuleCollider2D capsule = EnsureComponent<CapsuleCollider2D>(root);
            capsule.size      = new Vector2(0.65f, 1.8f);
            capsule.offset    = new Vector2(0f, 0.9f);
            capsule.isTrigger = false;

            BoxCollider2D box = root.GetComponent<BoxCollider2D>();
            if (box != null)
                Object.DestroyImmediate(box);

            // Disable the root-level SpriteRenderer (visual lives on the child)
            SpriteRenderer rootSr = root.GetComponent<SpriteRenderer>();
            if (rootSr != null)
                rootSr.enabled = false;

            // ── Visual child ─────────────────────────────────────────────────

            // Rename legacy "Idle_Player_0" child to "Visual" (or create it)
            Transform visualTrans = root.transform.Find("Visual");
            if (visualTrans == null)
            {
                visualTrans = root.transform.Find("Idle_Player_0");
                if (visualTrans != null)
                    visualTrans.gameObject.name = "Visual";
            }
            if (visualTrans == null)
            {
                var go = new GameObject("Visual");
                visualTrans = go.transform;
                visualTrans.SetParent(root.transform, false);
                visualTrans.localPosition = new Vector3(-0.54f, 0f, 0f);
                visualTrans.localScale    = new Vector3(0.41f, 0.40f, 1f);
            }

            SpriteRenderer visualSr = EnsureComponent<SpriteRenderer>(visualTrans.gameObject);
            visualSr.enabled      = true;
            visualSr.sortingOrder = 5;

            // ── PlayerSpriteAnimator ─────────────────────────────────────────

            PlayerSpriteAnimator spriteAnim = EnsureComponent<PlayerSpriteAnimator>(root);

            var soAnim = new SerializedObject(spriteAnim);
            soAnim.FindProperty("spriteRenderer").objectReferenceValue = visualSr;
            SetSpriteArray(soAnim, "idleFrames",   LoadSprites(IdlePath));
            SetSpriteArray(soAnim, "walkFrames",   LoadSprites(WalkPath));
            SetSpriteArray(soAnim, "jumpFrames",   LoadSprites(JumpPath));
            SetSpriteArray(soAnim, "attackFrames", LoadSprites(AttackPath));
            SetSpriteArray(soAnim, "dashFrames",   LoadSprites(DashPath));
            soAnim.FindProperty("idleFps").floatValue   = 10f;
            soAnim.FindProperty("walkFps").floatValue   = 14f;
            soAnim.FindProperty("jumpFps").floatValue   = 18f;
            soAnim.FindProperty("attackFps").floatValue = 18f;
            soAnim.FindProperty("dashFps").floatValue   = 24f;
            soAnim.FindProperty("faceWithFlipX").boolValue = true;
            soAnim.ApplyModifiedPropertiesWithoutUndo();

            // ── Remove duplicate Script_12_PlayerStats ────────────────────────

            var allStats = root.GetComponents<Script_12_PlayerStats>();
            for (int i = 1; i < allStats.Length; i++)
                Object.DestroyImmediate(allStats[i]);

            Script_12_PlayerStats stats = EnsureComponent<Script_12_PlayerStats>(root);

            // ── Script_11_PlayerController ────────────────────────────────────

            Script_11_PlayerController ctrl = EnsureComponent<Script_11_PlayerController>(root);
            var soCtrl = new SerializedObject(ctrl);
            soCtrl.FindProperty("_rb").objectReferenceValue             = rb;
            soCtrl.FindProperty("_animator").objectReferenceValue       = root.GetComponent<Animator>();
            soCtrl.FindProperty("_spriteAnimator").objectReferenceValue = spriteAnim;
            soCtrl.FindProperty("_speedBonusPerTier").floatValue        = 0.35f;
            soCtrl.ApplyModifiedPropertiesWithoutUndo();

            // ── GunPivot child (rotates to aim, holds gun sprite + weapon) ────

            Transform gunPivot = root.transform.Find("GunPivot");
            if (gunPivot == null)
            {
                // Rename legacy "Weapon_Primary" if it exists
                Transform legacy = root.transform.Find("Weapon_Primary");
                if (legacy != null)
                {
                    legacy.gameObject.name = "GunPivot";
                    gunPivot = legacy;
                }
                else
                {
                    var gpGo = new GameObject("GunPivot");
                    gunPivot = gpGo.transform;
                    gunPivot.SetParent(root.transform, false);
                }
            }
            gunPivot.localPosition = new Vector3(0f, 0.9f, 0f);

            // Gun sprite (Glock)
            SpriteRenderer gunSr = EnsureComponent<SpriteRenderer>(gunPivot.gameObject);
            gunSr.sortingOrder = 6;

            // ── FirePoint under GunPivot (rotates with the gun) ───────────────

            Transform firePoint = gunPivot.Find("FirePoint");
            if (firePoint == null)
            {
                firePoint = root.transform.Find("FirePoint");
                if (firePoint != null)
                    firePoint.SetParent(gunPivot, false);
            }
            if (firePoint == null)
            {
                var fpGo = new GameObject("FirePoint");
                firePoint = fpGo.transform;
                firePoint.SetParent(gunPivot, false);
            }
            firePoint.localPosition = new Vector3(0.5f, 0f, 0f);

            Transform weaponTrans = gunPivot;

            Script_20_WeaponBasic weapon = EnsureComponent<Script_20_WeaponBasic>(gunPivot.gameObject);
            var soWeapon = new SerializedObject(weapon);
            soWeapon.FindProperty("_projectilePoolKey").stringValue         = "Projectile_Basic";
            soWeapon.FindProperty("_fireRate").floatValue                   = 0.15f;
            soWeapon.FindProperty("_firePoint").objectReferenceValue        = firePoint;
            soWeapon.ApplyModifiedPropertiesWithoutUndo();

            // Assign weapon to controller
            soCtrl = new SerializedObject(ctrl);
            soCtrl.FindProperty("_primaryWeapon").objectReferenceValue = weapon;
            soCtrl.ApplyModifiedPropertiesWithoutUndo();

            // ── Script_38_PlayerLoadout (weapon manager) ──────────────────────

            Script_38_PlayerLoadout loadout = EnsureComponent<Script_38_PlayerLoadout>(root);
            var soLoadout = new SerializedObject(loadout);
            soLoadout.FindProperty("_weaponAttachPoint").objectReferenceValue = firePoint;
            soLoadout.ApplyModifiedPropertiesWithoutUndo();

            // ── Wire _gunPivot into controller ────────────────────────────────

            soCtrl = new SerializedObject(ctrl);
            soCtrl.FindProperty("_gunPivot").objectReferenceValue = gunPivot;
            soCtrl.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─────────────────────────────────────────────────────────────────────

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static Sprite[] LoadSprites(string path)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = all.OfType<Sprite>().ToList();

            sprites.Sort((a, b) =>
            {
                int na = ParseTrailingNumber(a.name);
                int nb = ParseTrailingNumber(b.name);
                return na.CompareTo(nb);
            });

            if (sprites.Count == 0)
                Debug.LogWarning($"[PlayerPrefabFixer] No se encontraron sprites en {path}");
            else
                Debug.Log($"[PlayerPrefabFixer] Cargados {sprites.Count} frames de {System.IO.Path.GetFileName(path)}");

            return sprites.ToArray();
        }

        private static int ParseTrailingNumber(string name)
        {
            int i = name.Length - 1;
            while (i >= 0 && char.IsDigit(name[i])) i--;
            string num = name.Substring(i + 1);
            return num.Length > 0 ? int.Parse(num) : 0;
        }

        private static void SetSpriteArray(SerializedObject so, string propertyName, Sprite[] sprites)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"[PlayerPrefabFixer] Propiedad '{propertyName}' no encontrada.");
                return;
            }
            prop.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }
    }
}
