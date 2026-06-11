#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MutationSwarm.Building;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using MutationSwarm.Evolution;
using MutationSwarm.Rooms;
using MutationSwarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Builds all 6 single-screen rooms and their supporting prefabs.
    ///
    /// Run via: Tools > Mutation Swarm > Build All Rooms
    ///
    /// Physics reference (Script_12_PlayerStats defaults):
    ///   MoveSpeed = 6 u/s  |  JumpForce = 12 (impulse, max height ~6 u)
    ///   DashForce = 18 u/s |  DashDuration = 0.15s (reach ~2.7 u)
    ///   Camera orthoSize = 5.4  →  visible: 19.2 u wide × 10.8 u tall
    ///   Tile = 1 unit  |  Walls at ±9.5  |  Floor at -4.0  |  Ceiling at 4.5
    /// </summary>
    public static class MutationSwarmRoomBuilder
    {
        // ── Paths ─────────────────────────────────────────────────────────
        private const string MaterialsRoot = "Assets/_Art/Materials";
        private const string ScenesPath    = "Assets/_Scenes/Rooms";
        private const string PrefabsPath   = "Assets/_Prefabs/Rooms";
        private const float  TileSize      = 1f;

        // ── Platform type enum ────────────────────────────────────────────
        private enum TileStyle { Grass, Purple, Galaxy, Brick }

        // ── Data structures ───────────────────────────────────────────────
        private struct PlatformDef
        {
            public string    Name;
            public Vector2   Center;
            public int       Width, Height;
            public TileStyle Style;
            public bool      OneWay;    // top-only collision
            public bool      Moving;    // apply MovingPlatform component
            public bool      Destructible;
            public Vector2   MoveA, MoveB;
            public float     MoveSpeed;
        }

        private struct SpawnDef
        {
            public string  Name;
            public Vector2 Position;
            public bool    IsPlayer;
        }

        private struct RoomDef
        {
            public string          SceneName;
            public string          RoomType;      // Room_Platform etc.
            public string          NextScene;
            public Color           BgColor;
            public PlatformDef[]   Platforms;
            public SpawnDef[]      Spawns;
            public Vector2         CheckpointPos;
            public Vector2         ExitPos;
            public Vector2[]       ChestPositions;
            public bool            RequireWaveClear;
            public string          EnemyPrefabName;
        }

        // ── Art sprites (loaded once) ─────────────────────────────────────
        private static Sprite _grassTop, _grassFill, _grassSide;
        private static Sprite _purpleFill, _purpleSide;
        private static Sprite _galaxy1, _galaxy2;
        private static Sprite _brick;
        private static Sprite _starryBg, _galaxyBg, _layer1;

        // ── Entry point ───────────────────────────────────────────────────

        [MenuItem("Tools/Mutation Swarm/Build All Rooms")]
        public static void BuildAll()
        {
            LoadSprites();
            if (_grassTop == null)
            {
                EditorUtility.DisplayDialog("Room Builder",
                    "Art sprites missing. Run 'Import Art Package Settings' first.", "OK");
                return;
            }

            EnsureFolder("Assets/_Scenes", "Rooms");
            EnsureFolder("Assets/_Prefabs", "Rooms");

            CreateRoomPrefabs();

            foreach (RoomDef room in BuildRoomDefinitions())
                BuildRoom(room);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] All 6 rooms built successfully.");
        }

        // ── Room definitions ──────────────────────────────────────────────

        private static RoomDef[] BuildRoomDefinitions()
        {
            Color darkBg   = new(0.02f, 0.02f, 0.05f);
            Color purpleBg = new(0.04f, 0.01f, 0.06f);
            Color galaxyBg = new(0.01f, 0.01f, 0.04f);

            return new RoomDef[]
            {
                // ── Room_01 — Platform Easy ────────────────────────────────
                new()
                {
                    SceneName        = "Room_01",
                    RoomType         = "Platform_Easy",
                    NextScene        = "Room_02",
                    BgColor          = darkBg,
                    RequireWaveClear = false,
                    EnemyPrefabName  = "Prefab_Enemy_Drone",
                    CheckpointPos    = new(-6f, -2.8f),
                    ExitPos          = new(7f, -2.8f),
                    ChestPositions   = new Vector2[] { new(0f, 3.8f) },
                    Platforms = new PlatformDef[]
                    {
                        //  NAME                    CENTER        W   H  STYLE           1WAY
                        Plat("Floor",         new( 0f,-4f),  18,  2, TileStyle.Grass,   false),
                        Plat("Wall_L",        new(-9.5f, 0f),  1, 10, TileStyle.Brick,  false),
                        Plat("Wall_R",        new( 9.5f, 0f),  1, 10, TileStyle.Brick,  false),
                        Plat("Ceiling",       new( 0f, 4.5f), 20,  1, TileStyle.Brick,  false),
                        Plat("Plat_LL",       new(-5.5f,-1.5f), 4, 1, TileStyle.Grass,  true),
                        Plat("Plat_Center",   new( 0f,  0f),  4,  1, TileStyle.Grass,   true),
                        Plat("Plat_RL",       new( 5f, -1.5f), 3, 1, TileStyle.Grass,   true),
                        Plat("Plat_LH",       new(-4f,  1.5f), 3, 1, TileStyle.Grass,   true),
                        Plat("Plat_RH",       new( 4f,  2.5f), 3, 1, TileStyle.Galaxy,  true),
                        MovingPlat("Plat_Mov",new(-1f,-2.5f), 3, 1, TileStyle.Purple,
                            new(-3.5f,-2.5f), new(3.5f,-2.5f), 2f),
                    },
                    Spawns = new SpawnDef[]
                    {
                        Spawn("sp_L",    new(-8f, -3f)),
                        Spawn("sp_R",    new( 8f, -3f)),
                        Player("p1",     new(-1f, -3f)),
                        Player("p2",     new( 1f, -3f)),
                        Player("p3",     new(-2f, -3f)),
                        Player("p4",     new( 2f, -3f)),
                    },
                },

                // ── Room_02 — Combat Medium ────────────────────────────────
                new()
                {
                    SceneName        = "Room_02",
                    RoomType         = "Combat_Medium",
                    NextScene        = "Room_03",
                    BgColor          = purpleBg,
                    RequireWaveClear = true,
                    EnemyPrefabName  = "Prefab_Enemy_Drone",
                    CheckpointPos    = new(-7f, -3f),
                    ExitPos          = new( 7f, -3f),
                    ChestPositions   = new Vector2[] { new(0f, 3f) },
                    Platforms = new PlatformDef[]
                    {
                        Plat("Floor",       new( 0f,-4f),   18, 2, TileStyle.Grass,   false),
                        Plat("Wall_L",      new(-9.5f, 0f),  1,10, TileStyle.Brick,   false),
                        Plat("Wall_R",      new( 9.5f, 0f),  1,10, TileStyle.Brick,   false),
                        Plat("Ceiling",     new( 0f, 4.5f), 20, 1, TileStyle.Brick,   false),
                        Plat("Plat_Center", new( 0f, -1f),   6, 1, TileStyle.Purple,  true),
                        Plat("Plat_L",      new(-6f,  1f),   3, 1, TileStyle.Grass,   true),
                        Plat("Plat_R",      new( 6f,  1f),   3, 1, TileStyle.Grass,   true),
                        Plat("Plat_Top",    new( 0f,  2.5f), 4, 1, TileStyle.Galaxy,  true),
                        Plat("Plat_LM",     new(-3f,  0f),   3, 1, TileStyle.Grass,   true),
                        Plat("Plat_RM",     new( 3f,  0f),   3, 1, TileStyle.Grass,   true),
                    },
                    Spawns = new SpawnDef[]
                    {
                        Spawn("sp_TL",  new(-8.5f, 4f)),
                        Spawn("sp_TR",  new( 8.5f, 4f)),
                        Spawn("sp_ML",  new(-8.5f, 0f)),
                        Spawn("sp_MR",  new( 8.5f, 0f)),
                        Spawn("sp_BL",  new(-8.5f,-3.5f)),
                        Player("p1",    new(-1f, -3f)),
                        Player("p2",    new( 1f, -3f)),
                        Player("p3",    new(-2f, -3f)),
                        Player("p4",    new( 2f, -3f)),
                    },
                },

                // ── Room_03 — Secret Medium ────────────────────────────────
                new()
                {
                    SceneName        = "Room_03",
                    RoomType         = "Secret_Medium",
                    NextScene        = "Room_04",
                    BgColor          = galaxyBg,
                    RequireWaveClear = false,
                    EnemyPrefabName  = "Prefab_Enemy_Mimic",
                    CheckpointPos    = new( 0f, -3f),
                    ExitPos          = new( 8f, -3f),
                    // Secret chest hidden near the top gap
                    ChestPositions   = new Vector2[] { new(0f, 4f), new(-7f, 3.7f) },
                    Platforms = new PlatformDef[]
                    {
                        Plat("Floor",         new( 0f,-4f),  18, 2, TileStyle.Grass,  false),
                        Plat("Wall_L",        new(-9.5f, 0f), 1,10, TileStyle.Brick,  false),
                        Plat("Wall_R",        new( 9.5f, 0f), 1,10, TileStyle.Brick,  false),
                        // Partial ceiling with gap at center (gap = -2 to +2)
                        Plat("Ceil_L",        new(-6f, 4.5f), 8, 1, TileStyle.Brick,  false),
                        Plat("Ceil_R",        new( 6f, 4.5f), 8, 1, TileStyle.Brick,  false),
                        // Inner wall creating secret left pocket
                        Plat("Inner_Wall_L",  new(-5.5f, 2f), 1, 5, TileStyle.Brick,  false),
                        // Regular platforms
                        Plat("Plat_A",        new(-4f,-1.5f), 3, 1, TileStyle.Grass,  true),
                        Plat("Plat_B",        new( 3f,-0.5f), 4, 1, TileStyle.Grass,  true),
                        Plat("Plat_C",        new(-1f, 1.5f), 3, 1, TileStyle.Purple, true),
                        Plat("Plat_D",        new( 5f, 2.5f), 3, 1, TileStyle.Grass,  true),
                        // Secret platforms near ceiling gap
                        Plat("Plat_Sec_L",   new(-7.5f, 3.5f), 2, 1, TileStyle.Galaxy, true),
                        Plat("Plat_Sec_R",   new( 6.5f, 3.8f), 2, 1, TileStyle.Galaxy, true),
                    },
                    Spawns = new SpawnDef[]
                    {
                        Spawn("sp_TL",     new(-8.5f, 3.5f)),
                        Spawn("sp_TR",     new( 8.5f, 3.5f)),
                        Spawn("sp_ML",     new(-8.5f,-1f)),
                        Spawn("sp_Secret", new( 0f,   4.2f)), // secret spawn in gap
                        Player("p1",       new(-1f,  -3f)),
                        Player("p2",       new( 1f,  -3f)),
                        Player("p3",       new(-2f,  -3f)),
                        Player("p4",       new( 2f,  -3f)),
                    },
                },

                // ── Room_04 — Mixed Hard ───────────────────────────────────
                new()
                {
                    SceneName        = "Room_04",
                    RoomType         = "Mixed_Hard",
                    NextScene        = "Room_05",
                    BgColor          = purpleBg,
                    RequireWaveClear = true,
                    EnemyPrefabName  = "Prefab_Enemy_Queen",
                    CheckpointPos    = new( 0f, -3f),
                    ExitPos          = new( 8f,  3f),
                    ChestPositions   = new Vector2[] { new(-7f, 3.7f), new(5.5f, 3.7f) },
                    Platforms = new PlatformDef[]
                    {
                        Plat("Floor",        new( 0f,-4f),  16, 2, TileStyle.Grass,  false),
                        Plat("Wall_L",       new(-9.5f, 0f), 1,10, TileStyle.Brick,  false),
                        Plat("Wall_R",       new( 9.5f, 0f), 1,10, TileStyle.Brick,  false),
                        Plat("Ceiling",      new( 0f, 4.5f),20, 1, TileStyle.Brick,  false),
                        Plat("Plat_1",       new(-6f,-2f),   3, 1, TileStyle.Grass,  true),
                        Plat("Plat_2",       new(-2f,-0.5f), 3, 1, TileStyle.Grass,  true),
                        Plat("Plat_3",       new( 3f, 0f),   3, 1, TileStyle.Grass,  true),
                        Plat("Plat_4",       new(-5f, 1.5f), 3, 1, TileStyle.Purple, true),
                        Plat("Plat_5",       new( 1f, 2.5f), 4, 1, TileStyle.Grass,  true),
                        Plat("Plat_6",       new( 6f, 3f),   3, 1, TileStyle.Galaxy, true),
                        Plat("Plat_7",       new(-7f, 3.5f), 2, 1, TileStyle.Galaxy, true),
                        Plat("Plat_Mut",     new( 1f,-2.5f), 4, 1, TileStyle.Purple, true),
                        // Destructible platforms
                        DestrPlat("Destr_1", new( 4f,-1.5f), 2),
                        DestrPlat("Destr_2", new( 2f, 1f),   2),
                        // Moving platform
                        MovingPlat("Plat_Mov", new(-1f,-3f), 3, 1, TileStyle.Purple,
                            new(-3f,-3f), new(3f,-3f), 1.5f),
                    },
                    Spawns = new SpawnDef[]
                    {
                        Spawn("sp_TL",  new(-8.5f, 4f)),
                        Spawn("sp_TR",  new( 8.5f, 4f)),
                        Spawn("sp_ML",  new(-8.5f, 0f)),
                        Spawn("sp_MR",  new( 8.5f, 0f)),
                        Spawn("sp_BL",  new(-8.5f,-3.5f)),
                        Spawn("sp_TC",  new( 0f,   4f)),
                        Player("p1",    new(-1f,  -3f)),
                        Player("p2",    new( 1f,  -3f)),
                        Player("p3",    new(-2f,  -3f)),
                        Player("p4",    new( 2f,  -3f)),
                    },
                },

                // ── Room_05 — Platform Hard ────────────────────────────────
                new()
                {
                    SceneName        = "Room_05",
                    RoomType         = "Platform_Hard",
                    NextScene        = "Room_Boss",
                    BgColor          = galaxyBg,
                    RequireWaveClear = false,
                    EnemyPrefabName  = "Prefab_Enemy_Parasite",
                    CheckpointPos    = new(-8f, -3f),
                    ExitPos          = new( 0f, 4f),
                    ChestPositions   = new Vector2[] { new(-1f, 3.7f) },
                    Platforms = new PlatformDef[]
                    {
                        // Split floor (large gap in center)
                        Plat("Floor_L",      new(-7f, -4f),  5, 2, TileStyle.Grass,  false),
                        Plat("Floor_R",      new( 6f, -4f),  6, 2, TileStyle.Grass,  false),
                        Plat("Wall_L",       new(-9.5f, 0f), 1,10, TileStyle.Brick,  false),
                        Plat("Wall_R",       new( 9.5f, 0f), 1,10, TileStyle.Brick,  false),
                        Plat("Ceiling",      new( 0f, 4.5f),20, 1, TileStyle.Brick,  false),
                        // Floating platforms (sparse — requires precise jumps)
                        Plat("Plat_0",       new( 0f,-3f),   2, 1, TileStyle.Galaxy, true),
                        Plat("Plat_1",       new(-6f,-2f),   2, 1, TileStyle.Grass,  true),
                        Plat("Plat_2",       new(-3f,-0.5f), 2, 1, TileStyle.Grass,  true),
                        Plat("Plat_3",       new( 1f, 0.5f), 2, 1, TileStyle.Purple, true),
                        Plat("Plat_3B",      new( 3f,-0.5f), 2, 1, TileStyle.Grass,  true),
                        Plat("Plat_4",       new(-5f, 2f),   2, 1, TileStyle.Grass,  true),
                        Plat("Plat_5",       new( 5f, 3f),   2, 1, TileStyle.Galaxy, true),
                        Plat("Plat_6",       new( 7f, 1.5f), 2, 1, TileStyle.Grass,  true),
                        Plat("Plat_Top",     new(-1f, 3.5f), 3, 1, TileStyle.Galaxy, true),
                        // Two moving platforms bridging key gaps
                        MovingPlat("Mov_V",  new(-7f, 1f),   1, 1, TileStyle.Purple,
                            new(-7f, 0f), new(-7f, 2.5f), 1.5f),
                        MovingPlat("Mov_H",  new( 4f,-1.5f), 2, 1, TileStyle.Purple,
                            new( 3f,-1.5f), new( 7f,-1.5f), 2f),
                    },
                    Spawns = new SpawnDef[]
                    {
                        Spawn("sp_TL",    new(-8.5f, 4f)),
                        Spawn("sp_TR",    new( 8.5f, 4f)),
                        Spawn("sp_Center",new(-1f,   4.2f)),
                        Spawn("sp_Gap",   new( 0f,  -3.5f)), // in the floor gap
                        Player("p1",      new(-8f,  -3f)),
                        Player("p2",      new(-7f,  -3f)),
                        Player("p3",      new(-8f,  -2.5f)),
                        Player("p4",      new(-7f,  -2.5f)),
                    },
                },

                // ── Room_Boss — Boss Arena ──────────────────────────────────
                new()
                {
                    SceneName        = "Room_Boss",
                    RoomType         = "Boss_Arena",
                    NextScene        = "Scene_01_MainMenu",
                    BgColor          = purpleBg,
                    RequireWaveClear = true,
                    EnemyPrefabName  = "Prefab_Enemy_Boss",
                    CheckpointPos    = new( 0f, -3f),
                    ExitPos          = new( 0f,  4f),
                    ChestPositions   = new Vector2[] { new(-8f, -3f), new( 8f, -3f) },
                    Platforms = new PlatformDef[]
                    {
                        // Full-width arena floor (purple theme)
                        Plat("Floor",      new( 0f,-4f),   20, 2, TileStyle.Purple, false),
                        Plat("Wall_L",     new(-9.5f, 0f),  1,10, TileStyle.Brick,  false),
                        Plat("Wall_R",     new( 9.5f, 0f),  1,10, TileStyle.Brick,  false),
                        Plat("Ceiling",    new( 0f, 4.5f), 20, 1, TileStyle.Brick,  false),
                        // Arena platforms (bilaterally symmetric)
                        Plat("Plat_L1",    new(-7f,-1.5f),  3, 1, TileStyle.Grass,  true),
                        Plat("Plat_R1",    new( 7f,-1.5f),  3, 1, TileStyle.Grass,  true),
                        Plat("Plat_L2",    new(-5.5f, 0.5f),3, 1, TileStyle.Grass,  true),
                        Plat("Plat_R2",    new( 5.5f, 0.5f),3, 1, TileStyle.Grass,  true),
                        Plat("Plat_CL",    new(-2.5f, 1.5f),3, 1, TileStyle.Purple, true),
                        Plat("Plat_CR",    new( 2.5f, 1.5f),3, 1, TileStyle.Purple, true),
                        Plat("Plat_L3",    new(-7f, 3f),    2, 1, TileStyle.Galaxy, true),
                        Plat("Plat_R3",    new( 7f, 3f),    2, 1, TileStyle.Galaxy, true),
                        // Central mutation zone marker
                        Plat("MutZone",    new( 0f,-2.5f),  6, 1, TileStyle.Purple, true),
                        // Moving platforms flanking the arena
                        MovingPlat("Mov_L",new(-5f,-2.5f),  2, 1, TileStyle.Galaxy,
                            new(-7.5f,-2.5f), new(-3f,-2.5f), 1.8f),
                        MovingPlat("Mov_R",new( 5f,-2.5f),  2, 1, TileStyle.Galaxy,
                            new( 3f,-2.5f),  new( 7.5f,-2.5f), 1.8f),
                    },
                    Spawns = new SpawnDef[]
                    {
                        Spawn("sp_TL",   new(-9f, 4f)),
                        Spawn("sp_TR",   new( 9f, 4f)),
                        Spawn("sp_ML",   new(-9f, 0f)),
                        Spawn("sp_MR",   new( 9f, 0f)),
                        Spawn("sp_BL",   new(-9f,-3.5f)),
                        Spawn("sp_BR",   new( 9f,-3.5f)),
                        Spawn("sp_TC",   new( 0f, 4f)),
                        Player("p1",     new(-1f,-3f)),
                        Player("p2",     new( 1f,-3f)),
                        Player("p3",     new(-2f,-3f)),
                        Player("p4",     new( 2f,-3f)),
                    },
                },
            };
        }

        // ── Room construction ─────────────────────────────────────────────

        private static void BuildRoom(RoomDef def)
        {
            int platformLayer = Mathf.Max(0, LayerMask.NameToLayer("Platform"));
            int playerLayer   = Mathf.Max(0, LayerMask.NameToLayer("Player"));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string scenePath = $"{ScenesPath}/{def.SceneName}.unity";

            // Camera
            CreateCamera(def.BgColor, 5.4f);

            // Managers
            GameObject managers = new("_Managers");
            CreateChild(managers, "GameManager",    typeof(Script_01_GameManager));
            CreateChild(managers, "WaveManager",    typeof(Script_02_WaveManager));
            CreateChild(managers, "EvolutionEngine",typeof(Script_07_EvolutionEngine));
            CreateChild(managers, "ObjectPool",     typeof(Script_04_ObjectPool));
            CreateChild(managers, "BuildManager",   typeof(Script_23_BuildManager));
            CreateChild(managers, "AudioManager",   typeof(Script_AudioManager));

            // Background
            GameObject env = new("_Environment");
            AddBackground(env.transform);

            // Platforms
            GameObject platforms = new("Platforms");
            platforms.transform.SetParent(env.transform);
            foreach (PlatformDef pDef in def.Platforms)
                BuildPlatform(platforms.transform, pDef, platformLayer);

            // Spawn points
            var spawnTransforms = new List<Transform>();
            GameObject spawnRoot = new("_SpawnPoints");
            spawnRoot.AddComponent<Script_SpawnPointGizmos>();

            foreach (SpawnDef sp in def.Spawns)
            {
                GameObject go = new(sp.Name);
                go.transform.SetParent(spawnRoot.transform);
                go.transform.position = sp.Position;
                if (!sp.IsPlayer)
                    spawnTransforms.Add(go.transform);
            }

            // Checkpoint
            PlaceRoomElement(spawnRoot.transform, "Checkpoint", def.CheckpointPos,
                typeof(CheckpointController), "Assets/_Prefabs/Rooms/Checkpoint.prefab");

            // Level exit
            GameObject exitGo = PlaceRoomElement(spawnRoot.transform, "LevelExit", def.ExitPos,
                typeof(LevelExit), "Assets/_Prefabs/Rooms/LevelExit.prefab");
            if (exitGo != null && exitGo.TryGetComponent(out LevelExit exit))
            {
                SerializedObject soExit = new(exit);
                soExit.FindProperty("_nextScene").stringValue         = def.NextScene;
                soExit.FindProperty("_requireWaveClear").boolValue    = def.RequireWaveClear;
                soExit.ApplyModifiedPropertiesWithoutUndo();
            }

            // Chests
            if (def.ChestPositions != null)
            {
                for (int i = 0; i < def.ChestPositions.Length; i++)
                    PlaceRoomElement(spawnRoot.transform, $"Chest_{i}", def.ChestPositions[i],
                        typeof(Chest), "Assets/_Prefabs/Rooms/Chest.prefab");
            }

            new GameObject("_Players");
            new GameObject("_Enemies");
            GameObject structRoot = new("_Structures");

            // UI
            GameObject ui = new("_UI");
            ui.AddComponent<Script_25_HUDController>();

            // Wire managers
            WireWaveManager(spawnTransforms.ToArray(), def.EnemyPrefabName);
            WireBuildManager(structRoot.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            EnsureSceneInBuildSettings(scenePath);

            Debug.Log($"[MutationSwarm] Room built: {def.SceneName} ({def.RoomType})");
        }

        // ── Platform builders ─────────────────────────────────────────────

        private static void BuildPlatform(Transform parent, PlatformDef def, int layer)
        {
            if (def.Moving)
            {
                BuildMovingPlatform(parent, def, layer);
                return;
            }
            if (def.Destructible)
            {
                BuildDestructiblePlatform(parent, def, layer);
                return;
            }

            GameObject root = new(def.Name);
            root.transform.SetParent(parent);

            float left   = def.Center.x - def.Width  * TileSize * 0.5f + TileSize * 0.5f;
            float bottom = def.Center.y - def.Height * TileSize * 0.5f + TileSize * 0.5f;

            for (int x = 0; x < def.Width; x++)
            {
                for (int y = 0; y < def.Height; y++)
                {
                    Vector2 pos    = new(left + x * TileSize, bottom + y * TileSize);
                    bool isTop     = (y == def.Height - 1);
                    Sprite sprite  = ChooseSprite(def.Style, x, y, def.Width, def.Height);
                    bool oneWayTop = def.OneWay && isTop;
                    CreateTile(root.transform, $"t{x}_{y}", pos, sprite, layer, oneWayTop);
                }
            }
        }

        private static void BuildMovingPlatform(Transform parent, PlatformDef def, int layer)
        {
            GameObject root = new(def.Name);
            root.transform.SetParent(parent);
            root.transform.position = def.Center;
            root.layer = layer;

            Sprite sprite = ChooseSprite(def.Style, 0, 0, def.Width, def.Height);
            for (int x = 0; x < def.Width; x++)
            {
                float tx = def.Center.x - def.Width * TileSize * 0.5f + TileSize * (x + 0.5f);
                CreateTile(root.transform, $"mt{x}", new Vector2(tx, def.Center.y),
                    sprite, layer, def.OneWay);
            }

            // Add MovingPlatform script to root
            Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType        = RigidbodyType2D.Kinematic;
            rb.interpolation   = RigidbodyInterpolation2D.Interpolate;

            MovingPlatform mp = root.AddComponent<MovingPlatform>();
            SerializedObject so = new(mp);
            so.FindProperty("_pointA").vector2Value  = def.MoveA;
            so.FindProperty("_pointB").vector2Value  = def.MoveB;
            so.FindProperty("_speed").floatValue     = def.MoveSpeed;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildDestructiblePlatform(Transform parent, PlatformDef def, int layer)
        {
            for (int x = 0; x < def.Width; x++)
            {
                float tx = def.Center.x - def.Width * TileSize * 0.5f + TileSize * (x + 0.5f);
                Vector2 pos = new(tx, def.Center.y);

                GameObject tile = new($"{def.Name}_{x}");
                tile.transform.SetParent(parent);
                tile.transform.position = pos;
                tile.layer = layer;

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite       = ChooseSprite(def.Style, x, 0, def.Width, 1);
                sr.sortingOrder = 0;

                BoxCollider2D col = tile.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;

                PlatformEffector2D fx = tile.AddComponent<PlatformEffector2D>();
                fx.useOneWay = true;

                DestructiblePlatform dp = tile.AddComponent<DestructiblePlatform>();
                SerializedObject so = new(dp);
                so.FindProperty("_playerMask").intValue = 1 << LayerMask.NameToLayer("Player");
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreateTile(Transform parent, string name, Vector2 pos, Sprite sprite, int layer, bool oneWayTop)
        {
            if (sprite == null) return;

            GameObject tile = new(name);
            tile.transform.SetParent(parent);
            tile.transform.position = pos;
            tile.layer = layer;

            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite       = sprite;
            sr.sortingOrder = 0;

            tile.AddComponent<BoxCollider2D>().size = Vector2.one;

            if (oneWayTop)
                tile.AddComponent<PlatformEffector2D>().useOneWay = true;
        }

        // ── Sprite selection ──────────────────────────────────────────────

        private static Sprite ChooseSprite(TileStyle style, int x, int y, int w, int h)
        {
            bool isTop  = y == h - 1;
            bool isEdge = (x == 0 || x == w - 1);

            return style switch
            {
                TileStyle.Grass   => isTop   ? _grassTop
                                   : isEdge  ? (_grassSide ?? _grassFill)
                                   : _grassFill,
                TileStyle.Purple  => isEdge  ? (_purpleSide ?? _purpleFill) : _purpleFill,
                TileStyle.Galaxy  => x % 2 == 0 ? _galaxy1 : (_galaxy2 ?? _galaxy1),
                TileStyle.Brick   => _brick ?? _grassFill,
                _                 => _grassFill,
            };
        }

        // ── Background ────────────────────────────────────────────────────

        private static void AddBackground(Transform parent)
        {
            if (_starryBg == null) return;

            GameObject bg = new("Background");
            bg.transform.SetParent(parent);

            AddBgLayer(bg.transform, "BG_Stars",  _starryBg, new Vector3(0f, 0.5f,10f),
                new Vector3(22f,12f,1f), -200);
            if (_galaxyBg != null)
                AddBgLayer(bg.transform, "BG_Galaxy", _galaxyBg, new Vector3(0f, 0f, 9f),
                    new Vector3(20f,11f,1f), -190);
            if (_layer1 != null)
                AddBgLayer(bg.transform, "BG_Layer1", _layer1, new Vector3(0f,-1.5f, 8f),
                    new Vector3(18f, 8f,1f), -150);
        }

        private static void AddBgLayer(Transform parent, string name, Sprite sprite,
            Vector3 pos, Vector3 scale, int order)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.position   = pos;
            go.transform.localScale = scale;
            SpriteRenderer sr       = go.AddComponent<SpriteRenderer>();
            sr.sprite       = sprite;
            sr.sortingOrder = order;
            Script_31_ParallaxLayer plx = go.AddComponent<Script_31_ParallaxLayer>();
            SerializedObject so = new(plx);
            so.FindProperty("_parallaxFactor").floatValue = 0.05f;
            so.FindProperty("_spriteWidth").floatValue    = scale.x;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Room element placement ────────────────────────────────────────

        private static GameObject PlaceRoomElement(Transform parent, string name, Vector2 pos,
            System.Type component, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject go;

            if (prefab != null)
            {
                go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (go != null)
                {
                    go.name = name;
                    go.transform.SetParent(parent);
                    go.transform.position = pos;
                    return go;
                }
            }

            // Fallback: create from scratch
            go = new(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.AddComponent(component);
            return go;
        }

        // ── Prefab creation ───────────────────────────────────────────────

        private static void CreateRoomPrefabs()
        {
            int playerLayer = Mathf.Max(0, LayerMask.NameToLayer("Player"));

            // SpawnPoint
            SavePrefabIfMissing("SpawnPoint", go =>
            {
                go.AddComponent<Script_SpawnPointGizmos>();
            });

            // Checkpoint
            SavePrefabIfMissing("Checkpoint", go =>
            {
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite       = _galaxy1 ?? _grassTop;
                sr.sortingOrder = 20;
                sr.color        = new Color(0.5f, 0.5f, 0.5f, 1f);

                BoxCollider2D col  = go.AddComponent<BoxCollider2D>();
                col.size           = new Vector2(1f, 2f);
                col.isTrigger      = true;

                go.AddComponent<CheckpointController>();
            });

            // LevelExit
            SavePrefabIfMissing("LevelExit", go =>
            {
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite         = _galaxy1 ?? _grassTop;
                sr.sortingOrder   = 20;
                sr.color          = new Color(0.2f, 0.6f, 1f, 1f);

                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size          = new Vector2(1.5f, 2.5f);
                col.isTrigger     = true;

                go.AddComponent<LevelExit>();
            });

            // Chest
            SavePrefabIfMissing("Chest", go =>
            {
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite         = _grassTop ?? _purpleFill;
                sr.sortingOrder   = 20;
                sr.color          = new Color(0.85f, 0.65f, 0.1f, 1f);
                go.transform.localScale = Vector3.one * 0.8f;

                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size          = Vector2.one * 0.9f;
                col.isTrigger     = true;

                go.AddComponent<Chest>();
            });

            // PlataformaMovil (template — waypoints set per instance)
            SavePrefabIfMissing("PlataformaMovil", go =>
            {
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite         = _purpleFill ?? _grassTop;
                sr.sortingOrder   = 0;

                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size          = Vector2.one;

                PlatformEffector2D fx = go.AddComponent<PlatformEffector2D>();
                fx.useOneWay = true;

                Rigidbody2D rb    = go.AddComponent<Rigidbody2D>();
                rb.bodyType       = RigidbodyType2D.Kinematic;
                rb.interpolation  = RigidbodyInterpolation2D.Interpolate;

                go.AddComponent<MovingPlatform>();
            });

            // PlataformaDestructible
            SavePrefabIfMissing("PlataformaDestructible", go =>
            {
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite         = _purpleFill ?? _grassTop;
                sr.sortingOrder   = 0;
                sr.color          = new Color(1f, 0.7f, 0.4f);

                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size          = Vector2.one;

                PlatformEffector2D fx = go.AddComponent<PlatformEffector2D>();
                fx.useOneWay = true;

                DestructiblePlatform dp = go.AddComponent<DestructiblePlatform>();
                SerializedObject so = new(dp);
                so.FindProperty("_playerMask").intValue = 1 << playerLayer;
                so.ApplyModifiedPropertiesWithoutUndo();
            });
        }

        private static void SavePrefabIfMissing(string prefabName, System.Action<GameObject> configure)
        {
            string path = $"{PrefabsPath}/{prefabName}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            GameObject go = new(prefabName);
            configure(go);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        // ── Manager wiring ────────────────────────────────────────────────

        private static void WireWaveManager(Transform[] spawnPoints, string enemyPrefabName)
        {
            Script_02_WaveManager wm = Object.FindFirstObjectByType<Script_02_WaveManager>();
            if (wm == null) return;

            GameObject enemyPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Prefabs/Enemies/{enemyPrefabName}.prefab");

            SerializedObject so = new(wm);
            so.FindProperty("_enemyPrefab").objectReferenceValue = enemyPrefab;

            SerializedProperty spArr = so.FindProperty("_spawnPoints");
            spArr.arraySize = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
                spArr.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireBuildManager(Transform structRoot)
        {
            Script_23_BuildManager bm = Object.FindFirstObjectByType<Script_23_BuildManager>();
            if (bm == null) return;

            SerializedObject so = new(bm);
            so.FindProperty("_structuresRoot").objectReferenceValue = structRoot;
            so.FindProperty("_buildSurfaceMask").intValue =
                (1 << LayerMask.NameToLayer("BuildSurface")) | (1 << LayerMask.NameToLayer("Platform"));
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Utilities ─────────────────────────────────────────────────────

        private static void LoadSprites()
        {
            _grassTop    = Load("Blocks/Grassy_Top.png");
            _grassFill   = Load("Blocks/Grassy_Fill.png");
            _grassSide   = Load("Blocks/Grassy_Side.png");
            _purpleFill  = Load("Blocks/Dirt_Purple_Fill.png");
            _purpleSide  = Load("Blocks/Dirt_Purple_Side.png");
            _galaxy1     = Load("Blocks/Galaxy_Block_1.png");
            _galaxy2     = Load("Blocks/Galaxy_Block_2.png");
            _brick       = Load("Blocks/Brick_Purple.png");
            _starryBg    = Load("Background/Starry_Night_Big.png");
            _galaxyBg    = Load("Background/GalaxyBackground.png");
            _layer1      = Load("Background/Layer1.png");
        }

        private static Sprite Load(string rel) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{MaterialsRoot}/{rel}");

        private static void CreateCamera(Color bg, float ortho)
        {
            GameObject cam = new("Main Camera");
            cam.tag = "MainCamera";
            Camera c = cam.AddComponent<Camera>();
            c.orthographic     = true;
            c.orthographicSize = ortho;
            c.backgroundColor  = bg;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.AddComponent<AudioListener>();
        }

        private static void CreateChild(GameObject parent, string name, System.Type type)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent.transform);
            child.AddComponent(type);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void EnsureSceneInBuildSettings(string path)
        {
            if (!File.Exists(path)) return;
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
                if (s.path == path) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ── PlatformDef factories ─────────────────────────────────────────

        private static PlatformDef Plat(string name, Vector2 c, int w, int h,
            TileStyle style, bool oneWay) =>
            new() { Name = name, Center = c, Width = w, Height = h,
                    Style = style, OneWay = oneWay };

        private static PlatformDef MovingPlat(string name, Vector2 c, int w, int h,
            TileStyle style, Vector2 a, Vector2 b, float speed) =>
            new() { Name = name, Center = c, Width = w, Height = h, Style = style,
                    OneWay = true, Moving = true, MoveA = a, MoveB = b, MoveSpeed = speed };

        private static PlatformDef DestrPlat(string name, Vector2 c, int w) =>
            new() { Name = name, Center = c, Width = w, Height = 1,
                    Style = TileStyle.Purple, OneWay = true, Destructible = true };

        private static SpawnDef Spawn(string name, Vector2 pos) =>
            new() { Name = name, Position = pos, IsPlayer = false };

        private static SpawnDef Player(string name, Vector2 pos) =>
            new() { Name = name, Position = pos, IsPlayer = true };
    }
}
#endif
