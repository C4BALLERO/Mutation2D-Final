#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MutationSwarm.Combat;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using MutationSwarm.UI;
using UnityEditor;
using UnityEngine;

namespace MutationSwarm.Editor
{
    public static class MutationSwarmGunsImport
    {
        private const string PackRoot = "Assets/_Art/GunsPack/Guns_V1.01 - Commission - Copy";
        private const string GunsFolder = PackRoot + "/01 - Individual sprites/Guns";
        private const string AmmoRoot = PackRoot + "/01 - Individual sprites/Bullets & Ammo";
        private const string WeaponsSoPath = "Assets/_ScriptableObjects/Combat/Weapons";
        private const string ProjectilesPath = "Assets/_Prefabs/Projectiles";
        private const string PlayerPrefabsPath = "Assets/_Prefabs/Player";

        private struct GunDef
        {
            public string id;
            public string fileName;
            public string ammoFolder;
            public string bulletFile;
            public int cost;
            public float fireRate;
            public float damage;
            public float speed;
            public bool starter;
            public string desc;
        }

        private static readonly GunDef[] Guns =
        {
            new() { id = "glock_p80", fileName = "Glock - P80 [64x48].png", ammoFolder = "Glock - P80", bulletFile = "Bullet.png",
                cost = 0, fireRate = 0.22f, damage = 9f, speed = 15f, starter = true, desc = "Pistola rápida, ideal para empezar." },
            new() { id = "revolver_colt", fileName = "Revolver - Colt 45 [64x32].png", ammoFolder = "Revolver - Colt 45", bulletFile = "Bullet.png",
                cost = 18, fireRate = 0.55f, damage = 24f, speed = 16f, starter = false, desc = "Alto daño, cadencia lenta." },
            new() { id = "mp5a3", fileName = "Submachine - MP5A3 [80x48].png", ammoFolder = "Submachine - MP5A3", bulletFile = "Bullet.png",
                cost = 35, fireRate = 0.09f, damage = 7f, speed = 14f, starter = false, desc = "Ráfaga constante." },
            new() { id = "ak47", fileName = "AK 47 [96x48].png", ammoFolder = "AK 47", bulletFile = "Bullet.png",
                cost = 50, fireRate = 0.11f, damage = 13f, speed = 18f, starter = false, desc = "Rifle de asalto equilibrado." },
            new() { id = "bazooka_m20", fileName = "Bazooka - M20 [192x32].png", ammoFolder = "Bazooka - M20 - Copy", bulletFile = "M20 Rocket.png",
                cost = 75, fireRate = 1.1f, damage = 48f, speed = 9f, starter = false, desc = "Explosivo pesado, proyectil lento." },
            new() { id = "bazooka_thick", fileName = "Thick Bazooka - M20 [192x32].png", ammoFolder = "Thick Bazooka - M20", bulletFile = "M20 Thick Rocket.png",
                cost = 110, fireRate = 1.4f, damage = 72f, speed = 8f, starter = false, desc = "Máximo daño, recarga larga." },
        };

        [MenuItem("Tools/Mutation Swarm/Import Guns Pack + Weapon Shop")]
        public static void ImportAll()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Art/GunsPack"))
            {
                Debug.LogError("[GunsImport] Copia primero la carpeta a Assets/_Art/GunsPack");
                return;
            }

            string scenePath = "Assets/_Scenes/Scene_02_GameWorld.unity";
            if (System.IO.File.Exists(scenePath))
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

            EnsureFolder(WeaponsSoPath);
            EnsureFolder(ProjectilesPath);
            ConfigureGunTextures();

            List<SO_WeaponData> catalog = new();
            foreach (GunDef def in Guns)
            {
                SO_WeaponData data = CreateWeaponAsset(def);
                if (data != null)
                    catalog.Add(data);
            }

            CreateShopInScene(catalog);
            RegisterPoolsOnScene(catalog);
            WirePlayerPrefabs();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[GunsImport] {catalog.Count} armas importadas y tienda configurada.");
        }

        private static void ConfigureGunTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { PackRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null)
                    continue;

                imp.textureType = TextureImporterType.Sprite;
                imp.spritePixelsPerUnit = 32;
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.alphaIsTransparency = true;
                imp.SaveAndReimport();
            }
        }

        private static SO_WeaponData CreateWeaponAsset(GunDef def)
        {
            string gunPath = $"{GunsFolder}/{def.fileName}";
            string bulletPath = $"{AmmoRoot}/{def.ammoFolder}/{def.bulletFile}";
            Sprite gun = AssetDatabase.LoadAssetAtPath<Sprite>(gunPath);
            Sprite bullet = AssetDatabase.LoadAssetAtPath<Sprite>(bulletPath);

            if (gun == null)
            {
                Debug.LogWarning($"[GunsImport] Falta sprite: {gunPath}");
                return null;
            }

            string poolKey = $"Projectile_{def.id}";
            CreateProjectilePrefab(poolKey, bullet, def);

            string assetPath = $"{WeaponsSoPath}/SO_Weapon_{def.id}.asset";
            SO_WeaponData data = AssetDatabase.LoadAssetAtPath<SO_WeaponData>(assetPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SO_WeaponData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }

            data.weaponId = def.id;
            data.displayName = def.fileName.Split('[')[0].Trim();
            data.description = def.desc;
            data.gunSprite = gun;
            data.projectileSprite = bullet;
            data.fireRate = def.fireRate;
            data.damage = def.damage;
            data.projectileSpeed = def.speed;
            data.projectileLifetime = def.id.Contains("bazooka") ? 4f : 3f;
            data.projectilePoolKey = poolKey;
            data.materialCost = def.cost;
            data.unlockedByDefault = def.starter;
            data.gunOffset = new Vector2(0.4f, 0.02f);

            EditorUtility.SetDirty(data);
            return data;
        }

        private static void CreateProjectilePrefab(string poolKey, Sprite bullet, GunDef def)
        {
            string path = $"{ProjectilesPath}/Prefab_{poolKey}.prefab";
            GameObject go = new($"Prefab_{poolKey}");
            go.layer = LayerMask.NameToLayer("Projectile_Player");
            if (go.layer < 0)
                go.layer = 0;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bullet;
            sr.sortingOrder = 15;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = def.id.Contains("bazooka") ? 0.2f : 0.12f;

            Script_19_Projectile proj = go.AddComponent<Script_19_Projectile>();
            SerializedObject so = new(proj);
            so.FindProperty("_poolKey").stringValue = poolKey;
            so.FindProperty("_damage").floatValue = def.damage;
            so.FindProperty("_speed").floatValue = def.speed;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            string poolAsset = $"Assets/_ScriptableObjects/Pools/SO_Pool_{poolKey}.asset";
            SO_PoolConfig pool = AssetDatabase.LoadAssetAtPath<SO_PoolConfig>(poolAsset);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<SO_PoolConfig>();
                AssetDatabase.CreateAsset(pool, poolAsset);
            }

            pool.poolKey = poolKey;
            pool.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            pool.initialSize = 16;
            pool.maxSize = 64;
            EditorUtility.SetDirty(pool);
        }

        private static void CreateShopInScene(List<SO_WeaponData> catalog)
        {
            GameObject shop = GameObject.Find("_WeaponShop");
            if (shop == null)
                shop = new GameObject("_WeaponShop");

            Script_39_WeaponShopManager manager = shop.GetComponent<Script_39_WeaponShopManager>();
            if (manager == null)
                manager = shop.AddComponent<Script_39_WeaponShopManager>();

            Script_40_WeaponShopUI ui = shop.GetComponent<Script_40_WeaponShopUI>();
            if (ui == null)
                ui = shop.AddComponent<Script_40_WeaponShopUI>();

            UnityEngine.UIElements.UIDocument doc = shop.GetComponent<UnityEngine.UIElements.UIDocument>();
            if (doc == null)
                doc = shop.AddComponent<UnityEngine.UIElements.UIDocument>();

            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(
                "Assets/_Scripts/UI/WeaponShop.uxml");
            UnityEngine.UIElements.StyleSheet sheet = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>(
                "Assets/_Scripts/UI/WeaponShop.uss");
            if (sheet != null)
                doc.rootVisualElement.styleSheets.Add(sheet);

            SerializedObject soM = new(manager);
            SerializedProperty list = soM.FindProperty("_catalog");
            list.ClearArray();
            for (int i = 0; i < catalog.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).objectReferenceValue = catalog[i];
            }

            soM.FindProperty("_shopUi").objectReferenceValue = ui;
            soM.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soUi = new(ui);
            soUi.FindProperty("_uiDocument").objectReferenceValue = doc;
            soUi.ApplyModifiedPropertiesWithoutUndo();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        private static void RegisterPoolsOnScene(List<SO_WeaponData> catalog)
        {
            Script_04_ObjectPool pool = Object.FindFirstObjectByType<Script_04_ObjectPool>();
            if (pool == null)
                return;

            SerializedObject so = new(pool);
            SerializedProperty configs = so.FindProperty("_poolConfigs");
            HashSet<string> existing = new();
            for (int i = 0; i < configs.arraySize; i++)
            {
                SO_PoolConfig c = configs.GetArrayElementAtIndex(i).objectReferenceValue as SO_PoolConfig;
                if (c != null)
                    existing.Add(c.poolKey);
            }

            foreach (SO_WeaponData w in catalog)
            {
                if (w == null || existing.Contains(w.projectilePoolKey))
                    continue;

                string poolAsset = $"Assets/_ScriptableObjects/Pools/SO_Pool_{w.projectilePoolKey}.asset";
                SO_PoolConfig cfg = AssetDatabase.LoadAssetAtPath<SO_PoolConfig>(poolAsset);
                if (cfg == null)
                    continue;

                int idx = configs.arraySize;
                configs.InsertArrayElementAtIndex(idx);
                configs.GetArrayElementAtIndex(idx).objectReferenceValue = cfg;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePlayerPrefabs()
        {
            WirePlayerPrefab($"{PlayerPrefabsPath}/Prefab_Player.prefab");
            WirePlayerPrefab($"{PlayerPrefabsPath}/Prefab_Player_Geo.prefab");
        }

        private static void WirePlayerPrefab(string path)
        {
            if (!File.Exists(path))
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
                return;

            Script_38_PlayerLoadout loadout = root.GetComponent<Script_38_PlayerLoadout>();
            if (loadout == null)
                loadout = root.AddComponent<Script_38_PlayerLoadout>();

            SO_WeaponData starter = AssetDatabase.LoadAssetAtPath<SO_WeaponData>(
                $"{WeaponsSoPath}/SO_Weapon_glock_p80.asset");
            SerializedObject soL = new(loadout);
            soL.FindProperty("_startingWeapon").objectReferenceValue = starter;
            soL.FindProperty("_weaponAttachPoint").objectReferenceValue = root.transform;
            soL.ApplyModifiedPropertiesWithoutUndo();

            Transform legacyWeapon = root.transform.Find("Weapon_Primary");
            if (legacyWeapon != null)
                Object.DestroyImmediate(legacyWeapon.gameObject);

            if (root.TryGetComponent(out Script_11_PlayerController controller))
            {
                SerializedObject soC = new(controller);
                soC.FindProperty("_primaryWeapon").objectReferenceValue = null;
                soC.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
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
