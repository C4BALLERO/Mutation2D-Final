using MutationSwarm.Combat;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using MutationSwarm.Evolution;
using MutationSwarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Tools -> Mutation Swarm -> Panel de Control
    /// 1. Re-slice enemy sprites  2. Create system prefabs  3. Setup Scene_02_GameWorld
    /// </summary>
    public class MutationSwarmControlPanel : EditorWindow
    {
        // ── Known asset GUIDs ──────────────────────────────────────────────
        const string SPAWN_CONFIG_GUID  = "b538d14e170048e292ebb97ad23d8b48"; // SO_EnemySpawnConfig_Art
        const string WAVE_CONFIG_GUID   = "13dafb5aea3bca141a99eeaccdca15f9"; // SO_WaveConfig_Default
        const string PLAYER_PREFAB_GUID = "77e052be38321084b94749dbedc1e251"; // _Player.prefab
        const string HUD_UXML_GUID      = "a0c22b66346544445b152be2d1aed84e"; // HUD_Main.uxml
        const string SHOP_UXML_GUID     = "0eb24d64b1f76974aa5360974d60a5e1"; // WeaponShop.uxml

        const string SYSTEM_PREFABS_DIR = "Assets/_Prefabs/System";
        const string GAME_SCENE_PATH    = "Assets/_Scenes/Scene_02_GameWorld.unity";

        const string ENEMY_BASE_DIR     = "Assets/_Prefabs/Enemies";
        const string VARIANTS_DIR       = "Assets/_Prefabs/Enemies/Variants";

        // ── Window ──────────────────────────────────────────────────────────
        [MenuItem("Tools/Mutation Swarm/Panel de Control")]
        public static void ShowWindow() =>
            GetWindow<MutationSwarmControlPanel>("Mutation Swarm").minSize = new Vector2(340, 370);

        private void OnGUI()
        {
            var header  = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            var section = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };

            EditorGUILayout.LabelField("Mutation Swarm 2D — Panel de Control", header);
            EditorGUILayout.Space(10);

            // ── 1. Sprites ──────────────────────────────────────────────────
            EditorGUILayout.LabelField("1. Sprites Enemigos", section);
            EditorGUILayout.HelpBox(
                "Re-corta los sprite sheets y wirea animaciones en los 4 prefabs de enemigos.",
                MessageType.None);
            if (GUILayout.Button("Re-Slice y Setup Animaciones Enemigas", GUILayout.Height(32)))
                EnemyPrefabAnimFixer.ResliceAndSetupAll();

            EditorGUILayout.Space(12);

            // ── 2. Prefabs ──────────────────────────────────────────────────
            EditorGUILayout.LabelField("2. Prefabs del Sistema (arrastrables)", section);
            EditorGUILayout.HelpBox(
                "Crea Assets/_Prefabs/System/: Managers, WaveSystem, HUD, WeaponShop, Bootstrap, SpawnRing.",
                MessageType.None);
            if (GUILayout.Button("Crear / Actualizar Prefabs del Sistema", GUILayout.Height(32)))
                CreateSystemPrefabs();

            EditorGUILayout.Space(12);

            // ── 3. Scene ────────────────────────────────────────────────────
            EditorGUILayout.LabelField("3. Armar Escena de Juego", section);
            EditorGUILayout.HelpBox(
                "Instancia los prefabs del sistema en Scene_02_GameWorld y construye la arena.",
                MessageType.None);
            if (GUILayout.Button("Configurar Scene_02_GameWorld", GUILayout.Height(32)))
                SetupGameScene();

            EditorGUILayout.Space(12);

            // ── 4. Enemy Variants ───────────────────────────────────────────
            EditorGUILayout.LabelField("4. Variantes de Enemigos", section);
            EditorGUILayout.HelpBox(
                "Crea Rapido, Tanque y Elite para cada uno de los 4 enemigos (12 prefabs en Variants/).\nComparten las mismas animaciones que los prefabs base.",
                MessageType.None);
            if (GUILayout.Button("Crear Variantes de Enemigos", GUILayout.Height(32)))
                CreateEnemyVariants();
        }

        // ================================================================
        //  SYSTEM PREFABS
        // ================================================================
        static void CreateSystemPrefabs()
        {
            if (!AssetDatabase.IsValidFolder(SYSTEM_PREFABS_DIR))
                AssetDatabase.CreateFolder("Assets/_Prefabs", "System");

            CreateManagersPrefab();
            CreateWaveSystemPrefab();
            CreateHUDPrefab();
            CreateShopPrefab();
            CreateBootstrapPrefab();
            CreateSpawnRingPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] Prefabs creados en " + SYSTEM_PREFABS_DIR);
        }

        // _Managers: GameManager + CoinManager
        static void CreateManagersPrefab()
        {
            var root = new GameObject("_Managers");
            AddChild<Script_01_GameManager>(root, "GameManager");
            AddChild<Script_42_CoinManager>(root, "CoinManager");
            SaveAndDestroy(root, "Prefab_Managers");
        }

        // _WaveSystem: WaveManager (SO_WaveConfig + SO_EnemySpawnConfig pre-assigned)
        static void CreateWaveSystemPrefab()
        {
            var root     = new GameObject("_WaveSystem");
            var wm       = AddChild<Script_02_WaveManager>(root, "WaveManager");
            var spawnCfg = LoadByGuid<SO_EnemySpawnConfig>(SPAWN_CONFIG_GUID);
            var waveCfg  = LoadByGuid<SO_WaveConfig>(WAVE_CONFIG_GUID);

            var so = new SerializedObject(wm);
            if (spawnCfg != null) so.FindProperty("_spawnConfig").objectReferenceValue = spawnCfg;
            if (waveCfg  != null) so.FindProperty("_config").objectReferenceValue      = waveCfg;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveAndDestroy(root, "Prefab_WaveSystem");
        }

        // _HUD: UIDocument (HUD_Main.uxml) + HUDController (wired)
        static void CreateHUDPrefab()
        {
            var root   = new GameObject("_HUD");
            var doc    = root.AddComponent<UIDocument>();
            var hud    = root.AddComponent<Script_25_HUDController>();
            var xmlDoc = LoadByGuid<VisualTreeAsset>(HUD_UXML_GUID);

            SetField(doc, "m_SourceAsset", xmlDoc);
            SetField(hud, "_uiDocument",   doc);

            SaveAndDestroy(root, "Prefab_HUD");
        }

        // _WeaponShop: UIDocument + WeaponShopUI + WeaponShopManager (all wired together)
        static void CreateShopPrefab()
        {
            var root    = new GameObject("_WeaponShop");
            var doc     = root.AddComponent<UIDocument>();
            var shopUi  = root.AddComponent<Script_40_WeaponShopUI>();
            var shopMgr = root.AddComponent<Script_39_WeaponShopManager>();
            var xmlDoc  = LoadByGuid<VisualTreeAsset>(SHOP_UXML_GUID);

            SetField(doc,     "m_SourceAsset", xmlDoc);
            SetField(shopUi,  "_uiDocument",   doc);
            SetField(shopMgr, "_shopUi",       shopUi);

            SaveAndDestroy(root, "Prefab_WeaponShop");
        }

        // _Bootstrap: GameplayBootstrap with _Player.prefab assigned
        static void CreateBootstrapPrefab()
        {
            var root      = new GameObject("_Bootstrap");
            var bootstrap = root.AddComponent<Script_32_GameplayBootstrap>();
            var player    = LoadByGuid<GameObject>(PLAYER_PREFAB_GUID);

            SetField(bootstrap, "_playerPrefab", player);

            SaveAndDestroy(root, "Prefab_Bootstrap");
        }

        // _SpawnRing: 8 Script_39_SpawnPoint children on a circle of radius 10
        static void CreateSpawnRingPrefab()
        {
            var root  = new GameObject("_SpawnRing");
            int count = 8;
            float r   = 10f;

            for (int i = 0; i < count; i++)
            {
                float angle = 360f / count * i * Mathf.Deg2Rad;
                var sp = new GameObject($"SpawnPoint_{i + 1}");
                sp.AddComponent<Script_39_SpawnPoint>();
                sp.transform.SetParent(root.transform);
                sp.transform.localPosition = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
            }

            SaveAndDestroy(root, "Prefab_SpawnRing");
        }

        // ================================================================
        //  SCENE SETUP
        // ================================================================
        static void SetupGameScene()
        {
            var scene = EditorSceneManager.GetSceneByPath(GAME_SCENE_PATH);
            if (!scene.IsValid())
                scene = EditorSceneManager.OpenScene(GAME_SCENE_PATH, OpenSceneMode.Single);

            if (!scene.IsValid())
            {
                Debug.LogError("[MutationSwarm] No se encontro " + GAME_SCENE_PATH + ". Creala primero.");
                return;
            }

            // Check prefabs exist
            var prefabMgr   = LoadPrefab("Prefab_Managers");
            var prefabWave  = LoadPrefab("Prefab_WaveSystem");
            var prefabHUD   = LoadPrefab("Prefab_HUD");
            var prefabShop  = LoadPrefab("Prefab_WeaponShop");
            var prefabBoot  = LoadPrefab("Prefab_Bootstrap");
            var prefabSpawn = LoadPrefab("Prefab_SpawnRing");

            if (prefabMgr == null || prefabWave == null || prefabHUD == null ||
                prefabShop == null || prefabBoot == null || prefabSpawn == null)
            {
                Debug.LogWarning("[MutationSwarm] Primero crea los prefabs del sistema (boton 2).");
                return;
            }

            // Remove duplicates
            foreach (var name in new[] { "_Managers", "_WaveSystem", "_HUD", "_WeaponShop", "_Bootstrap", "_SpawnRing", "Arena" })
                RemoveExistingByName(name);

            // Instantiate
            PrefabUtility.InstantiatePrefab(prefabMgr);
            PrefabUtility.InstantiatePrefab(prefabWave);
            PrefabUtility.InstantiatePrefab(prefabHUD);
            PrefabUtility.InstantiatePrefab(prefabShop);
            PrefabUtility.InstantiatePrefab(prefabBoot);
            PrefabUtility.InstantiatePrefab(prefabSpawn);

            // Build arena walls
            BuildArena();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MutationSwarm] Scene_02_GameWorld configurada con todos los sistemas.");
        }

        static void BuildArena()
        {
            // 24x14 enclosed arena; player spawns near center
            var arena = new GameObject("Arena");
            CreateWall(arena, "Floor",     new Vector3(0f, -7.5f, 0f), new Vector2(26f, 1f));
            CreateWall(arena, "Ceiling",   new Vector3(0f,  7.5f, 0f), new Vector2(26f, 1f));
            CreateWall(arena, "WallLeft",  new Vector3(-13f, 0f, 0f),  new Vector2(1f, 16f));
            CreateWall(arena, "WallRight", new Vector3( 13f, 0f, 0f),  new Vector2(1f, 16f));
        }

        static void CreateWall(GameObject parent, string name, Vector3 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color  = new Color(0.22f, 0.22f, 0.25f, 1f);
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            go.AddComponent<BoxCollider2D>();
        }

        // ================================================================
        //  ENEMY VARIANTS
        // ================================================================

        // (nameSuffix, speedMult, hpMult, damageMult, attackRange)
        static readonly (string suffix, float spd, float hp, float dmg, float range)[] VariantDefs =
        {
            ("Rapido", 2.5f, 0.5f,  0.8f, 0.7f),  // fast, fragile
            ("Tanque", 0.55f, 3.0f, 1.8f, 1.1f),  // slow, heavy
            ("Elite",  1.5f, 2.0f,  1.5f, 1.0f),  // balanced upgrade
        };

        // (baseName, walk-frame-0 guid, walk-frame-0 fileID)
        static readonly (string name, string spriteGuid, long spriteFileId)[] BaseEnemies =
        {
            ("Enemi_Dino",      "bf34fa1f1347897409d8a7c5fdfa9dc9",  4151462045169555281L),
            ("Enemi_Mono",      "d3907c7ac9334e944aa4815fca678e64", -9042599199946578222L),
            ("Enemi_Diablito",  "69605e78c0d56b941968e96ac7fc7f2a",  3564749028224628192L),
            ("Enemi_3",         "c7a6d4c5d065d2f4884bdbfc69019f1c", -6578401572324881044L),
        };

        static void CreateEnemyVariants()
        {
            if (!AssetDatabase.IsValidFolder(VARIANTS_DIR))
                AssetDatabase.CreateFolder(ENEMY_BASE_DIR, "Variants");

            int created = 0;

            foreach (var (baseName, spriteGuid, spriteFileId) in BaseEnemies)
            {
                string basePath = $"{ENEMY_BASE_DIR}/{baseName}.prefab";
                var basePrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
                if (basePrefab == null)
                {
                    Debug.LogWarning($"[MutationSwarm] No se encontro {basePath}");
                    continue;
                }

                // Load walk-frame-0 sprite to use as editor preview sprite
                string spritePath = AssetDatabase.GUIDToAssetPath(spriteGuid);
                var frame0 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + $"[{spriteFileId}]");
                // Fallback: load first sprite sub-asset
                if (frame0 == null)
                {
                    var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritePath);
                    foreach (var sub in subs)
                        if (sub is Sprite s) { frame0 = s; break; }
                }

                foreach (var (suffix, spd, hp, dmg, range) in VariantDefs)
                {
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                    go.name = $"{baseName}_{suffix}";
                    PrefabUtility.UnpackPrefabInstance(go,
                        PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                    // Set editor-preview sprite to walk frame 0
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null && frame0 != null)
                    {
                        var srSo = new SerializedObject(sr);
                        srSo.FindProperty("m_Sprite").objectReferenceValue = frame0;
                        srSo.ApplyModifiedPropertiesWithoutUndo();
                    }

                    // Tweak EnemyBase stats
                    var eb = go.GetComponent<Script_13_EnemyBase>();
                    if (eb != null)
                    {
                        var so = new SerializedObject(eb);
                        float baseSpd = so.FindProperty("_baseSpeed").floatValue;
                        float baseHp  = so.FindProperty("_baseHp").floatValue;
                        float baseDmg = so.FindProperty("_baseDamage").floatValue;

                        so.FindProperty("_baseSpeed").floatValue   = baseSpd * spd;
                        so.FindProperty("_baseHp").floatValue      = baseHp  * hp;
                        so.FindProperty("_baseDamage").floatValue  = baseDmg * dmg;
                        so.FindProperty("_attackRange").floatValue = range;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }

                    string outPath = $"{VARIANTS_DIR}/{go.name}.prefab";
                    PrefabUtility.SaveAsPrefabAsset(go, outPath);
                    Object.DestroyImmediate(go);
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MutationSwarm] {created} variantes creadas en {VARIANTS_DIR}");
        }

        // ================================================================
        //  HELPERS
        // ================================================================
        static T AddChild<T>(GameObject parent, string childName) where T : Component
        {
            var go = new GameObject(childName);
            go.transform.SetParent(parent.transform);
            return go.AddComponent<T>();
        }

        static void SetField(Object target, string fieldName, Object value)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[MutationSwarm] Campo '{fieldName}' no encontrado en {target.GetType().Name}");
            }
        }

        static void SaveAndDestroy(GameObject root, string prefabName)
        {
            string path = $"{SYSTEM_PREFABS_DIR}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        static T LoadByGuid<T>(string guid) where T : Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
        }

        static GameObject LoadPrefab(string prefabName) =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"{SYSTEM_PREFABS_DIR}/{prefabName}.prefab");

        static void RemoveExistingByName(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }
    }
}
