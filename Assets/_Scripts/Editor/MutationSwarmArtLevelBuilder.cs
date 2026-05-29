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
    /// Importa el paquete en Assets/_Art/Materials y construye Scene_02_GameWorld con ese arte.
    /// </summary>
    public static class MutationSwarmArtLevelBuilder
    {
        private const string MaterialsRoot = "Assets/_Art/Materials";
        private const string ScenesPath = "Assets/_Scenes";
        private const float TileSize = 1f;
        private const int PixelsPerUnit = 16;

        private struct ArtSet
        {
            public Sprite starryNight;
            public Sprite galaxyBg;
            public Sprite layer1;
            public Sprite layer2;
            public Sprite layer3;
            public Sprite grassyTop;
            public Sprite grassyFill;
            public Sprite grassySide;
            public Sprite dirtPurpleFill;
            public Sprite dirtPurpleSide;
            public Sprite galaxyBlock1;
            public Sprite galaxyBlock2;
            public Sprite brickPurple;
            public Sprite[] trees;
            public Sprite[] rocks;
            public Sprite cavePillar;
            public Sprite pixelGrass;
        }

        [MenuItem("Tools/Mutation Swarm/Import Art Package Settings")]
        public static void ImportArtPackageSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { MaterialsRoot });
            int count = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                count++;
            }

            Debug.Log($"[MutationSwarm] {count} texturas configuradas como sprites (PPU={PixelsPerUnit}, Point).");
        }

        [MenuItem("Tools/Mutation Swarm/Build Art Level (Scene_02)")]
        public static void BuildArtLevel()
        {
            ImportArtPackageSettings();
            ArtSet art = LoadArtSet();
            if (art.grassyTop == null)
            {
                EditorUtility.DisplayDialog("Mutation Swarm", "No se encontró Grassy_Top.png. Revisa Assets/_Art/Materials.", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(ScenesPath))
                AssetDatabase.CreateFolder("Assets", "_Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildGameWorldWithArt(art);
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Scene_02_GameWorld.unity");

            EnsureSceneInBuildSettings($"{ScenesPath}/Scene_02_GameWorld.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[MutationSwarm] Scene_02_GameWorld generada con paquete de arte.");
        }

        private static ArtSet LoadArtSet()
        {
            return new ArtSet
            {
                starryNight = LoadSprite("Background/Starry_Night_Big.png"),
                galaxyBg = LoadSprite("Background/GalaxyBackground.png"),
                layer1 = LoadSprite("Background/Layer1.png"),
                layer2 = LoadSprite("Background/Layer2.png"),
                layer3 = LoadSprite("Background/Layer3.png"),
                grassyTop = LoadSprite("Blocks/Grassy_Top.png"),
                grassyFill = LoadSprite("Blocks/Grassy_Fill.png"),
                grassySide = LoadSprite("Blocks/Grassy_Side.png"),
                dirtPurpleFill = LoadSprite("Blocks/Dirt_Purple_Fill.png"),
                dirtPurpleSide = LoadSprite("Blocks/Dirt_Purple_Side.png"),
                galaxyBlock1 = LoadSprite("Blocks/Galaxy_Block_1.png"),
                galaxyBlock2 = LoadSprite("Blocks/Galaxy_Block_2.png"),
                brickPurple = LoadSprite("Blocks/Brick_Purple.png"),
                trees = new[]
                {
                    LoadSprite("Trees/PixelTree1.png"),
                    LoadSprite("Trees/PixelTree2.png"),
                    LoadSprite("Trees/PixelTree3.png"),
                    LoadSprite("Trees/PixelTree4.png"),
                    LoadSprite("Trees/PixelTree5.png"),
                    LoadSprite("Trees/PixelTree6.png")
                },
                rocks = new[]
                {
                    LoadSprite("Objects/Rock_1.png"),
                    LoadSprite("Objects/Rock_2.png"),
                    LoadSprite("Objects/Rock_3.png")
                },
                cavePillar = LoadSprite("Objects/Cave_Pillar.png"),
                pixelGrass = LoadSprite("Grass/PixelGrass.png")
            };
        }

        private static Sprite LoadSprite(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{MaterialsRoot}/{relativePath}");
        }

        private static void BuildGameWorldWithArt(ArtSet art)
        {
            int platformLayer = LayerMask.NameToLayer("Platform");
            if (platformLayer < 0)
                platformLayer = 0;

            CreateCamera(new Color(0.02f, 0.02f, 0.05f), 5.4f);

            GameObject managers = new("_Managers");
            CreateChild(managers, "GameManager", typeof(Script_01_GameManager));
            CreateChild(managers, "WaveManager", typeof(Script_02_WaveManager));
            CreateChild(managers, "EvolutionEngine", typeof(Script_07_EvolutionEngine));
            CreateChild(managers, "ObjectPoolManager", typeof(Script_04_ObjectPool));
            CreateChild(managers, "BuildManager", typeof(Script_23_BuildManager));
            CreateChild(managers, "AudioManager", typeof(Script_AudioManager));

            GameObject environment = new("_Environment");

            // --- Fondos parallax ---
            GameObject parallax = new("Parallax");
            parallax.transform.SetParent(environment.transform);
            CreateParallaxBg(parallax.transform, "BG_Starry", art.starryNight, new Vector3(0f, 1f, 10f),
                new Vector3(25f, 14f, 1f), -200, 0.05f);
            CreateParallaxBg(parallax.transform, "BG_Galaxy", art.galaxyBg, new Vector3(0f, 0.5f, 9f),
                new Vector3(22f, 12f, 1f), -190, 0.08f);
            CreateParallaxBg(parallax.transform, "BG_Layer1", art.layer1, new Vector3(0f, -1.5f, 8f),
                new Vector3(20f, 8f, 1f), -150, 0.2f);
            CreateParallaxBg(parallax.transform, "BG_Layer2", art.layer2, new Vector3(0f, -2f, 7f),
                new Vector3(18f, 6f, 1f), -130, 0.35f);
            CreateParallaxBg(parallax.transform, "BG_Layer3", art.layer3, new Vector3(0f, -2.5f, 6f),
                new Vector3(16f, 5f, 1f), -110, 0.5f);

            // --- Plataformas (tile por tile) ---
            GameObject platforms = new("Platforms");
            platforms.transform.SetParent(environment.transform);

            // Suelo principal: y=-4, 18 tiles de ancho, 2 de alto
            BuildGrassPlatform(platforms.transform, "Platform_Ground", new Vector2(0f, -4f), 18, 2, art, platformLayer);
            BuildGrassPlatform(platforms.transform, "Platform_Left_High", new Vector2(-5f, 1.5f), 4, 2, art, platformLayer);
            BuildGrassPlatform(platforms.transform, "Platform_Center", new Vector2(0f, 0.5f), 4, 2, art, platformLayer);
            BuildGrassPlatform(platforms.transform, "Platform_Right_High", new Vector2(5f, 1.5f), 4, 2, art, platformLayer);
            BuildGrassPlatform(platforms.transform, "Platform_Left_Mid", new Vector2(-6.5f, -1.5f), 3, 2, art, platformLayer);
            BuildGrassPlatform(platforms.transform, "Platform_Right_Mid", new Vector2(6.5f, -1.5f), 3, 2, art, platformLayer);

            // Zona mutada central (tierra púrpura)
            BuildPurplePlatform(platforms.transform, "Platform_MutationZone", new Vector2(0f, -2.2f), 5, 2, art, platformLayer);

            // Plataformas galaxia flotantes
            BuildGalaxyPlatform(platforms.transform, "Platform_Galaxy_Left", new Vector2(-7f, 2.8f), 3, art, platformLayer);
            BuildGalaxyPlatform(platforms.transform, "Platform_Galaxy_Right", new Vector2(7f, 2.8f), 3, art, platformLayer);

            // Paredes y techo
            BuildWall(platforms.transform, "Wall_Left", new Vector2(-9.5f, 0f), 1, 10, art.brickPurple ?? art.grassyFill, platformLayer);
            BuildWall(platforms.transform, "Wall_Right", new Vector2(9.5f, 0f), 1, 10, art.brickPurple ?? art.grassyFill, platformLayer);
            BuildWall(platforms.transform, "Ceiling", new Vector2(0f, 4.5f), 20, 1, art.brickPurple ?? art.grassyFill, platformLayer);

            // --- Decoración ---
            GameObject decor = new("Decoration");
            decor.transform.SetParent(environment.transform);
            PlaceTree(decor.transform, new Vector2(-7.5f, -3.2f), art.trees, 0, 15);
            PlaceTree(decor.transform, new Vector2(-4f, -3.2f), art.trees, 1, 16);
            PlaceTree(decor.transform, new Vector2(4.5f, -3.2f), art.trees, 2, 15);
            PlaceTree(decor.transform, new Vector2(7f, -3.2f), art.trees, 3, 16);
            PlaceTree(decor.transform, new Vector2(-5.5f, 2.2f), art.trees, 4, 20);
            PlaceTree(decor.transform, new Vector2(5.5f, 2.2f), art.trees, 5, 20);
            PlaceProp(decor.transform, "Rock_A", new Vector2(-2f, -3.3f), art.rocks, 0, 12);
            PlaceProp(decor.transform, "Rock_B", new Vector2(2.5f, -3.3f), art.rocks, 1, 12);
            PlaceProp(decor.transform, "Pillar", new Vector2(-8f, -2f), art.cavePillar, 14);
            PlaceProp(decor.transform, "GrassDetail", new Vector2(-1f, -3.45f), art.pixelGrass, 18);

            // Spawn points
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

            new GameObject("_Players");
            new GameObject("_Enemies");
            GameObject structuresRoot = new("_Structures");

            GameObject ui = new("_UI");
            UIDocument hud = ui.AddComponent<UIDocument>();
            hud.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Scripts/UI/HUD_Main.uxml");
            StyleSheet hudUss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Scripts/UI/HUD_Main.uss");
            if (hudUss != null)
                hud.rootVisualElement.styleSheets.Add(hudUss);
            ui.AddComponent<Script_25_HUDController>();
            ui.AddComponent<Script_27_EvolutionDisplayUI>();

            WireManagers(structuresRoot.transform, spawnPoints.transform);
        }

        private static void BuildGrassPlatform(Transform parent, string name, Vector2 center, int width, int height, ArtSet art, int layer)
        {
            GameObject root = new(name);
            root.transform.SetParent(parent);
            float left = center.x - (width * TileSize) * 0.5f + TileSize * 0.5f;
            float bottom = center.y - (height * TileSize) * 0.5f + TileSize * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 pos = new(left + x * TileSize, bottom + y * TileSize);
                    Sprite sprite = y == height - 1 ? art.grassyTop : art.grassyFill;
                    if (x == 0 && art.grassySide != null)
                        sprite = art.grassySide;
                    else if (x == width - 1 && art.grassySide != null)
                        sprite = art.grassySide;

                    CreateColliderTile(root.transform, $"t_{x}_{y}", pos, sprite, layer, y == height - 1);
                }
            }
        }

        private static void BuildPurplePlatform(Transform parent, string name, Vector2 center, int width, int height, ArtSet art, int layer)
        {
            GameObject root = new(name);
            root.transform.SetParent(parent);
            float left = center.x - (width * TileSize) * 0.5f + TileSize * 0.5f;
            float bottom = center.y - (height * TileSize) * 0.5f + TileSize * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 pos = new(left + x * TileSize, bottom + y * TileSize);
                    Sprite sprite = art.dirtPurpleFill;
                    if ((x == 0 || x == width - 1) && art.dirtPurpleSide != null)
                        sprite = art.dirtPurpleSide;
                    CreateColliderTile(root.transform, $"p_{x}_{y}", pos, sprite, layer, y == height - 1);
                }
            }
        }

        private static void BuildGalaxyPlatform(Transform parent, string name, Vector2 center, int width, ArtSet art, int layer)
        {
            GameObject root = new(name);
            root.transform.SetParent(parent);
            float left = center.x - (width * TileSize) * 0.5f + TileSize * 0.5f;
            float y = center.y;

            for (int x = 0; x < width; x++)
            {
                Sprite sprite = x % 2 == 0 ? art.galaxyBlock1 : art.galaxyBlock2;
                CreateColliderTile(root.transform, $"g_{x}", new Vector2(left + x * TileSize, y), sprite, layer, true);
            }
        }

        private static void BuildWall(Transform parent, string name, Vector2 center, int width, int height, Sprite sprite, int layer)
        {
            GameObject root = new(name);
            root.transform.SetParent(parent);
            float left = center.x - (width * TileSize) * 0.5f + TileSize * 0.5f;
            float bottom = center.y - (height * TileSize) * 0.5f + TileSize * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreateColliderTile(root.transform, $"w_{x}_{y}",
                        new Vector2(left + x * TileSize, bottom + y * TileSize), sprite, layer, false);
                }
            }
        }

        private static void CreateColliderTile(Transform parent, string name, Vector2 pos, Sprite sprite, int layer, bool oneWayTop)
        {
            if (sprite == null)
                return;

            GameObject tile = new(name);
            tile.transform.SetParent(parent);
            tile.transform.position = pos;
            tile.layer = layer;
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0;
            BoxCollider2D col = tile.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            if (oneWayTop)
            {
                PlatformEffector2D fx = tile.AddComponent<PlatformEffector2D>();
                fx.useOneWay = true;
            }
        }

        private static void CreateParallaxBg(Transform parent, string name, Sprite sprite, Vector3 pos, Vector3 scale, int sortingOrder, float factor)
        {
            if (sprite == null)
                return;

            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            Script_31_ParallaxLayer parallax = go.AddComponent<Script_31_ParallaxLayer>();
            SerializedObject so = new(parallax);
            so.FindProperty("_parallaxFactor").floatValue = factor;
            so.FindProperty("_spriteWidth").floatValue = scale.x;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PlaceTree(Transform parent, Vector2 pos, Sprite[] trees, int index, int sortingOrder)
        {
            if (trees == null || index >= trees.Length || trees[index] == null)
                return;
            PlaceProp(parent, $"Tree_{index}", pos, trees[index], sortingOrder);
        }

        private static void PlaceProp(Transform parent, string name, Vector2 pos, Sprite[] sprites, int index, int sortingOrder)
        {
            if (sprites == null || index >= sprites.Length)
                return;
            PlaceProp(parent, name, pos, sprites[index], sortingOrder);
        }

        private static void PlaceProp(Transform parent, string name, Vector2 pos, Sprite sprite, int sortingOrder)
        {
            if (sprite == null)
                return;

            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
        }

        private static void WireManagers(Transform structuresRoot, Transform spawnRoot)
        {
            Script_23_BuildManager build = Object.FindFirstObjectByType<Script_23_BuildManager>();
            if (build != null)
            {
                SerializedObject so = new(build);
                so.FindProperty("_structuresRoot").objectReferenceValue = structuresRoot;
                so.FindProperty("_buildSurfaceMask").intValue =
                    (1 << LayerMask.NameToLayer("BuildSurface")) | (1 << LayerMask.NameToLayer("Platform"));
                so.FindProperty("_structureMask").intValue = 1 << LayerMask.NameToLayer("Structure");
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreateCamera(Color bg, float ortho = 5f)
        {
            GameObject camGo = new("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = ortho;
            cam.backgroundColor = bg;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
        }

        private static void CreateChild(GameObject parent, string name, System.Type type)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent.transform);
            child.AddComponent(type);
        }

        private static void CreateSpawn(Transform parent, string spawnName, Vector2 pos)
        {
            GameObject go = new(spawnName);
            go.transform.SetParent(parent);
            go.transform.position = pos;
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            if (!File.Exists(scenePath))
                return;

            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene s in scenes)
            {
                if (s.path == scenePath)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
