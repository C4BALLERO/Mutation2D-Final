#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MutationSwarm.Editor
{
    internal static class MutationSwarmPlayerAnimatorSetup
    {
        private const string AnimRoot = "Assets/_Art/Animations/Player";
        private const string ControllerPath = AnimRoot + "/AC_Player.controller";

        public static RuntimeAnimatorController BuildFromSheet(string sheetPath, int frameW, int frameH)
        {
            EnsureFolder(AnimRoot);
            Sprite[] sprites = LoadSheetSprites(sheetPath, frameW, frameH);
            if (sprites.Length < PlayerWalkFrameGenerator.IdleFrames + PlayerWalkFrameGenerator.WalkFrames)
            {
                Debug.LogError("[MutationSwarm] No se pudieron cortar sprites del sheet.");
                return null;
            }

            AnimationClip idle = CreateSpriteClip("Player_Idle", sprites, 0, PlayerWalkFrameGenerator.IdleFrames, 4f, true);
            AnimationClip walk = CreateSpriteClip("Player_Walk", sprites, PlayerWalkFrameGenerator.IdleFrames,
                PlayerWalkFrameGenerator.WalkFrames, 10f, true);
            AnimationClip jump = CreateSpriteClip("Player_Jump", sprites, 0, 1, 1f, false);
            AnimationClip fall = CreateSpriteClip("Player_Fall", sprites, 0, 1, 1f, true);
            AnimationClip dash = CreateSpriteClip("Player_Dash", sprites, 0, 1, 1f, false);
            AnimationClip attack = CreateSpriteClip("Player_Attack", sprites, 0, 1, 8f, false);
            AnimationClip hit = CreateSpriteClip("Player_Hit", sprites, 0, 1, 6f, false);
            AnimationClip die = CreateSpriteClip("Player_Die", sprites, 0, 1, 1f, false);

            SaveClip(idle);
            SaveClip(walk);
            SaveClip(jump);
            SaveClip(fall);
            SaveClip(dash);
            SaveClip(attack);
            SaveClip(hit);
            SaveClip(die);

            return BuildController(idle, walk, jump, fall, dash, attack, hit, die);
        }

        private static Sprite[] LoadSheetSprites(string sheetPath, int frameW, int frameH)
        {
            TextureImporter imp = AssetImporter.GetAtPath(sheetPath) as TextureImporter;
            if (imp == null)
                return System.Array.Empty<Sprite>();

            int total = PlayerWalkFrameGenerator.IdleFrames + PlayerWalkFrameGenerator.WalkFrames;
            var metas = new List<SpriteMetaData>();
            for (int i = 0; i < total; i++)
            {
                metas.Add(new SpriteMetaData
                {
                    name = $"player_frame_{i:D2}",
                    rect = new Rect(i * frameW, 0, frameW, frameH),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(0.5f, 0.35f)
                });
            }

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Multiple;
            imp.spritePixelsPerUnit = MutationSwarmPlayerArtSetup.PixelsPerUnit;
            imp.filterMode = FilterMode.Point;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.spritesheet = metas.ToArray();
            imp.SaveAndReimport();

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            var list = new List<Sprite>();
            foreach (Object o in assets)
            {
                if (o is Sprite s)
                    list.Add(s);
            }

            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list.ToArray();
        }

        private static AnimationClip CreateSpriteClip(string name, Sprite[] sprites, int start, int count, float fps, bool loop)
        {
            AnimationClip clip = new();
            clip.name = name;
            EditorCurveBinding binding = new()
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            var keys = new ObjectReferenceKeyframe[count];
            float frameTime = 1f / fps;
            for (int i = 0; i < count; i++)
            {
                int idx = Mathf.Min(start + i, sprites.Length - 1);
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameTime,
                    value = sprites[idx]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            clip.frameRate = fps;
            clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static void SaveClip(AnimationClip clip)
        {
            string path = $"{AnimRoot}/{clip.name}.anim";
            AssetDatabase.CreateAsset(clip, path);
        }

        private static AnimatorController BuildController(
            AnimationClip idle, AnimationClip walk, AnimationClip jump, AnimationClip fall,
            AnimationClip dash, AnimationClip attack, AnimationClip hit, AnimationClip die)
        {
            if (File.Exists(ControllerPath))
                AssetDatabase.DeleteAsset(ControllerPath);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            AddParam(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
            AddParam(controller, "IsRunning", AnimatorControllerParameterType.Bool);
            AddParam(controller, "IsFalling", AnimatorControllerParameterType.Bool);
            AddParam(controller, "Jump", AnimatorControllerParameterType.Trigger);
            AddParam(controller, "Dash", AnimatorControllerParameterType.Trigger);
            AddParam(controller, "Attack", AnimatorControllerParameterType.Trigger);
            AddParam(controller, "Hit", AnimatorControllerParameterType.Trigger);
            AddParam(controller, "Die", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState stIdle = sm.AddState("Idle", new Vector3(300, 0, 0));
            stIdle.motion = idle;
            AnimatorState stWalk = sm.AddState("Walk", new Vector3(300, 80, 0));
            stWalk.motion = walk;
            AnimatorState stFall = sm.AddState("Fall", new Vector3(300, 160, 0));
            stFall.motion = fall;
            AnimatorState stJump = sm.AddState("Jump", new Vector3(520, 80, 0));
            stJump.motion = jump;
            AnimatorState stDash = sm.AddState("Dash", new Vector3(520, 0, 0));
            stDash.motion = dash;
            AnimatorState stAttack = sm.AddState("Attack", new Vector3(520, 160, 0));
            stAttack.motion = attack;
            AnimatorState stHit = sm.AddState("Hit", new Vector3(520, 240, 0));
            stHit.motion = hit;
            AnimatorState stDie = sm.AddState("Die", new Vector3(300, 240, 0));
            stDie.motion = die;

            sm.defaultState = stIdle;

            AnyStateTo(sm, stDie, "Die");
            AnyStateTo(sm, stHit, "Hit", canTransitionToSelf: true);

            Transition(stIdle, stWalk, AnimatorConditionMode.If, 1f, "IsRunning");
            Transition(stWalk, stIdle, AnimatorConditionMode.IfNot, 0f, "IsRunning");
            Transition(stIdle, stFall, AnimatorConditionMode.If, 1f, "IsFalling");
            Transition(stWalk, stFall, AnimatorConditionMode.If, 1f, "IsFalling");
            AnimatorStateTransition fallToIdle = stFall.AddTransition(stIdle);
            fallToIdle.hasExitTime = false;
            fallToIdle.duration = 0.08f;
            fallToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsFalling");
            fallToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsRunning");

            AnimatorStateTransition fallToWalk = stFall.AddTransition(stWalk);
            fallToWalk.hasExitTime = false;
            fallToWalk.duration = 0.08f;
            fallToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsFalling");
            fallToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsRunning");

            TriggerFromAny(sm, stJump, "Jump", new[] { stIdle, stWalk, stFall });
            TriggerFromAny(sm, stDash, "Dash", new[] { stIdle, stWalk, stFall, stJump });
            TriggerFromAny(sm, stAttack, "Attack", new[] { stIdle, stWalk, stFall });

            ReturnToIdle(stJump, stIdle);
            ReturnToIdle(stDash, stIdle);
            ReturnToIdle(stAttack, stIdle);
            ReturnToIdle(stHit, stIdle, 0.25f);

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void AddParam(AnimatorController c, string name, AnimatorControllerParameterType type)
        {
            if (System.Array.Exists(c.parameters, p => p.name == name))
                return;
            c.AddParameter(name, type);
        }

        private static void Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string param,
            bool? extra = null, string extraParam = "IsRunning")
        {
            AnimatorStateTransition t = from.AddTransition(to);
            t.hasExitTime = false;
            t.duration = 0.08f;
            t.AddCondition(mode, threshold, param);
            if (extra.HasValue)
                t.AddCondition(extra.Value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, extraParam);
        }

        private static void AnyStateTo(AnimatorStateMachine sm, AnimatorState target, string trigger, bool canTransitionToSelf = false)
        {
            AnimatorStateTransition t = sm.AddAnyStateTransition(target);
            t.hasExitTime = false;
            t.duration = 0.05f;
            t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            t.canTransitionToSelf = canTransitionToSelf;
        }

        private static void TriggerFromAny(AnimatorStateMachine sm, AnimatorState target, string trigger, AnimatorState[] fromStates)
        {
            foreach (ChildAnimatorState child in sm.states)
            {
                if (child.state == target || child.state.motion == null)
                    continue;
                bool allowed = false;
                foreach (AnimatorState s in fromStates)
                {
                    if (child.state == s)
                    {
                        allowed = true;
                        break;
                    }
                }

                if (!allowed)
                    continue;

                AnimatorStateTransition t = child.state.AddTransition(target);
                t.hasExitTime = false;
                t.duration = 0.06f;
                t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            }
        }

        private static void ReturnToIdle(AnimatorState from, AnimatorState idle, float exitTime = 0.85f)
        {
            AnimatorStateTransition t = from.AddTransition(idle);
            t.hasExitTime = true;
            t.exitTime = exitTime;
            t.duration = 0.1f;
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
