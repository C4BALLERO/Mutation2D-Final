#if UNITY_EDITOR
using System.IO;
using MutationSwarm.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static MutationSwarm.Editor.EnemyAnimationSpriteFactory;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Unity Editor tool — generates all enemy animation spritesheets, clips,
    /// Animator Controllers, and wires them into the enemy prefabs.
    ///
    /// Run via: Tools > Mutation Swarm > Build Enemy Animations (Full Pipeline)
    /// </summary>
    public static class MutationSwarmEnemyAnimationSetup
    {
        private const string SpritesPath  = "Assets/_Art/Sprites/Enemies";
        private const string AnimsPath    = "Assets/_Art/Animations/Enemies";
        private const string PrefabsPath  = "Assets/_Prefabs/Enemies";
        private const int    FrameSize    = 64;
        private const float  PivotY      = 0.36f;

        // Animation playback speeds (fps)
        private const float IdleFps   = 8f;
        private const float MoveFps   = 10f;
        private const float AttackFps = 14f;
        private const float HitFps    = 16f;
        private const float DieFps    = 8f;

        private static readonly (EnemySpriteFactory.Archetype arch, string name, string prefabName)[] Enemies =
        {
            (EnemySpriteFactory.Archetype.Drone,    "Drone",    "Prefab_Enemy_Drone"),
            (EnemySpriteFactory.Archetype.Boss,     "Boss",     "Prefab_Enemy_Boss"),
            (EnemySpriteFactory.Archetype.Queen,    "Queen",    "Prefab_Enemy_Queen"),
            (EnemySpriteFactory.Archetype.Mimic,    "Mimic",    "Prefab_Enemy_Mimic"),
            (EnemySpriteFactory.Archetype.Parasite, "Parasite", "Prefab_Enemy_Parasite"),
        };

        [MenuItem("Tools/Mutation Swarm/Build Enemy Animations (Full Pipeline)")]
        public static void BuildAll()
        {
            EnsureFolder(SpritesPath);
            EnsureFolder(AnimsPath);

            foreach (var (arch, name, prefabName) in Enemies)
            {
                AnimationClip idleClip   = BuildStateClip(arch, name, AnimState.Idle,   IdleFps,   WrapMode.Loop);
                AnimationClip moveClip   = BuildStateClip(arch, name, AnimState.Move,   MoveFps,   WrapMode.Loop);
                AnimationClip attackClip = BuildStateClip(arch, name, AnimState.Attack, AttackFps, WrapMode.Once);
                AnimationClip hitClip    = BuildStateClip(arch, name, AnimState.Hit,    HitFps,    WrapMode.Once);
                AnimationClip dieClip    = BuildStateClip(arch, name, AnimState.Die,    DieFps,    WrapMode.ClampForever);

                AnimatorController controller = BuildAnimatorController(
                    name, idleClip, moveClip, attackClip, hitClip, dieClip);

                WirePrefab(prefabName, controller);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] Enemy animation pipeline complete.");
        }

        // ── Spritesheet + clip ───────────────────────────────────────────────

        private static AnimationClip BuildStateClip(
            EnemySpriteFactory.Archetype arch,
            string enemyName,
            AnimState state,
            float fps,
            WrapMode wrapMode)
        {
            // 1. Generate pixel frames
            Color[][] frames = GenerateFrames(arch, state, FrameSize);

            // 2. Build and save spritesheet PNG
            Texture2D sheet = BuildSpritesheet(frames, FrameSize);
            string pngPath  = $"{SpritesPath}/Spr_Enemy_{enemyName}_{state}.png";
            File.WriteAllBytes(pngPath, sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);

            // 3. Import and configure slicing
            AssetDatabase.ImportAsset(pngPath);
            Sprite[] sprites = ConfigureSlicing(pngPath, frames.Length, enemyName, state);

            // 4. Create Animation Clip
            AnimationClip clip = new();
            clip.name      = $"Enemy_{enemyName}_{state}";
            clip.wrapMode  = wrapMode;
            clip.frameRate = fps;

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keyframes = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
            {
                keyframes[i].time  = i / fps;
                keyframes[i].value = sprites[i];
            }
            // Closing keyframe: loop point or hold last frame
            keyframes[frames.Length].time  = frames.Length / fps;
            keyframes[frames.Length].value = wrapMode == WrapMode.Loop ? sprites[0] : sprites[^1];

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Set clip loop settings
            SerializedObject so = new(clip);
            SerializedProperty settings = so.FindProperty("m_AnimationClipSettings");
            settings.FindPropertyRelative("m_LoopTime").boolValue = wrapMode == WrapMode.Loop;
            so.ApplyModifiedProperties();

            string clipPath = $"{AnimsPath}/Enemy_{enemyName}_{state}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            return clip;
        }

        private static Sprite[] ConfigureSlicing(string pngPath, int frameCount, string enemyName, AnimState state)
        {
            TextureImporter imp = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (imp == null) return new Sprite[0];

            imp.textureType          = TextureImporterType.Sprite;
            imp.spriteImportMode     = SpriteImportMode.Multiple;
            imp.spritePixelsPerUnit  = 64;
            imp.filterMode           = FilterMode.Point;
            imp.textureCompression   = TextureImporterCompression.Uncompressed;
            imp.alphaIsTransparency  = true;

            var meta = new SpriteMetaData[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                meta[i] = new SpriteMetaData
                {
                    name      = $"Enemy_{enemyName}_{state}_{i}",
                    rect      = new Rect(i * FrameSize, 0, FrameSize, FrameSize),
                    pivot     = new Vector2(0.5f, PivotY),
                    alignment = (int)SpriteAlignment.Custom,
                };
            }
            imp.spritesheet = meta;
            imp.SaveAndReimport();

            var sprites = new Sprite[frameCount];
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(pngPath);
            int idx = 0;
            foreach (Object asset in assets)
            {
                if (asset is Sprite s && idx < frameCount)
                    sprites[idx++] = s;
            }
            return sprites;
        }

        // ── Animator Controller ──────────────────────────────────────────────

        private static AnimatorController BuildAnimatorController(
            string enemyName,
            AnimationClip idle, AnimationClip move,
            AnimationClip attack, AnimationClip hit, AnimationClip die)
        {
            string controllerPath = $"{AnimsPath}/AC_Enemy_{enemyName}.controller";

            // Overwrite if exists
            if (File.Exists(controllerPath))
                AssetDatabase.DeleteAsset(controllerPath);

            AnimatorController ctrl =
                AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            // Parameters
            ctrl.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Attack",   AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Hit",      AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Die",      AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = ctrl.layers[0].stateMachine;

            // States
            AnimatorState idleState   = sm.AddState("Idle");
            idleState.motion = idle;
            AnimatorState moveState   = sm.AddState("Move");
            moveState.motion = move;
            AnimatorState attackState = sm.AddState("Attack");
            attackState.motion = attack;
            AnimatorState hitState    = sm.AddState("Hit");
            hitState.motion = hit;
            AnimatorState dieState    = sm.AddState("Die");
            dieState.motion = die;

            sm.defaultState = idleState;

            // Idle <-> Move via IsMoving bool
            AddTransition(idleState, moveState, "IsMoving", true,  hasExit: false);
            AddTransition(moveState, idleState, "IsMoving", false, hasExit: false);

            // Attack → Idle (after exit time)
            var atToIdle = attackState.AddTransition(idleState);
            atToIdle.hasExitTime = true;
            atToIdle.exitTime    = 0.85f;
            atToIdle.duration    = 0.05f;

            // Hit → Idle (after exit time)
            var hitToIdle = hitState.AddTransition(idleState);
            hitToIdle.hasExitTime = true;
            hitToIdle.exitTime    = 0.85f;
            hitToIdle.duration    = 0.05f;

            // AnyState transitions are evaluated top-to-bottom.
            // Add in priority order: Die (highest) → Hit → Attack (lowest).

            var anyDie = sm.AddAnyStateTransition(dieState);
            anyDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
            anyDie.hasExitTime         = false;
            anyDie.duration            = 0.03f;
            anyDie.canTransitionToSelf = false;

            var anyHit = sm.AddAnyStateTransition(hitState);
            anyHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
            anyHit.hasExitTime         = false;
            anyHit.duration            = 0.03f;
            anyHit.canTransitionToSelf = false;

            var anyAttack = sm.AddAnyStateTransition(attackState);
            anyAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            anyAttack.hasExitTime         = false;
            anyAttack.duration            = 0.05f;
            anyAttack.canTransitionToSelf = false;

            EditorUtility.SetDirty(ctrl);
            AssetDatabase.SaveAssets();
            return ctrl;
        }

        private static void AddTransition(
            AnimatorState from, AnimatorState to,
            string param, bool value, bool hasExit)
        {
            var t = from.AddTransition(to);
            t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
            t.hasExitTime = hasExit;
            t.duration    = 0.1f;
        }

        // ── Prefab wiring ────────────────────────────────────────────────────

        private static void WirePrefab(string prefabName, AnimatorController controller)
        {
            string prefabPath = $"{PrefabsPath}/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[MutationSwarm] Prefab not found: {prefabPath}. Run Build Enemy Sprites first.");
                return;
            }

            string editablePath = prefabPath;
            GameObject root = PrefabUtility.LoadPrefabContents(editablePath);

            Animator animator = root.GetComponent<Animator>();
            if (animator == null)
                animator = root.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            PrefabUtility.SaveAsPrefabAsset(root, editablePath);
            PrefabUtility.UnloadPrefabContents(root);

            Debug.Log($"[MutationSwarm] Wired animator to {prefabName}.");
        }

        // ── Utilities ────────────────────────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name   = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
