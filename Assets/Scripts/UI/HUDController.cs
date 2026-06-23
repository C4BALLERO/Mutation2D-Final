using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MutationSwarm
{
    public class HUDController : MonoBehaviour
    {
        [Header("HUD")]
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI dnaText;
        public TextMeshProUGUI bestText;
        public Slider          hpSlider;
        public Image           hpFill;
        public Slider          dashSlider;
        public TextMeshProUGUI dominantGeneText;
        public Image           dominantGenePanel;
        public TextMeshProUGUI enemiesText;
        public TextMeshProUGUI upgradesListText;

        [Header("Upgrade Panel")]
        public GameObject upgradePanel;
        public Button[]   upgradeButtons;
        public TextMeshProUGUI[] upgradeNames;
        public TextMeshProUGUI[] upgradeDescs;

        [Header("Death Screen")]
        public GameObject deathScreen;
        public TextMeshProUGUI deathWaveText;
        public TextMeshProUGUI deathBestText;
        public TextMeshProUGUI deathGeneText;

        [Header("Pause Screen")]
        public GameObject pauseScreen;

        [Header("Main Menu")]
        public GameObject menuScreen;
        public Button     playButton;

        [Header("Build HUD")]
        public GameObject buildHUD;
        public TextMeshProUGUI buildTypeText;

        [Header("Splash Screen")]
        public GameObject splashScreen;
        public TextMeshProUGUI splashHintText;

        [Header("Story Screen")]
        public GameObject storyScreen;
        public TextMeshProUGUI storyBodyText;
        public TextMeshProUGUI storyPageIndicator;

        [Header("New Mechanics")]
        public Slider          furySlider;
        public Image           furyFill;
        public TextMeshProUGUI furyText;
        public TextMeshProUGUI comboText;
        public GameObject      bossBarRoot;
        public Slider          bossBar;
        public TextMeshProUGUI mutationMsgText;
        public TextMeshProUGUI mutationsListText;

        static readonly string[] StoryPages =
        {
            "AÑO 2087.\n\nUn virus desconocido llamado <color=#22ff44>MUTATION-X</color> arrasa\nla Zona Industrial.\n\nLos infectados evolucionan con cada oleada.",
            "Los científicos lo llaman:\n\n<color=#22ff44>LA MUTACIÓN ENJAMBRE</color>\n\nCada generación aprende de la anterior.\nCada oleada es más letal que la última.",
            "Eres el <color=#22ff44>AGENTE X-7</color>.\nÚltimo soldado activo de la\nUnidad de Hazmat Biológica.\n\nMisión: resistir hasta que llegue la evacuación.",
            "Tu único aliado es <color=#ffcc44>POLLO</color>.\n\nUn drone experimental con inteligencia\nde... gallina.\n\nNo es perfecto. Pero dispara.",
            "<color=#22ff44>SOBREVIVE.</color>\n\nLa humanidad cuenta contigo.\n\n<size=22><color=#336633>[ ESPACIO / ENTER ] para comenzar</color></size>",
        };

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // Splash screen
            if (splashScreen) splashScreen.SetActive(gm.Phase == GamePhase.Splash);

            if (gm.Phase == GamePhase.Splash)
            {
                if (splashHintText)
                {
                    var c = splashHintText.color;
                    c.a = 0.4f + 0.35f * Mathf.Sin(Time.time * 2.2f);
                    splashHintText.color = c;
                }
                if (Input.anyKeyDown) gm.SkipSplash();
                return;
            }

            // Story screen
            if (storyScreen) storyScreen.SetActive(gm.Phase == GamePhase.Story);

            if (gm.Phase == GamePhase.Story)
            {
                if (storyBodyText) storyBodyText.text = StoryPages[gm.StoryPage];
                if (storyPageIndicator)
                    storyPageIndicator.text = $"{gm.StoryPage + 1} / {GameManager.StoryPagesCount}";
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
                    gm.AdvanceStory();
                return;
            }

            var ps = PlayerStats.Instance;
            var wm = WaveManager.Instance;
            if (ps == null || wm == null) return;

            // Phase-dependent panels
            if (menuScreen) menuScreen.SetActive(gm.Phase == GamePhase.Menu);
            if (upgradePanel) upgradePanel.SetActive(gm.Phase == GamePhase.Upgrade);
            if (deathScreen) deathScreen.SetActive(gm.Phase == GamePhase.Dead);
            if (pauseScreen) pauseScreen.SetActive(gm.Phase == GamePhase.Paused);
            if (buildHUD) buildHUD.SetActive(gm.Phase == GamePhase.Building);

            // Main menu: start the run with Enter / Space / click or the Play button.
            if (gm.Phase == GamePhase.Menu)
            {
                if (playButton != null)
                {
                    playButton.onClick.RemoveAllListeners();
                    playButton.onClick.AddListener(() => gm.StartGame());
                }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
                    || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                    gm.StartGame();
                return;
            }

            // HUD values
            if (waveText) waveText.text = $"OLEADA  {gm.WaveNum}";
            if (dnaText)  dnaText.text  = $"ADN  {Mathf.FloorToInt(ps.Dna)}";
            if (bestText) bestText.text  = $"MEJOR: {gm.BestScore}";

            if (hpSlider) { hpSlider.value = ps.Hp / ps.MaxHp; }
            if (hpFill)
            {
                float r = ps.Hp / ps.MaxHp;
                hpFill.color = r > 0.5f ? new Color(0.27f, 1f, 0.13f)
                             : r > 0.25f ? new Color(1f, 0.8f, 0f)
                             : new Color(1f, 0.13f, 0.07f);
            }

            if (dashSlider && PlayerController.Instance)
                dashSlider.value = PlayerController.Instance.DashCdRatio;

            // ── Fury / Overdrive ──
            if (furySlider) furySlider.value = ps.Fury / 100f;
            if (furyFill)
                furyFill.color = ps.Overdrive ? new Color(1f, 0.5f, 0f)
                               : ps.Fury >= 100f ? new Color(1f, 0.85f, 0.1f)
                               : new Color(0.8f, 0.3f, 0.9f);
            if (furyText)
                furyText.text = ps.Overdrive ? "¡FURIA ACTIVA!"
                              : ps.Fury >= 100f ? "[ F ] FURIA LISTA"
                              : "";

            // ── Combo / multiplier ──
            if (comboText)
                comboText.text = ps.Combo >= 5 ? $"COMBO  x{ps.Multiplier}   ({ps.Combo})" : "";

            // ── Boss health bar ──
            var boss = wm.Boss;
            if (bossBarRoot) bossBarRoot.SetActive(boss != null);
            if (boss != null && bossBar) bossBar.value = boss.HpRatio;

            // ── Mutations ──
            if (mutationMsgText)
                mutationMsgText.text = ps.LastMutationMsgTimer > 0f
                    ? $"<color=#99ff33>¡MUTACIÓN OBTENIDA!</color>\n{ps.LastMutationMsg}"
                    : "";
            if (mutationsListText)
            {
                var msb = new System.Text.StringBuilder();
                foreach (var m in ps.Mutations) msb.AppendLine("» " + PlayerStats.MutationName(m));
                mutationsListText.text = msb.ToString();
            }

            // Dominant gene
            var evo = EvolutionSystem.Instance;
            if (evo != null && dominantGeneText)
            {
                var gene = evo.DominantGene;
                dominantGeneText.text = gene == GeneType.None ? "" :
                    $"MUTACIÓN DOMINANTE: {GeneData.GetName(gene)}";
                dominantGeneText.color = GeneData.GetColor(gene);
                if (dominantGenePanel) dominantGenePanel.gameObject.SetActive(gene != GeneType.None);
            }

            if (enemiesText) enemiesText.text = $"Enemigos: {wm.ActiveEnemies.Count}";

            // Upgrades list
            if (upgradesListText)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var up in new HashSet<string>(ps.Upgrades))
                    sb.AppendLine("+ " + GetUpgradeName(up));
                upgradesListText.text = sb.ToString();
            }

            // Build HUD
            if (buildTypeText && gm.Phase == GamePhase.Building)
            {
                var db = FindFirstObjectByType<DefenseBuilder>();
                string t = db?.SelectedType switch
                {
                    2 => "[1] Barricada   [2] TORRETA   [3] Mina",
                    3 => "[1] Barricada   [2] Torreta   [3] MINA",
                    _ => "[1] BARRICADA   [2] Torreta   [3] Mina",
                };
                buildTypeText.text = $"MODO CONSTRUCCIÓN (30 ADN) · {t} · B=Salir";
                buildTypeText.color = ps.Dna >= 30 ? new Color(0.27f,1f,0.5f) : new Color(1f,0.27f,0.27f);
            }

            // Upgrade cards
            if (gm.Phase == GamePhase.Upgrade)
            {
                var opts = gm.UpgradeOptions;
                for (int i = 0; i < upgradeButtons.Length; i++)
                {
                    bool valid = i < opts.Count;
                    upgradeButtons[i].gameObject.SetActive(valid);
                    if (!valid) continue;
                    if (upgradeNames[i]) upgradeNames[i].text = opts[i].name;
                    if (upgradeDescs[i]) upgradeDescs[i].text = opts[i].desc;
                    int idx = i;
                    upgradeButtons[i].onClick.RemoveAllListeners();
                    upgradeButtons[i].onClick.AddListener(() => gm.ApplyUpgrade(idx));
                }
                if (Input.GetKeyDown(KeyCode.Alpha1)) gm.ApplyUpgrade(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) gm.ApplyUpgrade(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) gm.ApplyUpgrade(2);
            }

            // Death screen
            if (gm.Phase == GamePhase.Dead)
            {
                if (deathWaveText) deathWaveText.text = $"OLEADA ALCANZADA: {gm.WaveNum}";
                if (deathBestText) deathBestText.text  = $"MEJOR: {gm.BestScore}";
                var evoInst = EvolutionSystem.Instance;
                if (deathGeneText && evoInst != null)
                {
                    var g = evoInst.DominantGene;
                    deathGeneText.text  = $"Mutación dominante: {GeneData.GetName(g)}";
                    deathGeneText.color = GeneData.GetColor(g);
                }
                if (Input.GetKeyDown(KeyCode.R)) gm.Restart();
            }
        }

        static string GetUpgradeName(string id) => id switch
        {
            "piercing"      => "Balas Perforantes",
            "electric"      => "Munición Eléctrica",
            "dashExplosive" => "Dash Explosivo",
            "drone"         => "Dron Acompañante",
            "regen"         => "Regeneración",
            "fastBuild"     => "Constructor Rápido",
            "moreDamage"    => "Más Daño",
            "fasterReload"  => "Recarga Rápida",
            "moreHp"        => "Más Vida Máxima",
            _               => id,
        };
    }
}
