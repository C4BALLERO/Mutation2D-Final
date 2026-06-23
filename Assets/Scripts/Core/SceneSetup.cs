#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MutationSwarm
{
    public class SceneSetup : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("MutationSwarm/Setup Scene")]
        static void Setup() => new GameObject("__SETUP__").AddComponent<SceneSetup>().BuildScene();
#endif

        void Start() { BuildScene(); Destroy(gameObject); }

        void BuildScene()
        {
            EnsureLayer("Ground");
            EnsureLayer("Platform");
            EnsureLayer("Enemy");
            EnsureLayer("Defense");

            ClearExisting();

            var managers = new GameObject("Managers");
            managers.AddComponent<GameManager>();
            managers.AddComponent<EvolutionSystem>();
            managers.AddComponent<ParticleManager>();
            managers.AddComponent<AudioManager>();

            var sf = new GameObject("SpriteFactory");
            sf.transform.SetParent(managers.transform);
            sf.AddComponent<SpriteFactory>();

            var camGO = GameObject.Find("Main Camera") ?? new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.GetOrAddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.07f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0, 2f, -10f);
            var cf = camGO.GetOrAddComponent<CameraFollow>();
            cf.minX = -14f; cf.maxX = 14f; cf.minY = -1f; cf.maxY = 9f;
            camGO.GetOrAddComponent<EnemyRenderer>();

            var level = new GameObject("Level");
            BuildBackground(level.transform);
            BuildPlatforms(level.transform);

            var enemiesGO = new GameObject("Enemies");
            new GameObject("Projectiles");
            var defensesGO = new GameObject("Defenses");

            var bulletPrefab    = BuildBulletPrefab();
            var enemyPrefab     = BuildEnemyPrefab();
            var barricadePrefab = BuildBarricadePrefab();
            var turretPrefab    = BuildTurretPrefab();
            var minePrefab      = BuildMinePrefab();
            var fireballPrefab  = BuildFireballPrefab();
            EnemyBase.FireballPrefab = fireballPrefab;

            bulletPrefab.SetActive(false);
            enemyPrefab.SetActive(false);
            barricadePrefab.SetActive(false);
            turretPrefab.SetActive(false);
            minePrefab.SetActive(false);
            fireballPrefab.SetActive(false);

            var player = BuildPlayer(bulletPrefab);

            // Drone (POLLO) - chicken sprite sheet if available
            var droneGO = new GameObject("Drone");
            droneGO.transform.SetParent(player.transform);
            var droneComp = droneGO.AddComponent<Drone>();
            droneComp.bulletPrefab = bulletPrefab;
            droneComp.chickenFrames = LoadFrames("Assets/_Art/Imported/Drone/PolloDron_Fly.png");

            var wmGO = new GameObject("WaveManager");
            wmGO.transform.SetParent(managers.transform);
            var wm = wmGO.AddComponent<WaveManager>();
            wm.enemyPrefab = enemyPrefab;
            wm.enemiesContainer = enemiesGO.transform;
            wm.spawnX = 16f; wm.spawnY = 0.5f;

            var dbGO = new GameObject("DefenseBuilder");
            dbGO.transform.SetParent(player.transform);
            var db = dbGO.AddComponent<DefenseBuilder>();
            db.barricadePrefab = barricadePrefab;
            db.turretPrefab    = turretPrefab;
            db.minePrefab      = minePrefab;
            db.defensesContainer = defensesGO.transform;

            BuildUI();

            Debug.Log("[MutationSwarm] Scene built successfully. Press Play!");

#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
        }
        void ClearExisting()
        {
            string[] keep = { "Main Camera", "Directional Light" };
            var toDestroy = new List<GameObject>();
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                bool doKeep = false;
                foreach (var k in keep) if (root.name == k) doKeep = true;
                if (!doKeep) toDestroy.Add(root);
            }
            foreach (var g in toDestroy)
            {
#if UNITY_EDITOR
                DestroyImmediate(g);
#else
                Destroy(g);
#endif
            }
        }

        void BuildBackground(Transform parent)
        {
            var bgSprite = LoadSingleSprite("Assets/_Art/Imported/Environment/Background.png");
            var bg = new GameObject("Background");
            bg.transform.SetParent(parent);
            bg.transform.position = new Vector3(0, 4f, 5f);
            var sr = bg.AddComponent<SpriteRenderer>();
            if (bgSprite != null)
            {
                sr.sprite = bgSprite;
                sr.color = new Color(0.85f, 0.85f, 0.9f); // leve atenuación para que el primer plano resalte
                bg.AddComponent<BackgroundFollow>(); // mantiene el fondo cubriendo la cámara
            }
            else
            {
                sr.sprite = MakeGradientSprite();
                bg.transform.localScale = new Vector3(40f, 20f, 1f);
            }
            sr.sortingOrder = -10;
        }

        static Sprite MakeGradientSprite()
        {
            int h = 64, w = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                var c = Color.Lerp(new Color(0.03f, 0.04f, 0.06f), new Color(0.06f, 0.07f, 0.10f), t);
                for (int x = 0; x < w; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 1);
        }

        static readonly (float x, float y, float w)[] PlatformData =
        {
            (  0f, -1.5f, 40f ),
            ( -8f,  1.0f,  5.5f ),
            (  1f,  2.5f,  4.5f ),
            (  8f,  1.5f,  5f   ),
            ( -3f,  4f,    4.5f ),
            (  6f,  4.5f,  4f   ),
            (-10f,  3f,    3.5f ),
            ( 10f,  3.5f,  3.5f ),
            (  0f,  6f,    3f   ),
        };

        void BuildPlatforms(Transform parent)
        {
            var brickSprite = LoadSingleSprite("Assets/_Art/Imported/Environment/Bricks.png");
            for (int i = 0; i < PlatformData.Length; i++)
            {
                var (px, py, pw) = PlatformData[i];
                bool isGround = i == 0;
                var go = new GameObject(isGround ? "Ground" : string.Format("Platform_{0}", i));
                go.transform.SetParent(parent);
                go.transform.position = new Vector3(px, py, 0);
                go.layer = LayerMask.NameToLayer(isGround ? "Ground" : "Platform");
                go.tag = isGround ? "Ground" : "Platform";

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = brickSprite != null ? brickSprite
                          : (SpriteFactory.Instance != null ? SpriteFactory.Instance.PlatformSprite : CreateWhiteSprite());
                sr.sortingOrder = 0;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(pw, isGround ? 2f : 0.5f);
                if (brickSprite != null)
                    sr.color = isGround ? new Color(0.7f, 0.68f, 0.78f) : Color.white; // ladrillos morados; suelo un poco más apagado
                else
                    sr.color = isGround ? new Color(0.18f, 0.12f, 0.22f) : new Color(0.23f, 0.19f, 0.30f);

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(pw, isGround ? 2f : 0.5f);
            }
        }

        GameObject BuildPlayer(GameObject bulletPrefab)
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.transform.position = new Vector3(0, 1.2f, 0);
            go.transform.localScale = Vector3.one;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3.5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.7f);
            col.offset = new Vector2(0f, 0f);

            go.AddComponent<PlayerStats>();
            go.AddComponent<PlayerController>();

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = Vector3.one * 0.38f;
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            sr.color = Color.white;
            visual.AddComponent<FrameAnimator>().fps = 14f;
            var pv = visual.AddComponent<PlayerVisual>();
            pv.idle = LoadFrames("Assets/_Art/Imported/Player/Idle_Player.png");
            pv.walk = LoadFrames("Assets/_Art/Imported/Player/Walk_Player.png");
            pv.jump = LoadFrames("Assets/_Art/Imported/Player/Jump_Player.png");
            pv.dash = LoadFrames("Assets/_Art/Imported/Player/Dahs_Player.png");
            if (pv.idle.Length > 0) sr.sprite = pv.idle[0];

            var muzzleGO = new GameObject("Muzzle");
            muzzleGO.transform.SetParent(go.transform);
            muzzleGO.transform.localPosition = new Vector3(0.5f, 0.15f, 0);

            var ps = go.AddComponent<PlayerShooter>();
            ps.bulletPrefab = bulletPrefab;
            ps.muzzle = muzzleGO.transform;

            return go;
        }

        static GameObject BuildBulletPrefab()
        {
            var go = new GameObject("BulletPrefab");
            var sr = go.AddComponent<SpriteRenderer>();
            var bulletSprite = LoadSingleSprite("Assets/_Art/Imported/Projectiles/BulletPlayer.png");
            sr.sprite = bulletSprite != null ? bulletSprite
                      : (SpriteFactory.Instance != null ? SpriteFactory.Instance.BulletSprite : CreateWhiteSprite());
            sr.sortingOrder = 8;
            sr.color = Color.white; // el sprite ya es dorado; Bullet.Init solo re-tinta las eléctricas
            go.transform.localScale = Vector3.one;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.28f;
            col.isTrigger = true;
            go.AddComponent<Bullet>();
            go.transform.SetParent(null);
            return go;
        }
        static GameObject BuildEnemyPrefab()
        {
            var go = new GameObject("EnemyPrefab");
            go.tag = "Enemy";
            go.layer = LayerMask.NameToLayer("Enemy");
            go.transform.localScale = Vector3.one;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            go.AddComponent<EnemyBase>();

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = Vector3.one * 0.26f;
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            visual.AddComponent<FrameAnimator>().fps = 12f;
            var ev = visual.AddComponent<EnemyVisual>();
            string E(string n) => "Assets/_Art/Imported/Enemies/" + n + ".png";
            ev.creatures = new[]
            {
                new EnemyVisual.CreatureFrames { walk = LoadFrames(E("Dino_Walk")),        attack = LoadFrames(E("Dino_Attack")),      death = LoadFrames(E("Dino_Death")) },
                new EnemyVisual.CreatureFrames { walk = LoadFrames(E("Mono_walk")),        attack = LoadFrames(E("Mono_Attack")),      death = LoadFrames(E("Mono_Dead")) },
                new EnemyVisual.CreatureFrames { walk = LoadFrames(E("enemy3_walk")),      attack = LoadFrames(E("enemy3_attack")),    death = LoadFrames(E("enemy3_death")) },
                new EnemyVisual.CreatureFrames { walk = LoadFrames(E("Diablito_volando")), attack = LoadFrames(E("Diablito_atacando")),death = LoadFrames(E("Diablito_dead")) },
            };
            if (ev.creatures[0].walk.Length > 0) sr.sprite = ev.creatures[0].walk[0];

            go.transform.SetParent(null);
            return go;
        }

        static Sprite[] LoadFrames(string pngPath)
        {
#if UNITY_EDITOR
            var sprites = AssetDatabase.LoadAllAssetsAtPath(pngPath)
                .OfType<Sprite>()
                .OrderBy(s => FrameNum(s.name))
                .ToArray();
            if (sprites.Length == 0) Debug.LogWarning($"[SceneSetup] No frames found at {pngPath}");
            return sprites;
#else
            return new Sprite[0];
#endif
        }

        static Sprite LoadSingleSprite(string pngPath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
#else
            return null;
#endif
        }

        static int FrameNum(string name)
        {
            int i = name.LastIndexOf('_');
            if (i >= 0 && i + 1 < name.Length && int.TryParse(name.Substring(i + 1), out int n)) return n;
            return 0;
        }

        static GameObject BuildBarricadePrefab()
        {
            var go = new GameObject("BarricadePrefab");
            go.tag = "Defense";
            var sr = go.AddComponent<SpriteRenderer>();
            var barricadeSprite = LoadSingleSprite("Assets/_Art/Imported/Defenses/Barricade.png");
            if (barricadeSprite != null)
            {
                sr.sprite = barricadeSprite;
                sr.color = Color.white;
                go.transform.localScale = Vector3.one;
            }
            else
            {
                sr.sprite = SpriteFactory.Instance != null ? SpriteFactory.Instance.PlatformSprite : CreateWhiteSprite();
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(3f, 1.5f);
                sr.color = new Color(0.4f, 0.3f, 0.2f);
            }
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();
            if (barricadeSprite != null) { col.size = new Vector2(2.9f, 1.25f); col.offset = new Vector2(0f, 0.73f); }
            else col.size = new Vector2(3f, 1.5f);
            go.AddComponent<DefenseBase>();
            go.transform.SetParent(null);
            return go;
        }

        static GameObject BuildTurretPrefab()
        {
            var go = new GameObject("TurretPrefab");
            go.tag = "Defense";
            var sr = go.AddComponent<SpriteRenderer>();
            var turretSprite = LoadSingleSprite("Assets/_Art/Imported/Defenses/Turret.png");
            if (turretSprite != null) { sr.sprite = turretSprite; sr.color = Color.white; go.transform.localScale = Vector3.one; }
            else
            {
                sr.sprite = SpriteFactory.Instance != null ? SpriteFactory.Instance.CircleSprite : CreateWhiteSprite();
                sr.color = new Color(0.19f, 0.25f, 0.38f);
                go.transform.localScale = Vector3.one * 1.2f;
            }
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();
            if (turretSprite != null) { col.size = new Vector2(1.3f, 1.35f); col.offset = new Vector2(0f, 0.78f); }
            else col.size = new Vector2(1.1f, 1.1f);
            go.AddComponent<DefenseBase>();
            go.transform.SetParent(null);
            return go;
        }

        static GameObject BuildMinePrefab()
        {
            var go = new GameObject("MinePrefab");
            go.tag = "Defense";
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Instance != null ? SpriteFactory.Instance.CircleSprite : CreateWhiteSprite();
            sr.sortingOrder = 3;
            sr.color = new Color(0.38f, 0.13f, 0.08f);
            go.transform.localScale = Vector3.one * 0.6f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;
            go.AddComponent<DefenseBase>();
            go.transform.SetParent(null);
            return go;
        }

        static GameObject BuildFireballPrefab()
        {
            var go = new GameObject("FireballPrefab");
            var sr = go.AddComponent<SpriteRenderer>();
            var fbSprite = LoadSingleSprite("Assets/_Art/Imported/Projectiles/Fireball.png");
            sr.sprite = fbSprite != null ? fbSprite
                      : (SpriteFactory.Instance != null ? SpriteFactory.Instance.FireballSprite : CreateWhiteSprite());
            sr.sortingOrder = 7;
            go.transform.localScale = Vector3.one * 0.55f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;
            col.isTrigger = true;
            go.AddComponent<FireballProjectile>();
            go.transform.SetParent(null);
            return go;
        }
        void BuildUI()
        {
            var canvas = new GameObject("Canvas");
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 20;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<GraphicRaycaster>();
            var ct = canvas.transform;

            // ── SPLASH SCREEN ──────────────────────────────────────────────
            var splashP = MakePanel(ct, "SplashScreen", new Vector2(1920, 1080), Vector2.zero, new Color(0f, 0f, 0f, 1f));
            FullStretch(splashP.GetComponent<RectTransform>());

            var logoSprite = LoadSingleSprite("Assets/_Art/UI/MutationLogo.png");
            if (logoSprite != null)
            {
                var logoGO = new GameObject("LogoImage");
                logoGO.transform.SetParent(splashP.transform, false);
                var logoImg = logoGO.AddComponent<Image>();
                logoImg.sprite = logoSprite;
                logoImg.preserveAspect = true;
                var logoRT = logoGO.GetComponent<RectTransform>();
                logoRT.anchorMin = new Vector2(0.5f, 0.5f);
                logoRT.anchorMax = new Vector2(0.5f, 0.5f);
                logoRT.pivot = new Vector2(0.5f, 0.5f);
                logoRT.sizeDelta = new Vector2(700, 350);
                logoRT.anchoredPosition = new Vector2(0, 40);
            }
            else
            {
                var t1 = MakeTMP(splashP.transform, "SplashTitle", new Vector2(0, 100), 96, new Color(0.15f, 1f, 0.2f));
                t1.text = "MUTATION";
                CenterAnchor(t1.GetComponent<RectTransform>()); t1.alignment = TextAlignmentOptions.Center;
                t1.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 120);
                var t2 = MakeTMP(splashP.transform, "SplashTitle2", new Vector2(0, -20), 64, new Color(0.1f, 0.85f, 0.15f));
                t2.text = "2D";
                CenterAnchor(t2.GetComponent<RectTransform>()); t2.alignment = TextAlignmentOptions.Center;
                t2.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 80);
            }

            var splashHint = MakeTMP(splashP.transform, "SplashHint", new Vector2(0, -220), 18, new Color(0.35f, 0.65f, 0.35f, 0.6f));
            splashHint.text = "Pulsa cualquier tecla para continuar";
            CenterAnchor(splashHint.GetComponent<RectTransform>()); splashHint.alignment = TextAlignmentOptions.Center;
            splashHint.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 35);

            // ── STORY SCREEN ───────────────────────────────────────────────
            var storyP = MakePanel(ct, "StoryScreen", new Vector2(1920, 1080), Vector2.zero, new Color(0.02f, 0.03f, 0.06f, 1f));
            FullStretch(storyP.GetComponent<RectTransform>());

            var borderGO = new GameObject("StoryAccent");
            borderGO.transform.SetParent(storyP.transform, false);
            var borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.1f, 0.55f, 0.15f, 0.6f);
            var borderRT = borderGO.GetComponent<RectTransform>();
            borderRT.anchorMin = new Vector2(0.08f, 0.2f);
            borderRT.anchorMax = new Vector2(0.08f, 0.85f);
            borderRT.pivot = new Vector2(0.5f, 0.5f);
            borderRT.sizeDelta = new Vector2(4, 0);

            var storyTextGO = new GameObject("StoryBodyText");
            storyTextGO.transform.SetParent(storyP.transform, false);
            var storyTMP = storyTextGO.AddComponent<TextMeshProUGUI>();
            storyTMP.fontSize = 28;
            storyTMP.color = new Color(0.85f, 0.95f, 0.85f);
            storyTMP.fontStyle = FontStyles.Normal;
            storyTMP.textWrappingMode = TMPro.TextWrappingModes.Normal;
            storyTMP.alignment = TextAlignmentOptions.Center;
            var storyBodyRT = storyTextGO.GetComponent<RectTransform>();
            storyBodyRT.anchorMin = new Vector2(0.1f, 0.22f);
            storyBodyRT.anchorMax = new Vector2(0.9f, 0.82f);
            storyBodyRT.offsetMin = Vector2.zero;
            storyBodyRT.offsetMax = Vector2.zero;

            var pageIndGO = new GameObject("StoryPageIndicator");
            pageIndGO.transform.SetParent(storyP.transform, false);
            var pageIndTMP = pageIndGO.AddComponent<TextMeshProUGUI>();
            pageIndTMP.fontSize = 15;
            pageIndTMP.color = new Color(0.38f, 0.6f, 0.38f, 0.7f);
            pageIndTMP.fontStyle = FontStyles.Bold;
            pageIndTMP.alignment = TextAlignmentOptions.Center;
            var pageRT = pageIndGO.GetComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0.5f, 0f);
            pageRT.anchorMax = new Vector2(0.5f, 0f);
            pageRT.pivot = new Vector2(0.5f, 0f);
            pageRT.sizeDelta = new Vector2(200, 30);
            pageRT.anchoredPosition = new Vector2(0, 55);

            var storyHintGO = new GameObject("StoryContinueHint");
            storyHintGO.transform.SetParent(storyP.transform, false);
            var storyHintTMP = storyHintGO.AddComponent<TextMeshProUGUI>();
            storyHintTMP.fontSize = 16;
            storyHintTMP.color = new Color(0.3f, 0.65f, 0.35f, 0.85f);
            storyHintTMP.fontStyle = FontStyles.Bold;
            storyHintTMP.alignment = TextAlignmentOptions.Center;
            storyHintTMP.text = "[ ESPACIO / ENTER / CLIC ] Continuar";
            var storyHintRT = storyHintGO.GetComponent<RectTransform>();
            storyHintRT.anchorMin = new Vector2(0.5f, 0f);
            storyHintRT.anchorMax = new Vector2(0.5f, 0f);
            storyHintRT.pivot = new Vector2(0.5f, 0f);
            storyHintRT.sizeDelta = new Vector2(700, 30);
            storyHintRT.anchoredPosition = new Vector2(0, 22);
            // ── HUD Panel ──────────────────────────────────────────────────
            var hudPanel = MakePanel(ct, "HUDPanel", new Vector2(260, 110), new Vector2(10, -10), new Color(0, 0, 0, 0.65f));
            var waveT   = MakeTMP(hudPanel.transform, "WaveText",  new Vector2(10, -12), 20, new Color(0.8f, 0.9f, 0.8f));
            var bestT   = MakeTMP(hudPanel.transform, "BestText",  new Vector2(160, -12), 14, new Color(0.67f, 0.73f, 0.53f));
            var dnaT    = MakeTMP(hudPanel.transform, "DNAText",   new Vector2(10, -34), 20, new Color(0f, 1f, 0.8f));
            var hpSlider   = MakeSlider(hudPanel.transform, "HPSlider",   new Vector2(10, -58), new Vector2(240, 14), new Color(0.13f, 0, 0),    new Color(0.27f, 1f, 0.13f), out var hpFillImg);
            var dashSlider = MakeSlider(hudPanel.transform, "DashSlider", new Vector2(10, -78), new Vector2(240, 8),  new Color(0, 0.1f, 0.2f), new Color(0.27f, 0.67f, 1f),  out _);

            var enemT = MakeTMP(ct, "EnemiesText", new Vector2(-20, 30), 16, new Color(1f, 0.4f, 0.27f));
            var er = enemT.GetComponent<RectTransform>();
            er.anchorMin = er.anchorMax = new Vector2(1f, 0f);
            er.pivot = new Vector2(1f, 0f);
            er.anchoredPosition = new Vector2(-20, 30);
            enemT.alignment = TextAlignmentOptions.Right;

            var domPanel = MakePanel(ct, "DominantPanel", new Vector2(500, 40), new Vector2(0, -10), new Color(0, 0, 0, 0.6f));
            var domPanelRT = domPanel.GetComponent<RectTransform>();
            domPanelRT.anchorMin = domPanelRT.anchorMax = new Vector2(0.5f, 1f);
            domPanelRT.pivot = new Vector2(0.5f, 1f);
            domPanelRT.anchoredPosition = new Vector2(0, -10);
            var domT = MakeTMP(domPanel.transform, "DominantText", Vector2.zero, 16, Color.white);
            domT.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            domT.GetComponent<RectTransform>().anchorMax = Vector2.one;
            domT.alignment = TextAlignmentOptions.Center;

            var upList = MakeTMP(ct, "UpgradesList", new Vector2(-10, -10), 11, new Color(0.67f, 0.8f, 0.53f));
            var upRT = upList.GetComponent<RectTransform>();
            upRT.anchorMin = upRT.anchorMax = new Vector2(1f, 1f);
            upRT.pivot = new Vector2(1f, 1f);
            upRT.anchoredPosition = new Vector2(-10, -10);
            upRT.sizeDelta = new Vector2(200, 200);
            upList.alignment = TextAlignmentOptions.Right;

            var upgPanel = MakePanel(ct, "UpgradePanel", new Vector2(1920, 1080), Vector2.zero, new Color(0, 0, 0, 0.78f));
            FullStretch(upgPanel.GetComponent<RectTransform>());
            var upgTitle = MakeTMP(upgPanel.transform, "UpgradeTitle", new Vector2(0, 120), 28, new Color(0f, 1f, 0.8f));
            upgTitle.text = "OLEADA COMPLETADA - ELIGE UNA MEJORA";
            upgTitle.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            upgTitle.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            upgTitle.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            upgTitle.alignment = TextAlignmentOptions.Center;

            var btnNames = new TextMeshProUGUI[3];
            var btnDescs = new TextMeshProUGUI[3];
            var btns = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                float bx = (i - 1) * 360f;
                var card = MakePanel(upgPanel.transform, string.Format("Card{0}", i), new Vector2(320, 160), Vector2.zero, new Color(0.04f, 0.12f, 0.08f, 0.9f));
                var cardRT = card.GetComponent<RectTransform>();
                cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
                cardRT.pivot = Vector2.one * 0.5f;
                cardRT.anchoredPosition = new Vector2(bx, -30);
                var btn = card.AddComponent<Button>();
                btn.targetGraphic = card.GetComponent<Image>();
                btns[i] = btn;
                var keyT = MakeTMP(card.transform, string.Format("Key{0}", i), new Vector2(0, 50), 22, new Color(0f, 1f, 0.8f));
                keyT.text = string.Format("[{0}]", i + 1);
                keyT.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                keyT.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                keyT.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                keyT.alignment = TextAlignmentOptions.Center;
                btnNames[i] = MakeTMP(card.transform, string.Format("UpgName{0}", i), new Vector2(0, 15), 17, new Color(0.9f, 1f, 0.9f));
                btnNames[i].GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                btnNames[i].GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                btnNames[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                btnNames[i].alignment = TextAlignmentOptions.Center;
                btnDescs[i] = MakeTMP(card.transform, string.Format("UpgDesc{0}", i), new Vector2(0, -20), 12, new Color(0.53f, 0.67f, 0.53f));
                btnDescs[i].GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                btnDescs[i].GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                btnDescs[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                btnDescs[i].GetComponent<RectTransform>().sizeDelta = new Vector2(290, 80);
                btnDescs[i].textWrappingMode = TMPro.TextWrappingModes.Normal;
                btnDescs[i].alignment = TextAlignmentOptions.Center;
            }

            var deathP = MakePanel(ct, "DeathScreen", new Vector2(1920, 1080), Vector2.zero, new Color(0, 0, 0, 0.82f));
            FullStretch(deathP.GetComponent<RectTransform>());
            var deathTitle = MakeTMP(deathP.transform, "DeadTitle", new Vector2(0, 140), 52, new Color(1f, 0.13f, 0.07f));
            deathTitle.text = "HAS CAIDO";
            CenterAnchor(deathTitle.GetComponent<RectTransform>()); deathTitle.alignment = TextAlignmentOptions.Center;
            var deathWaveT = MakeTMP(deathP.transform, "DeathWave", new Vector2(0, 80), 24, new Color(0.8f, 0.87f, 0.8f));
            CenterAnchor(deathWaveT.GetComponent<RectTransform>()); deathWaveT.alignment = TextAlignmentOptions.Center;
            var deathBestT = MakeTMP(deathP.transform, "DeathBest", new Vector2(0, 48), 24, new Color(0.8f, 0.87f, 0.8f));
            CenterAnchor(deathBestT.GetComponent<RectTransform>()); deathBestT.alignment = TextAlignmentOptions.Center;
            var deathGeneT = MakeTMP(deathP.transform, "DeathGene", new Vector2(0, 0), 20, Color.white);
            CenterAnchor(deathGeneT.GetComponent<RectTransform>()); deathGeneT.alignment = TextAlignmentOptions.Center;
            var retryT = MakeTMP(deathP.transform, "RetryHint", new Vector2(0, -80), 22, new Color(0.27f, 0.8f, 0.27f));
            retryT.text = "[ R ]  Reintentar";
            CenterAnchor(retryT.GetComponent<RectTransform>()); retryT.alignment = TextAlignmentOptions.Center;

            var pauseP = MakePanel(ct, "PauseScreen", new Vector2(1920, 1080), Vector2.zero, new Color(0, 0, 0, 0.6f));
            FullStretch(pauseP.GetComponent<RectTransform>());
            var pauseTitle = MakeTMP(pauseP.transform, "PauseTitle", new Vector2(0, 20), 44, new Color(0.8f, 1f, 0.8f));
            pauseTitle.text = "PAUSADO";
            CenterAnchor(pauseTitle.GetComponent<RectTransform>()); pauseTitle.alignment = TextAlignmentOptions.Center;
            var pauseHint = MakeTMP(pauseP.transform, "PauseHint", new Vector2(0, -30), 18, new Color(0.53f, 0.67f, 0.53f));
            pauseHint.text = "ESC para continuar";
            CenterAnchor(pauseHint.GetComponent<RectTransform>()); pauseHint.alignment = TextAlignmentOptions.Center;

            var menuP = MakePanel(ct, "MenuScreen", new Vector2(1920, 1080), Vector2.zero, new Color(0.02f, 0.04f, 0.03f, 0.96f));
            FullStretch(menuP.GetComponent<RectTransform>());
            var menuTitle = MakeTMP(menuP.transform, "MenuTitle", new Vector2(0, 200), 72, new Color(0.27f, 1f, 0.27f));
            menuTitle.text = "MUTATION SWARM";
            CenterAnchor(menuTitle.GetComponent<RectTransform>()); menuTitle.alignment = TextAlignmentOptions.Center;
            var menuSub = MakeTMP(menuP.transform, "MenuSub", new Vector2(0, 130), 22, new Color(0.7f, 0.85f, 0.7f));
            menuSub.text = "Sobrevive a oleadas de mutantes que evolucionan contra ti";
            CenterAnchor(menuSub.GetComponent<RectTransform>()); menuSub.alignment = TextAlignmentOptions.Center;

            var playBtnGO = MakePanel(menuP.transform, "PlayButton", new Vector2(320, 90), Vector2.zero, new Color(0.1f, 0.4f, 0.18f, 0.95f));
            var playRT = playBtnGO.GetComponent<RectTransform>();
            playRT.anchorMin = playRT.anchorMax = new Vector2(0.5f, 0.5f);
            playRT.pivot = Vector2.one * 0.5f;
            playRT.anchoredPosition = new Vector2(0, 0);
            var playButton = playBtnGO.AddComponent<Button>();
            playButton.targetGraphic = playBtnGO.GetComponent<Image>();
            var playLabel = MakeTMP(playBtnGO.transform, "PlayLabel", Vector2.zero, 34, Color.white);
            FullStretch(playLabel.GetComponent<RectTransform>());
            playLabel.text = "JUGAR";
            playLabel.alignment = TextAlignmentOptions.Center;

            var menuControls = MakeTMP(menuP.transform, "MenuControls", new Vector2(0, -150), 18, new Color(0.6f, 0.72f, 0.6f));
            menuControls.text = "A / D  Mover     W / Espacio  Saltar (doble)     Shift  Dash\nRaton  Apuntar y disparar     B  Construir     Esc  Pausa";
            CenterAnchor(menuControls.GetComponent<RectTransform>()); menuControls.alignment = TextAlignmentOptions.Center;
            menuControls.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 90);
            var menuHint = MakeTMP(menuP.transform, "MenuHint", new Vector2(0, -240), 20, new Color(0.27f, 0.9f, 0.4f));
            menuHint.text = "Pulsa ENTER o haz clic en JUGAR";
            CenterAnchor(menuHint.GetComponent<RectTransform>()); menuHint.alignment = TextAlignmentOptions.Center;

            var buildP = MakePanel(ct, "BuildHUD", new Vector2(0, 50), Vector2.zero, new Color(0, 0, 0, 0.75f));
            var bRT = buildP.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0f, 0f); bRT.anchorMax = new Vector2(1f, 0f);
            bRT.pivot = new Vector2(0.5f, 0f);
            bRT.anchoredPosition = Vector2.zero; bRT.sizeDelta = new Vector2(0, 50);
            var buildTypeT = MakeTMP(buildP.transform, "BuildType", Vector2.zero, 14, new Color(0.27f, 1f, 0.5f));
            FullStretch(buildTypeT.GetComponent<RectTransform>());
            buildTypeT.alignment = TextAlignmentOptions.Center;

            // HUDController wiring
            var hudGO = new GameObject("HUDController");
            hudGO.transform.SetParent(ct);
            var hud = hudGO.AddComponent<HUDController>();
            hud.splashScreen    = splashP;
            hud.splashHintText  = splashHint;
            hud.storyScreen     = storyP;
            hud.storyBodyText   = storyTMP;
            hud.storyPageIndicator = pageIndTMP;
            hud.waveText        = waveT;
            hud.dnaText         = dnaT;
            hud.bestText        = bestT;
            hud.hpSlider        = hpSlider;
            hud.hpFill          = hpFillImg;
            hud.dashSlider      = dashSlider;
            hud.dominantGeneText  = domT;
            hud.dominantGenePanel = domPanel.GetComponent<Image>();
            hud.enemiesText     = enemT;
            hud.upgradesListText = upList;
            hud.upgradePanel    = upgPanel;
            hud.upgradeButtons  = btns;
            hud.upgradeNames    = btnNames;
            hud.upgradeDescs    = btnDescs;
            hud.deathScreen     = deathP;
            hud.deathWaveText   = deathWaveT;
            hud.deathBestText   = deathBestT;
            hud.deathGeneText   = deathGeneT;
            hud.pauseScreen     = pauseP;
            hud.menuScreen      = menuP;
            hud.playButton      = playButton;
            hud.buildHUD        = buildP;
            hud.buildTypeText   = buildTypeT;

            // Splash & Story are opaque full-screen panels and MUST draw on top of every
            // gameplay/HUD element. They are created first, so without this they render behind
            // the always-on HUD (wave/dna/enemies/dominant) and look empty. Move them to front.
            splashP.transform.SetAsLastSibling();
            storyP.transform.SetAsLastSibling();

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        static void CenterAnchor(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        static GameObject MakePanel(Transform parent, string name, Vector2 size, Vector2 pos, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        static TextMeshProUGUI MakeTMP(Transform parent, string name, Vector2 pos, float size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = size;
            tmp.color = color;
            tmp.text = "";
            tmp.fontStyle = FontStyles.Bold;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 35);
            return tmp;
        }

        static Slider MakeSlider(Transform parent, string name, Vector2 pos, Vector2 size,
            Color bgColor, Color fillColor, out Image fillImage)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var bgGO = new GameObject("BG");
            bgGO.transform.SetParent(go.transform, false);
            var bgImg = bgGO.AddComponent<Image>(); bgImg.color = bgColor;
            FullStretch(bgGO.GetComponent<RectTransform>());
            var fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(go.transform, false);
            FullStretch(fillArea.AddComponent<RectTransform>());
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillArea.transform, false);
            fillImage = fillGO.AddComponent<Image>(); fillImage.color = fillColor;
            FullStretch(fillGO.GetComponent<RectTransform>());
            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillGO.GetComponent<RectTransform>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
            slider.interactable = false;
            return slider;
        }

        static void FullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Sprite CreateWhiteSprite()
        {
            var tex = new Texture2D(4, 4) { filterMode = FilterMode.Point };
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4);
        }

        static void EnsureLayer(string layerName)
        {
#if UNITY_EDITOR
            var tagManager = new UnityEditor.SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == layerName) return;
            }
            for (int i = 8; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
#endif
        }
    }

    static class GoExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
            => go.GetComponent<T>() != null ? go.GetComponent<T>() : go.AddComponent<T>();
    }
}