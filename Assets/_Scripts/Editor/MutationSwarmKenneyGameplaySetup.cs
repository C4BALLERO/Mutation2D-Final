#if UNITY_EDITOR
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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIButton = UnityEngine.UI.Button;
using UIImage = UnityEngine.UI.Image;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// UI Kenney clara + nivel geométrico jugable + menú con botones.
    /// </summary>
    public static class MutationSwarmKenneyGameplaySetup
    {
        private const string KenneyRoot = "Assets/_Art/kenney_ui-pack-space-expansion/Vector";
        private const string ScenesPath = "Assets/_Scenes";
        private const string PrefabsPath = "Assets/_Prefabs";
        private const string GeneratedSprites = "Assets/_Art/Sprites/Generated";

        [MenuItem("Tools/Mutation Swarm/Build Kenney UI + Playable Geometric Level")]
        public static void BuildAll()
        {
            MutationSwarmEnemyArtSetup.BuildAll();
            MutationSwarmPlayerArtSetup.BuildAll();
            ImportKenneySprites();
            EnsureFolder(GeneratedSprites);
            Sprite projectile = SaveGeneratedSprite("Spr_Geo_Projectile", GeometricSpriteFactory.Shape.Circle, new Color(0.95f, 0.95f, 0.3f), 32);

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Player/Prefab_Player.prefab");
            if (playerPrefab == null)
            {
                Sprite square = SaveGeneratedSprite("Spr_Geo_Square", GeometricSpriteFactory.Shape.Square, new Color(0.2f, 0.75f, 1f));
                playerPrefab = CreatePlayerPrefab(square, projectile);
            }
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Enemies/Prefab_Enemy_Drone.prefab");
            if (enemyPrefab == null)
            {
                Sprite circle = SaveGeneratedSprite("Spr_Geo_Circle", GeometricSpriteFactory.Shape.Circle, new Color(1f, 0.35f, 0.35f));
                enemyPrefab = CreateEnemyPrefab(circle);
            }
            GameObject projectilePrefab = CreateProjectilePrefab(projectile);

            SavePrefab(playerPrefab, $"{PrefabsPath}/Player/Prefab_Player_Geo.prefab");
            SavePrefab(enemyPrefab, $"{PrefabsPath}/Enemies/Prefab_Enemy_Geo.prefab");
            SavePrefab(projectilePrefab, $"{PrefabsPath}/Projectiles/Prefab_Projectile_Geo.prefab");

            BuildMainMenuScene();
            BuildPlayableLevelScene(playerPrefab, enemyPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] Kenney UI + nivel geométrico jugable listos.");
        }

        private static void ImportKenneySprites()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { KenneyRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".svg"))
                    continue;

                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null)
                    continue;

                imp.textureType = TextureImporterType.Sprite;
                imp.spritePixelsPerUnit = 100;
                imp.filterMode = FilterMode.Bilinear;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.SaveAndReimport();
            }
        }

        private static Sprite LoadKenney(string relative)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{KenneyRoot}/{relative}");
        }

        private static Sprite SaveGeneratedSprite(string name, GeometricSpriteFactory.Shape shape, Color color, int size = 64)
        {
            string path = $"{GeneratedSprites}/{name}.png";
            Sprite sprite = GeometricSpriteFactory.Create(shape, color, size, 64f);
            Texture2D tex = sprite.texture;
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(sprite);
            AssetDatabase.ImportAsset(path);
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spritePixelsPerUnit = 64;
                imp.filterMode = FilterMode.Point;
                imp.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreatePlayerPrefab(Sprite body, Sprite projSprite)
        {
            GameObject go = new("Prefab_Player_Geo");
            go.tag = "Player";
            go.layer = LayerMask.NameToLayer("Player");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = body;
            sr.sortingOrder = 10;
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.7f, 0.9f);
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
            firePoint.transform.localPosition = new Vector3(0.45f, 0f, 0f);

            GameObject weapon = new("Weapon_Primary");
            weapon.transform.SetParent(go.transform);
            Script_20_WeaponBasic wb = weapon.AddComponent<Script_20_WeaponBasic>();
            SerializedObject soW = new(wb);
            soW.FindProperty("_firePoint").objectReferenceValue = firePoint.transform;
            soW.FindProperty("_projectilePoolKey").stringValue = "Projectile_Geo";
            soW.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soC = new(controller);
            soC.FindProperty("_groundCheckPoint").objectReferenceValue = groundCheck.transform;
            soC.FindProperty("_primaryWeapon").objectReferenceValue = wb;
            int platformLayer = LayerMask.NameToLayer("Platform");
            soC.FindProperty("_groundMask").intValue = 1 << platformLayer;
            soC.FindProperty("_wallMask").intValue = 1 << platformLayer;
            soC.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        private static GameObject CreateEnemyPrefab(Sprite body)
        {
            GameObject go = new("Prefab_Enemy_Geo");
            go.layer = LayerMask.NameToLayer("Enemy");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = body;
            sr.sortingOrder = 5;
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            Script_13_EnemyBase enemy = go.AddComponent<Script_13_EnemyBase>();
            SerializedObject so = new(enemy);
            so.FindProperty("_playerMask").intValue = 1 << LayerMask.NameToLayer("Player");
            so.FindProperty("_enemyMask").intValue = 1 << LayerMask.NameToLayer("Enemy");
            so.ApplyModifiedPropertiesWithoutUndo();
            go.AddComponent<Script_22_StatusEffects>();
            return go;
        }

        private static GameObject CreateProjectilePrefab(Sprite sprite)
        {
            GameObject go = new("Prefab_Projectile_Geo");
            go.layer = LayerMask.NameToLayer("Projectile_Player");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 15;
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            Script_19_Projectile proj = go.AddComponent<Script_19_Projectile>();
            SerializedObject so = new(proj);
            so.FindProperty("_poolKey").stringValue = "Projectile_Geo";
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static void BuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = CreateCamera(new Color(0.75f, 0.88f, 1f));
            cam.clearFlags = CameraClearFlags.SolidColor;

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject es = new("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            Sprite panel = LoadKenney("Extra/panel_glass.svg");
            Sprite btnSprite = LoadKenney("Blue/button_square_header_large_rectangle.svg");
            Sprite btnPlay = LoadKenney("Green/button_square_header_large_rectangle.svg");
            Sprite btnSmall = LoadKenney("Yellow/bar_round_gloss_small_m.svg");

            GameObject canvasGo = new("Canvas_MainMenu");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject panelGo = CreateUIImage(canvasGo.transform, "Panel", panel, new Vector2(0, 0), new Vector2(520, 420));
            UIImage panelImg = panelGo.GetComponent<UIImage>();
            panelImg.color = new Color(1f, 1f, 1f, 0.92f);

            CreateUIText(panelGo.transform, "Title", "MUTATION SWARM", 42, new Vector2(0, 140), new Color(0.1f, 0.25f, 0.45f));
            CreateUIText(panelGo.transform, "Subtitle", "Argos-9 — contén el enjambre", 18, new Vector2(0, 95), new Color(0.2f, 0.4f, 0.55f));

            GameObject playBtn = CreateUIButton(panelGo.transform, "BtnPlay", btnPlay ?? btnSprite, "JUGAR", new Vector2(0, 20), new Vector2(280, 56));
            GameObject quitBtn = CreateUIButton(panelGo.transform, "BtnQuit", btnSprite, "SALIR", new Vector2(0, -50), new Vector2(280, 56));
            GameObject minusBtn = CreateUIButton(panelGo.transform, "BtnPlayersMinus", btnSmall, "-", new Vector2(-90, -120), new Vector2(56, 56));
            GameObject plusBtn = CreateUIButton(panelGo.transform, "BtnPlayersPlus", btnSmall, "+", new Vector2(90, -120), new Vector2(56, 56));
            GameObject lblGo = CreateUIText(panelGo.transform, "LblPlayers", "1 Jugador(es)", 22, new Vector2(0, -120), new Color(0.15f, 0.35f, 0.5f));

            Script_33_KenneyMainMenuUGUI menu = canvasGo.AddComponent<Script_33_KenneyMainMenuUGUI>();
            SerializedObject soMenu = new(menu);
            soMenu.FindProperty("_btnPlay").objectReferenceValue = playBtn.GetComponent<UIButton>();
            soMenu.FindProperty("_btnQuit").objectReferenceValue = quitBtn.GetComponent<UIButton>();
            soMenu.FindProperty("_btnPlus").objectReferenceValue = plusBtn.GetComponent<UIButton>();
            soMenu.FindProperty("_btnMinus").objectReferenceValue = minusBtn.GetComponent<UIButton>();
            soMenu.FindProperty("_lblPlayers").objectReferenceValue = lblGo.GetComponent<Text>();
            soMenu.ApplyModifiedPropertiesWithoutUndo();

            if (Object.FindFirstObjectByType<Script_01_GameManager>() == null)
            {
                GameObject gm = new("_GameManager");
                gm.AddComponent<Script_01_GameManager>();
            }

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Scene_01_MainMenu.unity");
        }

        private static void BuildPlayableLevelScene(GameObject playerPrefab, GameObject enemyPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.82f, 0.92f, 1f));

            GameObject managers = new("_Managers");
            CreateChild(managers, "GameManager", typeof(Script_01_GameManager));
            CreateChild(managers, "WaveManager", typeof(Script_02_WaveManager));
            CreateChild(managers, "EvolutionEngine", typeof(Script_07_EvolutionEngine));
            CreateChild(managers, "ObjectPoolManager", typeof(Script_04_ObjectPool));
            CreateChild(managers, "BuildManager", typeof(Script_23_BuildManager));
            CreateChild(managers, "AudioManager", typeof(Script_AudioManager));

            GameObject env = new("_Environment");
            Sprite platformSprite = LoadKenney("Yellow/bar_square_gloss_large.svg") ?? LoadKenney("Blue/bar_square_gloss_large_l.svg");

            BuildPlatform(env.transform, "Platform_Ground", new Vector2(0f, -4f), new Vector2(18f, 1.2f), platformSprite);
            BuildPlatform(env.transform, "Platform_Left_High", new Vector2(-5f, 1.5f), new Vector2(4f, 0.8f), platformSprite);
            BuildPlatform(env.transform, "Platform_Center", new Vector2(0f, 0.5f), new Vector2(4f, 0.8f), platformSprite);
            BuildPlatform(env.transform, "Platform_Right_High", new Vector2(5f, 1.5f), new Vector2(4f, 0.8f), platformSprite);
            BuildPlatform(env.transform, "Platform_Left_Mid", new Vector2(-6.5f, -1.5f), new Vector2(3f, 0.7f), platformSprite);
            BuildPlatform(env.transform, "Platform_Right_Mid", new Vector2(6.5f, -1.5f), new Vector2(3f, 0.7f), platformSprite);

            Sprite bgPanel = LoadKenney("Extra/panel_rectangle.svg");
            CreateDecorSprite(env.transform, "BG_Accent_Left", bgPanel, new Vector3(-8f, 2f, 0f), new Vector3(2f, 3f, 1f), -50);
            CreateDecorSprite(env.transform, "BG_Accent_Right", bgPanel, new Vector3(8f, 2f, 0f), new Vector3(2f, 3f, 1f), -50);

            GameObject spawns = new("_SpawnPoints");
            spawns.AddComponent<Script_SpawnPointGizmos>();
            Transform spLeftTop = CreateSpawn(spawns.transform, "sp_left_top", new Vector2(-9f, 3f));
            Transform spLeftMid = CreateSpawn(spawns.transform, "sp_left_mid", new Vector2(-9f, 0f));
            Transform spRightTop = CreateSpawn(spawns.transform, "sp_right_top", new Vector2(9f, 3f));
            Transform spRightMid = CreateSpawn(spawns.transform, "sp_right_mid", new Vector2(9f, 0f));
            Transform spRightBot = CreateSpawn(spawns.transform, "sp_right_bot", new Vector2(9f, -3.5f));
            Transform spLeftBot = CreateSpawn(spawns.transform, "sp_left_bot", new Vector2(-9f, -3.5f));
            Transform p1 = CreateSpawn(spawns.transform, "p1_spawn", new Vector2(-1f, -3f));

            new GameObject("_Players");
            new GameObject("_Enemies");
            new GameObject("_Structures");

            GameObject bootstrap = new("_GameplayBootstrap");
            Script_32_GameplayBootstrap boot = bootstrap.AddComponent<Script_32_GameplayBootstrap>();
            SerializedObject soBoot = new(boot);
            soBoot.FindProperty("_playerPrefab").objectReferenceValue = playerPrefab;
            soBoot.FindProperty("_playerSpawn").objectReferenceValue = p1;
            soBoot.ApplyModifiedPropertiesWithoutUndo();

            Script_02_WaveManager wave = Object.FindFirstObjectByType<Script_02_WaveManager>();
            if (wave != null)
            {
                SerializedObject soWave = new(wave);
                soWave.FindProperty("_enemyPrefab").objectReferenceValue = enemyPrefab;
                soWave.FindProperty("_spawnPoints").arraySize = 6;
                SetSpawn(soWave, 0, spLeftTop);
                SetSpawn(soWave, 1, spLeftMid);
                SetSpawn(soWave, 2, spRightTop);
                SetSpawn(soWave, 3, spRightMid);
                SetSpawn(soWave, 4, spRightBot);
                SetSpawn(soWave, 5, spLeftBot);
                soWave.ApplyModifiedPropertiesWithoutUndo();
            }

            Script_04_ObjectPool pool = Object.FindFirstObjectByType<Script_04_ObjectPool>();
            if (pool != null)
            {
                GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Projectiles/Prefab_Projectile_Geo.prefab");
                SerializedObject soPool = new(pool);
                SerializedProperty configs = soPool.FindProperty("_poolConfigs");
                configs.ClearArray();
                configs.InsertArrayElementAtIndex(0);
                configs.GetArrayElementAtIndex(0).objectReferenceValue = CreatePoolConfig(projPrefab);
                soPool.ApplyModifiedPropertiesWithoutUndo();
            }

            GameObject ui = new("_UI");
            UnityEngine.UIElements.UIDocument hud = ui.AddComponent<UnityEngine.UIElements.UIDocument>();
            hud.visualTreeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>("Assets/_Scripts/UI/HUD_Main.uxml");
            UnityEngine.UIElements.StyleSheet hudLight = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>("Assets/_Scripts/UI/HUD_Main_Light.uss");
            UnityEngine.UIElements.StyleSheet hudBase = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>("Assets/_Scripts/UI/HUD_Main.uss");
            if (hudLight != null)
                hud.rootVisualElement.styleSheets.Add(hudLight);
            else if (hudBase != null)
                hud.rootVisualElement.styleSheets.Add(hudBase);
            ui.AddComponent<Script_25_HUDController>();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Scene_02_GameWorld.unity");
        }

        private static ScriptableObject CreatePoolConfig(GameObject prefab)
        {
            var config = ScriptableObject.CreateInstance<SO_PoolConfig>();
            config.poolKey = "Projectile_Geo";
            config.prefab = prefab;
            config.initialSize = 20;
            config.maxSize = 80;
            string path = "Assets/_ScriptableObjects/Pools/SO_Pool_Projectile_Geo.asset";
            AssetDatabase.CreateAsset(config, path);
            return config;
        }

        private static void SetSpawn(SerializedObject so, int index, Transform t)
        {
            so.FindProperty("_spawnPoints").GetArrayElementAtIndex(index).objectReferenceValue = t;
        }

        private static void BuildPlatform(Transform parent, string name, Vector2 center, Vector2 size, Sprite sprite)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = center;
            go.transform.localScale = size;
            go.layer = LayerMask.NameToLayer("Platform");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, 0.95f);
            sr.sortingOrder = 0;
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            PlatformEffector2D fx = go.AddComponent<PlatformEffector2D>();
            fx.useOneWay = true;
        }

        private static void CreateDecorSprite(Transform parent, string name, Sprite sprite, Vector3 pos, Vector3 scale, int order)
        {
            if (sprite == null) return;
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.color = new Color(1f, 1f, 1f, 0.35f);
        }

        private static Camera CreateCamera(Color bg)
        {
            GameObject camGo = new("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = bg;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
            return cam;
        }

        private static GameObject CreateUIImage(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            UIImage img = go.AddComponent<UIImage>();
            img.sprite = sprite;
            img.type = UIImage.Type.Sliced;
            return go;
        }

        private static GameObject CreateUIButton(Transform parent, string name, Sprite sprite, string label, Vector2 pos, Vector2 size)
        {
            GameObject go = CreateUIImage(parent, name, sprite, pos, size);
            UIButton btn = go.AddComponent<UIButton>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.9f, 1f, 1f);
            cb.pressedColor = new Color(0.75f, 0.9f, 1f);
            btn.colors = cb;
            GameObject textGo = CreateUIText(go.transform, "Text", label, 20, Vector2.zero, new Color(0.1f, 0.2f, 0.35f));
            textGo.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            textGo.GetComponent<RectTransform>().anchorMax = Vector2.one;
            textGo.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            textGo.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateUIText(Transform parent, string name, string content, int fontSize, Vector2 pos, Color color)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 60);
            Text txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return go;
        }

        private static Transform CreateSpawn(Transform parent, string spawnName, Vector2 pos)
        {
            GameObject go = new(spawnName);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            return go.transform;
        }

        private static void CreateChild(GameObject parent, string name, System.Type type)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent.transform);
            child.AddComponent(type);
        }

        private static void SavePrefab(GameObject go, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
