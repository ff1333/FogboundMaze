using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FogboundMaze
{
    public sealed class GameHud : MonoBehaviour
    {
        private static readonly Color Ink = new(0.92f, 0.95f, 0.91f);
        private static readonly Color Panel = new(0.035f, 0.055f, 0.06f, 0.92f);
        private static readonly Color Accent = new(0.2f, 0.78f, 0.62f);
        private static readonly Color Warning = new(0.95f, 0.55f, 0.18f);

        private GameInput input;
        private Font font;
        private Text healthText;
        private Text levelText;
        private Text weaponText;
        private Text timerText;
        private Text killsText;
        private Text statusText;
        private Text resultTitle;
        private Text resultStats;
        private Image crosshair;
        private GameObject selectionPanel;
        private GameObject resultPanel;
        private GameObject pausePanel;
        private float crosshairFlash;

        public static GameHud Create(GameInput input)
        {
            var root = new GameObject("Game HUD");
            var hud = root.AddComponent<GameHud>();
            hud.input = input;
            hud.Build();
            return hud;
        }

        private void Update()
        {
            if (crosshairFlash > 0f)
            {
                crosshairFlash -= Time.unscaledDeltaTime;
                crosshair.color = crosshairFlash > 0f ? Warning : Ink;
            }
        }

        public void Refresh(GameDirector director, PlayerController player)
        {
            healthText.text = $"HP  {Mathf.CeilToInt(player.Health.Current)} / {Mathf.CeilToInt(player.Health.Maximum)}";
            levelText.text = $"LEVEL  {director.CurrentLevel.levelNumber:00}";
            timerText.text = TimeSpan.FromSeconds(director.Elapsed).ToString(@"mm\:ss");
            killsText.text = $"KILLS  {director.Kills:000}";
            weaponText.text = player.Weapon.Type == WeaponType.Pistol
                ? $"PISTOL  {(player.Weapon.IsReloading ? "RELOAD" : player.Weapon.Ammunition.ToString("00"))}"
                : "MACHETE";
            statusText.text = director.CurrentLevel.miasma
                ? (director.CurrentLevel.miasma && director.CurrentLevel.safeLightRadius > 0f
                    ? "MIASMA ACTIVE"
                    : string.Empty)
                : string.Empty;
            statusText.color = director.CurrentLevel.miasma ? Warning : Accent;
        }

        public void ShowWeaponSelection(int level)
        {
            selectionPanel.SetActive(true);
            resultPanel.SetActive(false);
            pausePanel.SetActive(false);
            levelText.text = $"LEVEL  {level:00}";
        }

        public void HideWeaponSelection() => selectionPanel.SetActive(false);

        public void SetPhase(GamePhase phase, int level)
        {
            levelText.text = $"LEVEL  {level:00}";
        }

        public void ShowResult(bool won, int level, float elapsed, int kills)
        {
            resultPanel.SetActive(true);
            resultTitle.text = won ? (level == 10 ? "CAMPAIGN CLEARED" : "EXIT REACHED") : "RUN LOST";
            resultTitle.color = won ? Accent : Warning;
            resultStats.text = $"LEVEL {level:00}    {TimeSpan.FromSeconds(elapsed):mm\\:ss}    KILLS {kills:000}";
            var next = resultPanel.transform.Find("Next")?.gameObject;
            if (next != null) next.SetActive(won && level < 10);
        }

        public void SetPause(bool paused) => pausePanel.SetActive(paused);

        public void FlashCrosshair()
        {
            crosshairFlash = 0.08f;
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            BuildTopBar();
            BuildCrosshair();
            BuildSelection();
            BuildResult();
            BuildPause();
            if (Application.isMobilePlatform)
            {
                MobileHudBuilder.Build(transform, input, font);
            }
        }

        private void BuildTopBar()
        {
            var bar = PanelObject("Top Bar", transform, Panel);
            Stretch(bar.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -82f), Vector2.zero);
            levelText = Label("Level", bar.transform, "LEVEL  01", 28, TextAnchor.MiddleLeft);
            SetRect(levelText.rectTransform, new Vector2(0f, 0f), new Vector2(0.16f, 1f), new Vector2(28f, 0f), new Vector2(-8f, 0f));
            healthText = Label("Health", bar.transform, "HP  100 / 100", 28, TextAnchor.MiddleLeft);
            SetRect(healthText.rectTransform, new Vector2(0.16f, 0f), new Vector2(0.38f, 1f));
            weaponText = Label("Weapon", bar.transform, "UNARMED", 28, TextAnchor.MiddleCenter);
            SetRect(weaponText.rectTransform, new Vector2(0.38f, 0f), new Vector2(0.62f, 1f));
            statusText = Label("Status", bar.transform, string.Empty, 24, TextAnchor.MiddleCenter);
            SetRect(statusText.rectTransform, new Vector2(0.62f, 0f), new Vector2(0.79f, 1f));
            killsText = Label("Kills", bar.transform, "KILLS  000", 25, TextAnchor.MiddleCenter);
            SetRect(killsText.rectTransform, new Vector2(0.79f, 0f), new Vector2(0.91f, 1f));
            timerText = Label("Timer", bar.transform, "00:00", 30, TextAnchor.MiddleRight);
            SetRect(timerText.rectTransform, new Vector2(0.91f, 0f), Vector2.one, Vector2.zero, new Vector2(-28f, 0f));
        }

        private void BuildCrosshair()
        {
            var root = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(transform, false);
            crosshair = root.GetComponent<Image>();
            crosshair.color = Ink;
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(5f, 5f);
        }

        private void BuildSelection()
        {
            selectionPanel = PanelObject("Weapon Selection", transform, Panel);
            var rect = selectionPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(760f, 410f);
            var title = Label("Title", selectionPanel.transform, "CHOOSE YOUR LOADOUT", 42, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 0.72f), Vector2.one);
            var subtitle = Label("Subtitle", selectionPanel.transform, "ONE WEAPON. ONE EXIT.", 20, TextAnchor.MiddleCenter, new Color(0.62f, 0.7f, 0.69f));
            SetRect(subtitle.rectTransform, new Vector2(0f, 0.61f), new Vector2(1f, 0.76f));
            var pistol = Button("Pistol", selectionPanel.transform, "PISTOL\n10 rounds  |  28m", new Color(0.12f, 0.42f, 0.55f));
            SetRect(pistol.GetComponent<RectTransform>(), new Vector2(0.08f, 0.12f), new Vector2(0.48f, 0.55f));
            pistol.onClick.AddListener(() => GameDirector.Instance.SelectWeapon(WeaponType.Pistol));
            var machete = Button("Machete", selectionPanel.transform, "MACHETE\nhigh damage  |  close", new Color(0.55f, 0.25f, 0.12f));
            SetRect(machete.GetComponent<RectTransform>(), new Vector2(0.52f, 0.12f), new Vector2(0.92f, 0.55f));
            machete.onClick.AddListener(() => GameDirector.Instance.SelectWeapon(WeaponType.Machete));
        }

        private void BuildResult()
        {
            resultPanel = PanelObject("Result", transform, Panel);
            var rect = resultPanel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(650f, 340f);
            resultTitle = Label("Result Title", resultPanel.transform, "EXIT REACHED", 48, TextAnchor.MiddleCenter);
            SetRect(resultTitle.rectTransform, new Vector2(0f, 0.62f), Vector2.one);
            resultStats = Label("Result Stats", resultPanel.transform, string.Empty, 24, TextAnchor.MiddleCenter);
            SetRect(resultStats.rectTransform, new Vector2(0.05f, 0.43f), new Vector2(0.95f, 0.64f));
            var retry = Button("Retry", resultPanel.transform, "RETRY", new Color(0.24f, 0.29f, 0.3f));
            SetRect(retry.GetComponent<RectTransform>(), new Vector2(0.08f, 0.12f), new Vector2(0.48f, 0.36f));
            retry.onClick.AddListener(() => GameDirector.Instance.Retry());
            var next = Button("Next", resultPanel.transform, "NEXT", Accent);
            SetRect(next.GetComponent<RectTransform>(), new Vector2(0.52f, 0.12f), new Vector2(0.92f, 0.36f));
            next.onClick.AddListener(() => GameDirector.Instance.NextLevel());
            resultPanel.SetActive(false);
        }

        private void BuildPause()
        {
            pausePanel = PanelObject("Pause", transform, Panel);
            var rect = pausePanel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(480f, 260f);
            var title = Label("Title", pausePanel.transform, "PAUSED", 46, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 0.54f), Vector2.one);
            var resume = Button("Resume", pausePanel.transform, "RESUME", Accent);
            SetRect(resume.GetComponent<RectTransform>(), new Vector2(0.16f, 0.14f), new Vector2(0.84f, 0.43f));
            resume.onClick.AddListener(() => GameDirector.Instance.SetPaused(false));
            pausePanel.SetActive(false);
        }

        private Text Label(string name, Transform parent, string value, int size, TextAnchor alignment, Color? color = null)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Text));
            root.transform.SetParent(parent, false);
            var text = root.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color ?? Ink;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = size;
            return text;
        }

        private Button Button(string name, Transform parent, string label, Color color)
        {
            var root = PanelObject(name, parent, color);
            var button = root.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
            button.colors = colors;
            var text = Label("Label", root.transform, label, 27, TextAnchor.MiddleCenter);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 8f), new Vector2(-16f, -8f));
            return button;
        }

        private static GameObject PanelObject(string name, Transform parent, Color color)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = color;
            return root;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var root = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            root.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 min, Vector2 max)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;
        }
    }
}
