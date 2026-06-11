#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MutationSwarm.Editor
{
    public static class MutationSwarmPlayerSpriteImporter
    {
        private const string AnimRoot = "Assets/_Art/Animations/Player";
        private const string PrefabRoot = "Assets/_Prefabs/Player";
        private const string ControllerPath = AnimRoot + "/AC_Player.controller";
        private const string LogPath = "Logs/player-images-info.txt";

        [MenuItem("Tools/Mutation Swarm/Inspect Player Sprite Sheets")]
        public static void InspectImages()
        {
            string logDir = Path.GetDirectoryName(LogPath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            using (StreamWriter sw = new StreamWriter(LogPath, false))
            {
                sw.WriteLine("=== PLAYER SPRITE SHEET INSPECTION ===");
                Debug.Log("=== PLAYER SPRITE SHEET INSPECTION ===");

                string[] files = { "Dahs_Player.png", "Idle_Player.png", "Jump_Player.png", "Walk_Player.png", "Posicion_Arma.png" };
                foreach (string file in files)
                {
                    string fullPath = Path.Combine(Application.dataPath, "_Art/Animations/Player", file);
                    if (File.Exists(fullPath))
                    {
                        byte[] bytes = File.ReadAllBytes(fullPath);
                        Texture2D tex = new Texture2D(2, 2);
                        if (tex.LoadImage(bytes))
                        {
                            string msg = $"{file}: {tex.width}x{tex.height}";
                            sw.WriteLine(msg);
                            Debug.Log(msg);
                        }
                        else
                        {
                            string msg = $"{file}: Failed to load image data";
                            sw.WriteLine(msg);
                            Debug.LogError(msg);
                        }
                        Object.DestroyImmediate(tex);
                    }
                    else
                    {
                        string msg = $"{file}: NOT FOUND at {fullPath}";
                        sw.WriteLine(msg);
                        Debug.LogWarning(msg);
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Inspection completed. Results written to {LogPath}");
        }

        [MenuItem("Tools/Mutation Swarm/Import & Slice Player Sprite Sheets")]
        public static void ImportAndSlicePlayer()
        {
            Debug.Log("=== STARTING PLAYER SPRITE IMPORT & SLICE SYSTEM ===");

            // Paths to the sliced png sheets
            string idlePath = $"{AnimRoot}/Idle_Player.png";
            string walkPath = $"{AnimRoot}/Walk_Player.png";
            string jumpPath = $"{AnimRoot}/Jump_Player.png";
            string dashPath = $"{AnimRoot}/Dahs_Player.png";
            string weaponPath = $"{AnimRoot}/Posicion_Arma.png";

            // Load and sort sprites from each sheet
            Sprite[] idleSprites = LoadAndSortSprites(idlePath);
            Sprite[] walkSprites = LoadAndSortSprites(walkPath);
            Sprite[] jumpSprites = LoadAndSortSprites(jumpPath);
            Sprite[] dashSprites = LoadAndSortSprites(dashPath);
            Sprite[] weaponSprites = LoadAndSortSprites(weaponPath);

            Debug.Log($"[Importer] Sliced frames loaded: Idle({idleSprites.Length}), Walk({walkSprites.Length}), Jump({jumpSprites.Length}), Dash({dashSprites.Length}), Weapon({weaponSprites.Length})");

            if (idleSprites.Length == 0 || walkSprites.Length == 0 || jumpSprites.Length == 0 || dashSprites.Length == 0 || weaponSprites.Length == 0)
            {
                Debug.LogError("[Importer] One or more player sprite sheets could not be loaded or sliced. Make sure they are fully imported.");
                return;
            }

            // Generate or overwrite the AnimationClips
            Debug.Log("[Importer] Generating animation clips...");
            AnimationClip idleClip = CreateSpriteClip("Player_Idle", idleSprites, 0, idleSprites.Length, 12f, true);
            AnimationClip walkClip = CreateSpriteClip("Player_Walk", walkSprites, 0, walkSprites.Length, 15f, true);
            
            // Jump sheet contains rise and fall sequence. Let's split it half-and-half.
            int halfJump = jumpSprites.Length / 2;
            AnimationClip jumpClip = CreateSpriteClip("Player_Jump", jumpSprites, 0, halfJump, 12f, false);
            AnimationClip fallClip = CreateSpriteClip("Player_Fall", jumpSprites, halfJump, jumpSprites.Length - halfJump, 12f, true);
            
            AnimationClip dashClip = CreateSpriteClip("Player_Dash", dashSprites, 0, dashSprites.Length, 24f, false);
            
            // Attack uses the Weapon Position sheet
            AnimationClip attackClip = CreateSpriteClip("Player_Attack", weaponSprites, 0, weaponSprites.Length, 18f, false);
            
            // Hit uses a subset of Idle sprites (e.g., frames 10-15) for a quick reaction
            AnimationClip hitClip = CreateSpriteClip("Player_Hit", idleSprites, Mathf.Min(10, idleSprites.Length - 1), Mathf.Min(6, idleSprites.Length), 12f, false);
            
            // Die uses the last few frames of Jump (lying down / falling)
            int dieStart = Mathf.Max(0, jumpSprites.Length - 8);
            AnimationClip dieClip = CreateSpriteClip("Player_Die", jumpSprites, dieStart, jumpSprites.Length - dieStart, 10f, false);

            SaveClip(idleClip);
            SaveClip(walkClip);
            SaveClip(jumpClip);
            SaveClip(fallClip);
            SaveClip(dashClip);
            SaveClip(attackClip);
            SaveClip(hitClip);
            SaveClip(dieClip);

            // Rebuild the Animator Controller
            Debug.Log("[Importer] Building Animator Controller...");
            RuntimeAnimatorController controller = BuildController(idleClip, walkClip, jumpClip, fallClip, dashClip, attackClip, hitClip, dieClip);

            // Update the Player prefabs
            Debug.Log("[Importer] Updating player prefabs...");
            Sprite defaultIdleSprite = idleSprites[0];
            UpdatePlayerPrefabs(defaultIdleSprite, controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("=== PLAYER SPRITE IMPORT & SLICE SYSTEM COMPLETED SUCCESSFULLY ===");
        }

        private static Sprite[] LoadAndSortSprites(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            List<Sprite> sprites = new List<Sprite>();
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }

            // Sort by numerical index at the end of the name (e.g. Walk_Player_5 -> 5)
            sprites.Sort((a, b) =>
            {
                int indexA = GetFrameIndex(a.name);
                int indexB = GetFrameIndex(b.name);
                return indexA.CompareTo(indexB);
            });

            return sprites.ToArray();
        }

        private static int GetFrameIndex(string name)
        {
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore >= 0 && lastUnderscore < name.Length - 1)
            {
                string suffix = name.Substring(lastUnderscore + 1);
                if (int.TryParse(suffix, out int result))
                {
                    return result;
                }
            }
            return 0;
        }

        private static AnimationClip CreateSpriteClip(string name, Sprite[] sprites, int start, int count, float fps, bool loop)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            
            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[count];
            float frameTime = 1f / fps;
            for (int i = 0; i < count; i++)
            {
                int idx = Mathf.Clamp(start + i, 0, sprites.Length - 1);
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
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
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

        private static void Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string param)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            t.hasExitTime = false;
            t.duration = 0.08f;
            t.AddCondition(mode, threshold, param);
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

        private static void UpdatePlayerPrefabs(Sprite idleSprite, RuntimeAnimatorController controller)
        {
            // Update Prefab_Player.prefab
            string mainPrefabPath = $"{PrefabRoot}/Prefab_Player.prefab";
            if (File.Exists(mainPrefabPath))
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(mainPrefabPath);
                
                SpriteRenderer sr = contents.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = idleSprite;
                    Debug.Log($"[Importer] Updated SpriteRenderer sprite on Prefab_Player to: {idleSprite.name}");
                }

                Animator anim = contents.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.runtimeAnimatorController = controller;
                    Debug.Log("[Importer] Updated AnimatorController on Prefab_Player");
                }

                PrefabUtility.SaveAsPrefabAsset(contents, mainPrefabPath);
                PrefabUtility.UnloadPrefabContents(contents);
            }
            else
            {
                Debug.LogWarning($"[Importer] Prefab not found: {mainPrefabPath}");
            }

            // Update Prefab_Player_Geo.prefab
            string geoPrefabPath = $"{PrefabRoot}/Prefab_Player_Geo.prefab";
            if (File.Exists(geoPrefabPath))
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(geoPrefabPath);

                Animator anim = contents.GetComponent<Animator>();
                if (anim == null)
                {
                    anim = contents.AddComponent<Animator>();
                }
                anim.runtimeAnimatorController = controller;
                Debug.Log("[Importer] Updated AnimatorController on Prefab_Player_Geo");

                PrefabUtility.SaveAsPrefabAsset(contents, geoPrefabPath);
                PrefabUtility.UnloadPrefabContents(contents);
            }
            else
            {
                Debug.LogWarning($"[Importer] Prefab not found: {geoPrefabPath}");
            }
        }
    }
}
#endif
