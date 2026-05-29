#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MutationSwarm.Building;
using MutationSwarm.Combat;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using MutationSwarm.Evolution;
using MutationSwarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Genera escenas, prefabs, capas, ScriptableObjects y Build Settings del proyecto.
    /// Menú: Tools → Mutation Swarm → Setup Complete Project
    /// </summary>
    public static class MutationSwarmProjectSetup
    {
        private const string ScenesPath = "Assets/_Scenes";
        private const string PrefabsPath = "Assets/_Prefabs";
        private const string DataPath = "Assets/_Data";
        private const string SpritesPath = "Assets/_Art/Sprites/Environment";

        private static readonly string[] LayerNames =
        {
            "Player", "Enemy", "Projectile_Player", "Projectile_Enemy",
            "Platform", "BuildSurface", "Structure"
        };

        [MenuItem("Tools/Mutation Swarm/Setup Complete Project")]
        public static void SetupCompleteProject()
        {
            EnsureFolders();
            ConfigureLayers();
            ConfigurePhysics2D();
            Sprite placeholder = CreatePlaceholderSprite();
            CreateScriptableObjects(placeholder);
            CreatePrefabs(placeholder);
            CreateAllScenes(placeholder);
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] Setup completo: escenas, prefabs, capas y build settings.");
        }

        [InitializeOnLoadMethod]
        private static void AutoSetupOnFirstOpen()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                string bootScene = $"{ScenesPath}/Scene_00_Boot.unity";
                if (File.Exists(bootScene))
                    return;

                if (EditorUtility.DisplayDialog(
                        "Mutation Swarm",
                        "No se encontraron las escenas base (Scene_00_Boot). ¿Generar escenas, prefabs y configuración ahora?",
                        "Sí, generar",
                        "Después"))
                {
                    SetupCompleteProject();
                }
            };
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                ScenesPath, PrefabsPath, $"{PrefabsPath}/Player", $"{PrefabsPath}/Enemies",
                $"{PrefabsPath}/Projectiles", $"{PrefabsPath}/Structures", $"{PrefabsPath}/UI",
                DataPath, SpritesPath,
                "Assets/_ScriptableObjects/Waves", "Assets/_ScriptableObjects/Pools",
                "Assets/_ScriptableObjects/Building", "Assets/_ScriptableObjects/Combat"
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
                    string name = Path.GetFileName(folder);
                    if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
                        AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        private static void ConfigureLayers()
        {
            SerializedObject tagManager = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 0; i < LayerNames.Length; i++)
            {
                int layerIndex = 6 + i;
                SerializedProperty layer = layers.GetArrayElementAtIndex(layerIndex);
                layer.stringValue = LayerNames[i];
            }

            tagManager.ApplyModifiedProperties();
        }

        private static void ConfigurePhysics2D()
        {
            Physics2D.gravity = new Vector2(0f, -20f);
        }

        private static Sprite CreatePlaceholderSprite()
        {
            string path = $"{SpritesPath}/Spr_Placeholder_White.png";
            if (!File.Exists(path))
            {
                Texture2D tex = new(4, 4, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[16];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void CreateScriptableObjects(Sprite placeholder)
        {
            CreateAssetIfMissing<SO_WaveConfig>("Assets/_ScriptableObjects/Waves/SO_WaveConfig_Default.asset");
            CreateAssetIfMissing<SO_PoolConfig>("Assets/_ScriptableObjects/Pools/SO_Pool_Projectile_Basic.asset", so =>
            {
                so.poolKey = "Projectile_Basic";
                so.initialSize = 20;
                so.maxSize = 100;
            });

            CreateStructureSo("SO_Structure_Turret", StructureType.TurretBasic, "Torreta", 8, 30f, 5f, 1f);
            CreateStructureSo("SO_Structure_Barricade", StructureType.Barricade, "Barricada", 6, 0f, 0f, 0f);
            CreateStructureSo("SO_Structure_Trap", StructureType.FloorTrap, "Trampa", 4, 45f, 0f, 0f);
            CreateStructureSo("SO_Structure_Mine", StructureType.Mine, "Mina", 7, 0f, 2f, 0f);
            CreateStructureSo("SO_Structure_Platform", StructureType.TemporaryPlatform, "Plataforma", 5, 60f, 0f, 0f);

            CreateUpgradeSo("SO_Upgrade_ElectricAmmo", "Munición eléctrica", "+25% daño, cadena a 2 enemigos", 0.25f, "electric_ammo");
            CreateUpgradeSo("SO_Upgrade_ExplosiveDash", "Dash explosivo", "Dash daña en radio 1", 1f, "explosive_dash");
            CreateUpgradeSo("SO_Upgrade_TempShield", "Escudo temporal", "Absorbe 1 golpe al 0 HP", 1f, "temp_shield");
            CreateUpgradeSo("SO_Upgrade_Drone", "Dron compañero", "Dispara al enemigo cercano", 1f, "drone");
            CreateUpgradeSo("SO_Upgrade_Pierce", "Balas perforantes", "Atraviesa hasta 3 enemigos", 3f, "pierce");
            CreateUpgradeSo("SO_Upgrade_Regen", "Regeneración pasiva", "+2 HP/seg", 2f, "regen");
            CreateUpgradeSo("SO_Upgrade_FollowTurret", "Torreta personal", "Torreta que sigue al jugador", 1f, "follow_turret");
            CreateUpgradeSo("SO_Upgrade_Vampirism", "Vampirismo", "10% del daño convertido en HP", 0.1f, "vampirism");
        }

        private static void CreateStructureSo(string fileName, StructureType type, string displayName, int cost, float lifetime, float range, float fireRate)
        {
            string path = $"Assets/_ScriptableObjects/Building/{fileName}.asset";
            CreateAssetIfMissing<SO_StructureData>(path, so =>
            {
                so.structureType = type;
                so.structureName = displayName;
                so.materialCost = cost;
                so.lifetime = lifetime;
                so.range = range;
                so.fireRate = fireRate;
                so.maxHp = type == StructureType.Barricade ? 200f : 80f;
                so.projectilePoolKey = "Projectile_Basic";
            });
        }

        private static void CreateUpgradeSo(string fileName, string title, string desc, float value, string effectId)
        {
            string path = $"Assets/_ScriptableObjects/Combat/{fileName}.asset";
            CreateAssetIfMissing<SO_UpgradeData>(path, so =>
            {
                so.upgradeName = title;
                so.description = desc;
                so.numericEffect = value;
                so.effectId = effectId;
            });
        }

        private static void CreatePrefabs(Sprite sprite)
        {
            GameObject projectile = CreateProjectilePrefab(sprite);
            GameObject enemy = CreateEnemyPrefab(sprite);
            GameObject player = CreatePlayerPrefab(sprite, projectile);
            GameObject turret = CreateTurretPrefab(sprite, projectile);
            GameObject barricade = CreateBarricadePrefab(sprite);

            SO_PoolConfig pool = AssetDatabase.LoadAssetAtPath<SO_PoolConfig>("Assets/_ScriptableObjects/Pools/SO_Pool_Projectile_Basic.asset");
            if (pool != null)
                pool.prefab = projectile;

            LinkStructurePrefab("SO_Structure_Turret", turret);
            LinkStructurePrefab("SO_Structure_Barricade", barricade);

            PrefabUtility.SaveAsPrefabAsset(projectile, $"{PrefabsPath}/Projectiles/Prefab_Projectile_Basic.prefab");
            PrefabUtility.SaveAsPrefabAsset(enemy, $"{PrefabsPath}/Enemies/Prefab_Enemy_Drone.prefab");
            PrefabUtility.SaveAsPrefabAsset(player, $"{PrefabsPath}/Player/Prefab_Player.prefab");
            PrefabUtility.SaveAsPrefabAsset(turret, $"{PrefabsPath}/Structures/Prefab_Structure_Turret.prefab");
            PrefabUtility.SaveAsPrefabAsset(barricade, $"{PrefabsPath}/Structures/Prefab_Structure_Barricade.prefab");
        }

        private static GameObject CreateProjectilePrefab(Sprite sprite)
        {
            GameObject go = new("Prefab_Projectile_Basic");
            go.layer = LayerMask.NameToLayer("Projectile_Player");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 0.9f, 0.2f);
            sr.sortingOrder = 10;
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.12f;
            go.AddComponent<Script_19_Projectile>();
            return go;
        }

        private static GameObject CreateEnemyPrefab(Sprite sprite)
        {
            GameObject go = new("Prefab_Enemy_Drone");
            go.layer = LayerMask.NameToLayer("Enemy");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.9f, 0.2f, 0.3f);
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            Script_13_EnemyBase enemy = go.AddComponent<Script_13_EnemyBase>();
            SerializedObject soEnemy = new(enemy);
            soEnemy.FindProperty("_playerMask").intValue = 1 << LayerMask.NameToLayer("Player");
            soEnemy.FindProperty("_enemyMask").intValue = 1 << LayerMask.NameToLayer("Enemy");
            soEnemy.ApplyModifiedPropertiesWithoutUndo();
            go.AddComponent<Script_22_StatusEffects>();
            return go;
        }

        private static GameObject CreatePlayerPrefab(Sprite sprite, GameObject projectilePrefab)
        {
            GameObject go = new("Prefab_Player");
            go.layer = LayerMask.NameToLayer("Player");
            go.tag = "Player";
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.3f, 0.8f, 1f);
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.6f, 0.9f);
            CapsuleCollider2D ground = go.AddComponent<CapsuleCollider2D>();
            ground.isTrigger = true;
            ground.size = new Vector2(0.5f, 0.2f);
            ground.offset = new Vector2(0f, -0.45f);
            go.AddComponent<Script_12_PlayerStats>();
            Script_11_PlayerController controller = go.AddComponent<Script_11_PlayerController>();

            GameObject groundCheck = new("GroundCheck");
            groundCheck.transform.SetParent(go.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            GameObject firePoint = new("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0.4f, 0.1f, 0f);

            GameObject weaponPrimary = new("Weapon_Primary");
            weaponPrimary.transform.SetParent(go.transform);
            Script_20_WeaponBasic weapon = weaponPrimary.AddComponent<Script_20_WeaponBasic>();
            SerializedObject soWeapon = new(weapon);
            soWeapon.FindProperty("_firePoint").objectReferenceValue = firePoint.transform;
            soWeapon.FindProperty("_projectilePoolKey").stringValue = "Projectile_Basic";
            soWeapon.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soController = new(controller);
            soController.FindProperty("_groundCheckPoint").objectReferenceValue = groundCheck.transform;
            soController.FindProperty("_primaryWeapon").objectReferenceValue = weapon;
            soController.FindProperty("_groundMask").intValue = 1 << LayerMask.NameToLayer("Platform");
            soController.FindProperty("_wallMask").intValue = 1 << LayerMask.NameToLayer("Platform");
            soController.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        private static GameObject CreateTurretPrefab(Sprite sprite, GameObject projectilePrefab)
        {
            GameObject go = new("Prefab_Structure_Turret");
            go.layer = LayerMask.NameToLayer("Structure");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.7f, 0.7f, 0.75f);
            GameObject pivot = new("Pivot");
            pivot.transform.SetParent(go.transform);
            GameObject firePoint = new("FirePoint");
            firePoint.transform.SetParent(pivot.transform);
            firePoint.transform.localPosition = new Vector3(0.4f, 0f, 0f);
            TurretStructure turret = go.AddComponent<TurretStructure>();
            SerializedObject so = new(turret);
            so.FindProperty("_pivot").objectReferenceValue = pivot.transform;
            so.FindProperty("_firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static GameObject CreateBarricadePrefab(Sprite sprite)
        {
            GameObject go = new("Prefab_Structure_Barricade");
            go.layer = LayerMask.NameToLayer("Structure");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.55f, 0.45f, 0.35f);
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 0.6f);
            go.AddComponent<BarricadeStructure>();
            return go;
        }

        private static void LinkStructurePrefab(string soName, GameObject prefab)
        {
            string path = $"Assets/_ScriptableObjects/Building/{soName}.asset";
            SO_StructureData data = AssetDatabase.LoadAssetAtPath<SO_StructureData>(path);
            if (data != null)
            {
                data.prefab = prefab;
                EditorUtility.SetDirty(data);
            }
        }

        private static void CreateAllScenes(Sprite sprite)
        {
            CreateBootScene();
            CreateMainMenuScene();
            // Si existe el builder de arte, preferir escena ya generada con paquete visual.
            string artScene = $"{ScenesPath}/Scene_02_GameWorld.unity";
            if (!File.Exists(artScene))
                CreateGameWorldScene(sprite);
            CreateUpgradeMenuScene();
        }

        private static void CreateBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject boot = new("_Boot");
            boot.AddComponent<Script_00_BootLoader>();
            CreateManager<Script_01_GameManager>("_GameManager");
            CreateManager<Script_05_SaveManager>("_SaveManager");
            CreateManager<Script_06_InputManager>("_InputManager");
            CreateManager<Script_AudioManager>("_AudioManager");
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Scene_00_Boot.unity");
        }

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.05f, 0.05f, 0.07f));
            GameObject ui = new("UI_MainMenu");
            UIDocument doc = ui.AddComponent<UIDocument>();
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Scripts/UI/MainMenu.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Scripts/UI/MainMenu.uss");
            doc.visualTreeAsset = uxml;
            if (uss != null)
                doc.rootVisualElement.styleSheets.Add(uss);
            ui.AddComponent<Script_30_MainMenuController>();
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Scene_01_MainMenu.unity");
        }

        private static void CreateGameWorldScene(Sprite sprite)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.05f, 0.05f, 0.07f), 5.4f);

            GameObject managers = new("_Managers");
            CreateChildManager(managers, "GameManager", typeof(Script_01_GameManager));
            CreateChildManager(managers, "WaveManager", typeof(Script_02_WaveManager));
            CreateChildManager(managers, "EvolutionEngine", typeof(Script_07_EvolutionEngine));
            CreateChildManager(managers, "ObjectPoolManager", typeof(Script_04_ObjectPool));
            CreateChildManager(managers, "BuildManager", typeof(Script_23_BuildManager));
            CreateChildManager(managers, "AudioManager", typeof(Script_AudioManager));

            GameObject environment = new("_Environment");
            CreatePlatform(environment.transform, "Platform_Ground", new Vector2(0f, -4f), new Vector2(18f, 1f), sprite);
            CreatePlatform(environment.transform, "Platform_Left_High", new Vector2(-5f, 1.5f), new Vector2(4f, 0.5f), sprite);
            CreatePlatform(environment.transform, "Platform_Center", new Vector2(0f, 0.5f), new Vector2(4f, 0.5f), sprite);
            CreatePlatform(environment.transform, "Platform_Right_High", new Vector2(5f, 1.5f), new Vector2(4f, 0.5f), sprite);
            CreatePlatform(environment.transform, "Platform_Left_Mid", new Vector2(-6.5f, -1.5f), new Vector2(3f, 0.5f), sprite);
            CreatePlatform(environment.transform, "Platform_Right_Mid", new Vector2(6.5f, -1.5f), new Vector2(3f, 0.5f), sprite);
            CreateWall(environment.transform, "Wall_Left", new Vector2(-9.5f, 0f), new Vector2(0.5f, 10f), sprite);
            CreateWall(environment.transform, "Wall_Right", new Vector2(9.5f, 0f), new Vector2(0.5f, 10f), sprite);
            CreateWall(environment.transform, "Ceiling", new Vector2(0f, 4.5f), new Vector2(20f, 0.5f), sprite);

            GameObject spawnPoints = new("_SpawnPoints");
            spawnPoints.AddComponent<Script_SpawnPointGizmos>();
            CreateSpawn(spawnPoints.transform, "sp_left_top", new Vector2(-9f, 3f));
            CreateSpawn(spawnPoints.transform, "sp_left_mid", new Vector2(-9f, 0f));
            CreateSpawn(spawnPoints.transform, "sp_left_bot", new Vector2(-9f, -3.5f));
            CreateSpawn(spawnPoints.transform, "sp_right_top", new Vector2(9f, 3f));
            CreateSpawn(spawnPoints.transform, "sp_right_mid", new Vector2(9f, 0f));
            CreateSpawn(spawnPoints.transform, "sp_right_bot", new Vector2(9f, -3.5f));
            CreateSpawn(spawnPoints.transform, "sp_top_left", new Vector2(-4f, 4.5f));
            CreateSpawn(spawnPoints.transform, "sp_top_right", new Vector2(4f, 4.5f));
            CreateSpawn(spawnPoints.transform, "p1_spawn", new Vector2(-1f, -3f));
            CreateSpawn(spawnPoints.transform, "p2_spawn", new Vector2(1f, -3f));
            CreateSpawn(spawnPoints.transform, "p3_spawn", new Vector2(-2f, -3f));
            CreateSpawn(spawnPoints.transform, "p4_spawn", new Vector2(2f, -3f));

            GameObject playersRoot = new("_Players");
            GameObject enemiesRoot = new("_Enemies");
            GameObject structuresRoot = new("_Structures");

            GameObject ui = new("_UI");
            UIDocument hudDoc = ui.AddComponent<UIDocument>();
            hudDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Scripts/UI/HUD_Main.uxml");
            StyleSheet hudUss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Scripts/UI/HUD_Main.uss");
            if (hudUss != null)
                hudDoc.rootVisualElement.styleSheets.Add(hudUss);
            ui.AddComponent<Script_25_HUDController>();
            ui.AddComponent<Script_27_EvolutionDisplayUI>();

            // Referencias básicas de managers
            Script_23_BuildManager build = Object.FindFirstObjectByType<Script_23_BuildManager>();
            if (build != null)
            {
                SerializedObject so = new(build);
                so.FindProperty("_structuresRoot").objectReferenceValue = structuresRoot.transform;
                so.FindProperty("_buildSurfaceMask").intValue =
                    (1 << LayerMask.NameToLayer("BuildSurface")) | (1 << LayerMask.NameToLayer("Platform"));
                so.FindProperty("_structureMask").intValue = 1 << LayerMask.NameToLayer("Structure");

                SerializedProperty list = so.FindProperty("_availableStructures");
                list.ClearArray();
                string[] structureAssets =
                {
                    "SO_Structure_Turret", "SO_Structure_Barricade", "SO_Structure_Trap",
                    "SO_Structure_Mine", "SO_Structure_Platform"
                };
                for (int i = 0; i < structureAssets.Length; i++)
                {
                    SO_StructureData data = AssetDatabase.LoadAssetAtPath<SO_StructureData>(
                        $"Assets/_ScriptableObjects/Building/{structureAssets[i]}.asset");
                    if (data == null)
                        continue;
                    list.InsertArrayElementAtIndex(i);
                    list.GetArrayElementAtIndex(i).objectReferenceValue = data;
                }

                Transform[] spawns =
                {
                    spawnPoints.transform.Find("sp_left_top"),
                    spawnPoints.transform.Find("sp_left_mid"),
                    spawnPoints.transform.Find("sp_right_top")
                };
                so.FindProperty("_enemySpawnPoints").arraySize = spawns.Length;
                for (int i = 0; i < spawns.Length; i++)
                    so.FindProperty("_enemySpawnPoints").GetArrayElementAtIndex(i).objectReferenceValue = spawns[i];

                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Script_04_ObjectPool pool = Object.FindFirstObjectByType<Script_04_ObjectPool>();
            if (pool != null)
            {
                SerializedObject poolSo = new(pool);
                SerializedProperty configs = poolSo.FindProperty("_poolConfigs");
                configs.ClearArray();
                SO_PoolConfig projectilePool = AssetDatabase.LoadAssetAtPath<SO_PoolConfig>(
                    "Assets/_ScriptableObjects/Pools/SO_Pool_Projectile_Basic.asset");
                if (projectilePool != null)
                {
                    configs.InsertArrayElementAtIndex(0);
                    configs.GetArrayElementAtIndex(0).objectReferenceValue = projectilePool;
                }
                poolSo.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Scene_02_GameWorld.unity");
        }

        private static void CreateUpgradeMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject ui = new("UI_UpgradeMenu");
            UIDocument doc = ui.AddComponent<UIDocument>();
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Scripts/UI/UpgradeMenu.uxml");
            Script_26_UpgradeMenuUI upgrade = ui.AddComponent<Script_26_UpgradeMenuUI>();
            List<SO_UpgradeData> pool = new();
            string[] guids = AssetDatabase.FindAssets("t:SO_UpgradeData", new[] { "Assets/_ScriptableObjects/Combat" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SO_UpgradeData data = AssetDatabase.LoadAssetAtPath<SO_UpgradeData>(path);
                if (data != null)
                    pool.Add(data);
            }

            SerializedObject so = new(upgrade);
            so.FindProperty("_upgradePool").ClearArray();
            for (int i = 0; i < pool.Count; i++)
            {
                so.FindProperty("_upgradePool").InsertArrayElementAtIndex(i);
                so.FindProperty("_upgradePool").GetArrayElementAtIndex(i).objectReferenceValue = pool[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Scene_03_UpgradeMenu.unity");
        }

        private static void ConfigureBuildSettings()
        {
            string[] scenes =
            {
                $"{ScenesPath}/Scene_00_Boot.unity",
                $"{ScenesPath}/Scene_01_MainMenu.unity",
                $"{ScenesPath}/Scene_02_GameWorld.unity",
                $"{ScenesPath}/Scene_03_UpgradeMenu.unity"
            };

            List<EditorBuildSettingsScene> buildScenes = new();
            foreach (string scenePath in scenes)
            {
                if (!File.Exists(scenePath))
                    continue;
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        private static void CreateManager<T>(string name) where T : Component
        {
            GameObject go = new(name);
            go.AddComponent<T>();
        }

        private static void CreateChildManager(GameObject parent, string name, System.Type type)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent.transform);
            child.AddComponent(type);
        }

        private static void CreateCamera(Color bg, float orthoSize = 5f)
        {
            GameObject camGo = new("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.backgroundColor = bg;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
        }

        private static void CreatePlatform(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.layer = LayerMask.NameToLayer("Platform");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.25f, 0.28f, 0.32f);
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            PlatformEffector2D effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
        }

        private static void CreateWall(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.layer = LayerMask.NameToLayer("Platform");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.15f, 0.15f, 0.18f, 0.5f);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
        }

        private static void CreateSpawn(Transform parent, string name, Vector2 pos)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
        }

        private static void CreateAssetIfMissing<T>(string path, System.Action<T> configure = null) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                configure?.Invoke(asset);
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                configure?.Invoke(asset);
                EditorUtility.SetDirty(asset);
            }
        }
    }
}
#endif
