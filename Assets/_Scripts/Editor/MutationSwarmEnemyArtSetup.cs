#if UNITY_EDITOR
using System.IO;
using MutationSwarm.Combat;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using UnityEditor;
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Genera sprites y prefabs de enemigos según design/art/art-bible.md
    /// </summary>
    public static class MutationSwarmEnemyArtSetup
    {
        private const string SpritesPath = "Assets/_Art/Sprites/Enemies";
        private const string PrefabsPath = "Assets/_Prefabs/Enemies";

        [MenuItem("Tools/Mutation Swarm/Build Enemy Sprites (Art Bible)")]
        public static void BuildAll()
        {
            EnsureFolder(SpritesPath);
            EnsureFolder(PrefabsPath);

            BuildArchetype(EnemySpriteFactory.Archetype.Drone, "Spr_Enemy_Drone", typeof(Script_13_EnemyBase),
                "Prefab_Enemy_Drone", 0.35f, 1f);
            BuildArchetype(EnemySpriteFactory.Archetype.Boss, "Spr_Enemy_Boss", typeof(Script_15_EnemyBoss),
                "Prefab_Enemy_Boss", 0.55f, 1.35f);
            BuildArchetype(EnemySpriteFactory.Archetype.Queen, "Spr_Enemy_Queen", typeof(Script_16_EnemyQueen),
                "Prefab_Enemy_Queen", 0.5f, 1.6f);
            BuildArchetype(EnemySpriteFactory.Archetype.Mimic, "Spr_Enemy_Mimic", typeof(Script_17_EnemyMimic),
                "Prefab_Enemy_Mimic", 0.38f, 1.05f);
            BuildArchetype(EnemySpriteFactory.Archetype.Parasite, "Spr_Enemy_Parasite", typeof(Script_18_EnemyParasite),
                "Prefab_Enemy_Parasite", 0.28f, 0.75f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] Sprites y prefabs de enemigos (art bible) generados en " + SpritesPath);
        }

        private static void BuildArchetype(
            EnemySpriteFactory.Archetype archetype,
            string spriteName,
            System.Type behaviourType,
            string prefabName,
            float colliderRadius,
            float scale)
        {
            int size = archetype == EnemySpriteFactory.Archetype.Parasite ? 56 : 64;
            Sprite sprite = EnemySpriteFactory.Create(archetype, size, 64f);
            string pngPath = $"{SpritesPath}/{spriteName}.png";
            SaveSpritePng(sprite, pngPath);
            Sprite imported = ConfigureImport(pngPath);

            GameObject go = CreateEnemyGameObject(imported, behaviourType, colliderRadius, scale,
                EnemySpriteFactory.GetDefaultTint(archetype));
            go.name = prefabName;

            string prefabPath = $"{PrefabsPath}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }

        private static void SaveSpritePng(Sprite sprite, string path)
        {
            byte[] png = sprite.texture.EncodeToPNG();
            Object.DestroyImmediate(sprite.texture);
            Object.DestroyImmediate(sprite);
            File.WriteAllBytes(path, png);
        }

        private static Sprite ConfigureImport(string path)
        {
            AssetDatabase.ImportAsset(path);
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spritePixelsPerUnit = 64;
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.alphaIsTransparency = true;
                imp.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateEnemyGameObject(
            Sprite sprite,
            System.Type behaviourType,
            float colliderRadius,
            float scale,
            Color defaultTint)
        {
            GameObject go = new("Enemy");
            go.layer = LayerMask.NameToLayer("Enemy");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = defaultTint;
            sr.sortingOrder = behaviourType == typeof(Script_16_EnemyQueen) ? 8
                : behaviourType == typeof(Script_15_EnemyBoss) ? 7 : 5;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = colliderRadius;

            // Animator component — controller is assigned by MutationSwarmEnemyAnimationSetup
            go.AddComponent<Animator>();

            Script_13_EnemyBase enemy = go.AddComponent(behaviourType) as Script_13_EnemyBase;
            if (enemy != null)
            {
                SerializedObject so = new(enemy);
                so.FindProperty("_playerMask").intValue = 1 << LayerMask.NameToLayer("Player");
                so.FindProperty("_enemyMask").intValue = 1 << LayerMask.NameToLayer("Enemy");
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (go.GetComponent<Script_22_StatusEffects>() == null)
                go.AddComponent<Script_22_StatusEffects>();

            go.transform.localScale = Vector3.one * scale;
            return go;
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
