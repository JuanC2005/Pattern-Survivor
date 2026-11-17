using System;
using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using PatternSurvivor.Patterns.Facade;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PatternSurvivor.UI
{
    public sealed class GameUIController : MonoBehaviour
    {
        private GameFacade facade;
        private GameplayRuntime runtime;
        private PatternDebugPanel patternPanel;
        private Canvas canvas;
        private GameObject menuPanel;
        private GameObject gameplayPanel;
        private GameObject pausePanel;
        private GameObject upgradePanel;
        private GameObject gameOverPanel;
        private GameObject patternsPanel;
        private Text healthText;
        private Text scoreText;
        private Text timeText;
        private Text waveText;
        private Text levelText;
        private Text weaponText;
        private Text skillText;
        private Text gameOverText;
        private Slider healthSlider;
        private Button[] upgradeButtons;
        private bool built;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private readonly Color backgroundColor = new Color(0.025f, 0.04f, 0.09f, 0.97f);
        private readonly Color panelColor = new Color(0.06f, 0.09f, 0.17f, 0.96f);
        private readonly Color accentColor = new Color(0.2f, 0.9f, 0.8f);

        private void OnEnable()
        {
            GameEvents.HealthChanged += HandleHealthChanged;
            GameEvents.ScoreChanged += HandleScoreChanged;
            GameEvents.KillsChanged += HandleKillsChanged;
            GameEvents.LevelChanged += HandleLevelChanged;
            GameEvents.TimeChanged += HandleTimeChanged;
            GameEvents.WaveChanged += HandleWaveChanged;
        }

        private void OnDisable()
        {
            GameEvents.HealthChanged -= HandleHealthChanged;
            GameEvents.ScoreChanged -= HandleScoreChanged;
            GameEvents.KillsChanged -= HandleKillsChanged;
            GameEvents.LevelChanged -= HandleLevelChanged;
            GameEvents.TimeChanged -= HandleTimeChanged;
            GameEvents.WaveChanged -= HandleWaveChanged;
        }

        public void Build(GameFacade gameFacade, GameplayRuntime gameplayRuntime, PatternDebugPanel debugPanel)
        {
            if (built)
            {
                return;
            }

            facade = gameFacade;
            runtime = gameplayRuntime;
            patternPanel = debugPanel;
            canvas = CreateCanvas();
            CreateMenuPanel();
            CreateGameplayPanel();
            CreatePausePanel();
            CreateUpgradePanel();
            CreateGameOverPanel();
            patternsPanel = patternPanel.Build(canvas.transform, accentColor, panelColor, backgroundColor);
            built = true;
            RefreshInitialValues();
            ApplyResponsiveLayout();
        }

        public void ShowMenu()
        {
            if (!built) return;
            menuPanel.SetActive(true);
            gameplayPanel.SetActive(false);
            pausePanel.SetActive(false);
            upgradePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            patternsPanel.SetActive(false);
        }

        public void ShowGameplay()
        {
            if (!built) return;
            menuPanel.SetActive(false);
            gameplayPanel.SetActive(true);
            pausePanel.SetActive(false);
            upgradePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            patternsPanel.SetActive(false);
        }

        public void ShowPause()
        {
            if (!built) return;
            gameplayPanel.SetActive(true);
            pausePanel.SetActive(true);
            menuPanel.SetActive(false);
            upgradePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            patternsPanel.SetActive(false);
        }

        public void ShowGameOver()
        {
            if (!built) return;
            gameplayPanel.SetActive(false);
            pausePanel.SetActive(false);
            menuPanel.SetActive(false);
            upgradePanel.SetActive(false);
            gameOverPanel.SetActive(true);
            patternsPanel.SetActive(false);
            gameOverText.text = $"PUNTUACIÓN FINAL  {GameManagerAccessor.Score}\nENEMIGOS ELIMINADOS  {GameManagerAccessor.Kills}\nTIEMPO  {FormatTime(GameManagerAccessor.ElapsedTime)}";
        }

        public void ShowUpgradeSelection(UpgradeOption[] options)
        {
            if (!built) return;
            gameplayPanel.SetActive(true);
            upgradePanel.SetActive(true);
            pausePanel.SetActive(false);
            menuPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            patternsPanel.SetActive(false);

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int index = i;
                upgradeButtons[i].gameObject.SetActive(index < options.Length);
                if (index >= options.Length) continue;
                Text label = upgradeButtons[i].GetComponentInChildren<Text>();
                label.text = $"{options[index].Title}\n<size=16>{options[index].Description}</size>";
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => facade.SelectUpgrade(options[index]));
            }
        }

        public void ShowPatterns()
        {
            if (!built) return;
            menuPanel.SetActive(false);
            gameplayPanel.SetActive(false);
            pausePanel.SetActive(false);
            upgradePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            patternsPanel.SetActive(true);
            patternPanel.Refresh();
        }

        public void HidePatterns() => patternsPanel.SetActive(false);

        private void CreateMenuPanel()
        {
            menuPanel = CreatePanel("MenuPanel", canvas.transform, backgroundColor, Vector2.zero, Vector2.one, Vector2.zero);
            CreateText(menuPanel.transform, "PATTERN SURVIVOR", 52, accentColor, new Vector2(0.5f, 0.75f), new Vector2(760f, 80f), TextAnchor.MiddleCenter);
            CreateText(menuPanel.transform, "SUPERVIVENCIA ARCADE  ·  11 PATRONES EN ACCIÓN", 18, Color.white, new Vector2(0.5f, 0.66f), new Vector2(760f, 40f), TextAnchor.MiddleCenter);
            Button play = CreateButton(menuPanel.transform, "JUGAR", new Vector2(0.5f, 0.51f), accentColor);
            play.onClick.AddListener(facade.StartNewGame);
            Button patterns = CreateButton(menuPanel.transform, "VER PATRONES", new Vector2(0.5f, 0.40f), new Color(0.35f, 0.55f, 1f));
            patterns.onClick.AddListener(facade.OpenPatterns);
            Button quit = CreateButton(menuPanel.transform, "SALIR", new Vector2(0.5f, 0.29f), new Color(0.85f, 0.3f, 0.35f));
            quit.onClick.AddListener(Application.Quit);
            CreateText(menuPanel.transform, "WASD / FLECHAS  Mover\nCLIC IZQUIERDO / ESPACIO  Disparar\nQ  Habilidad combinada\nESC  Pausa", 18, new Color(0.75f, 0.8f, 0.9f), new Vector2(0.5f, 0.12f), new Vector2(700f, 100f), TextAnchor.MiddleCenter);
        }

        private void CreateGameplayPanel()
        {
            gameplayPanel = CreatePanel("GameplayPanel", canvas.transform, Color.clear, Vector2.zero, Vector2.one, Vector2.zero);
            healthText = CreateText(gameplayPanel.transform, "HP: 100/100", 22, Color.white, new Vector2(0.08f, 0.94f), new Vector2(260f, 34f), TextAnchor.MiddleLeft);
            healthSlider = CreateSlider(gameplayPanel.transform, new Vector2(0.08f, 0.885f), new Vector2(260f, 18f), accentColor);
            waveText = CreateText(gameplayPanel.transform, "OLEADA 1 — 00:00", 24, Color.white, new Vector2(0.5f, 0.94f), new Vector2(360f, 34f), TextAnchor.MiddleCenter);
            scoreText = CreateText(gameplayPanel.transform, "PUNTOS: 0  |  KILLS: 0", 22, Color.white, new Vector2(0.90f, 0.94f), new Vector2(360f, 34f), TextAnchor.MiddleRight);
            levelText = CreateText(gameplayPanel.transform, "NIVEL 1", 18, accentColor, new Vector2(0.90f, 0.885f), new Vector2(360f, 28f), TextAnchor.MiddleRight);
            weaponText = CreateText(gameplayPanel.transform, "ARMA BASIC", 17, Color.white, new Vector2(0.08f, 0.09f), new Vector2(380f, 30f), TextAnchor.MiddleLeft);
            skillText = CreateText(gameplayPanel.transform, "Q HABILIDAD LISTA", 17, Color.white, new Vector2(0.92f, 0.09f), new Vector2(400f, 30f), TextAnchor.MiddleRight);
            CreateText(gameplayPanel.transform, "WASD Mover  |  Click Disparar  |  Q Habilidad  |  Esc Pausa", 16, new Color(0.7f, 0.75f, 0.85f), new Vector2(0.5f, 0.045f), new Vector2(680f, 28f), TextAnchor.MiddleCenter);
            Button pause = CreateButton(gameplayPanel.transform, "Ⅱ", new Vector2(0.97f, 0.89f), new Color(0.3f, 0.45f, 0.7f), new Vector2(70f, 42f));
            pause.onClick.AddListener(facade.PauseGame);
            Button patterns = CreateButton(gameplayPanel.transform, "PATRONES", new Vector2(0.08f, 0.14f), new Color(0.35f, 0.55f, 1f), new Vector2(130f, 36f));
            patterns.onClick.AddListener(facade.OpenPatterns);
            gameplayPanel.SetActive(false);
        }

        private void CreatePausePanel()
        {
            pausePanel = CreatePanel("PausePanel", canvas.transform, new Color(0.01f, 0.02f, 0.05f, 0.86f), Vector2.zero, Vector2.one, Vector2.zero);
            CreateText(pausePanel.transform, "PAUSA", 44, accentColor, new Vector2(0.5f, 0.67f), new Vector2(500f, 64f), TextAnchor.MiddleCenter);
            Button resume = CreateButton(pausePanel.transform, "REANUDAR", new Vector2(0.5f, 0.52f), accentColor);
            resume.onClick.AddListener(facade.ResumeGame);
            Button restart = CreateButton(pausePanel.transform, "REINICIAR", new Vector2(0.5f, 0.41f), new Color(0.35f, 0.55f, 1f));
            restart.onClick.AddListener(facade.RestartGame);
            Button patterns = CreateButton(pausePanel.transform, "VER PATRONES", new Vector2(0.5f, 0.30f), new Color(0.45f, 0.4f, 0.8f));
            patterns.onClick.AddListener(facade.OpenPatterns);
            Button menu = CreateButton(pausePanel.transform, "MENÚ", new Vector2(0.5f, 0.19f), new Color(0.85f, 0.3f, 0.35f));
            menu.onClick.AddListener(facade.ReturnToMenu);
            pausePanel.SetActive(false);
        }

        private void CreateUpgradePanel()
        {
            upgradePanel = CreatePanel("UpgradePanel", canvas.transform, new Color(0.01f, 0.02f, 0.05f, 0.92f), Vector2.zero, Vector2.one, Vector2.zero);
            CreateText(upgradePanel.transform, "SUBIDA DE NIVEL", 40, accentColor, new Vector2(0.5f, 0.72f), new Vector2(700f, 60f), TextAnchor.MiddleCenter);
            CreateText(upgradePanel.transform, "ELIGE UNA MEJORA", 18, Color.white, new Vector2(0.5f, 0.64f), new Vector2(500f, 32f), TextAnchor.MiddleCenter);
            upgradeButtons = new Button[3];
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                upgradeButtons[i] = CreateButton(upgradePanel.transform, "MEJORA", new Vector2(0.28f + i * 0.22f, 0.45f), new Color(0.2f, 0.5f, 0.85f), new Vector2(260f, 112f));
            }
            upgradePanel.SetActive(false);
        }

        private void CreateGameOverPanel()
        {
            gameOverPanel = CreatePanel("GameOverPanel", canvas.transform, backgroundColor, Vector2.zero, Vector2.one, Vector2.zero);
            CreateText(gameOverPanel.transform, "GAME OVER", 52, new Color(1f, 0.35f, 0.4f), new Vector2(0.5f, 0.68f), new Vector2(700f, 72f), TextAnchor.MiddleCenter);
            gameOverText = CreateText(gameOverPanel.transform, string.Empty, 22, Color.white, new Vector2(0.5f, 0.53f), new Vector2(600f, 120f), TextAnchor.MiddleCenter);
            Button retry = CreateButton(gameOverPanel.transform, "REINTENTAR", new Vector2(0.5f, 0.34f), accentColor);
            retry.onClick.AddListener(facade.RestartGame);
            Button menu = CreateButton(gameOverPanel.transform, "MENÚ", new Vector2(0.5f, 0.23f), new Color(0.35f, 0.55f, 1f));
            menu.onClick.AddListener(facade.ReturnToMenu);
            gameOverPanel.SetActive(false);
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("UI Canvas");
            canvasObject.transform.SetParent(transform);
            Canvas result = canvasObject.AddComponent<Canvas>();
            result.renderMode = RenderMode.ScreenSpaceOverlay;
            result.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            return result;
        }

        private GameObject CreatePanel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = Vector2.zero;
            return panel;
        }

        private Text CreateText(Transform parent, string content, int size, Color color, Vector2 anchor, Vector2 dimensions, TextAnchor alignment)
        {
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = dimensions;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Vector2 anchor, Color color, Vector2? dimensions = null)
        {
            GameObject buttonObject = new GameObject($"Button_{label}");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(color.r, color.g, color.b, 0.92f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = Color.Lerp(image.color, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(image.color, Color.black, 0.2f);
            button.colors = colors;
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = dimensions ?? new Vector2(300f, 58f);
            Text text = CreateText(buttonObject.transform, label, 20, Color.white, new Vector2(0.5f, 0.5f), rect.sizeDelta - new Vector2(18f, 10f), TextAnchor.MiddleCenter);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = 22;
            return button;
        }

        private Slider CreateSlider(Transform parent, Vector2 anchor, Vector2 dimensions, Color fillColor)
        {
            GameObject sliderObject = new GameObject("HealthBar");
            sliderObject.transform.SetParent(parent, false);
            Image background = sliderObject.AddComponent<Image>();
            background.color = new Color(0.1f, 0.13f, 0.2f, 0.95f);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            RectTransform rect = sliderObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = dimensions;
            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(sliderObject.transform, false);
            Image fill = fillObject.AddComponent<Image>();
            fill.color = fillColor;
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            slider.fillRect = fillRect;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private void RefreshInitialValues()
        {
            HandleHealthChanged(100, 100);
            HandleScoreChanged(0);
            HandleKillsChanged(0);
            HandleLevelChanged(1);
            HandleTimeChanged(0f);
            HandleWaveChanged(1);
        }

        private void HandleHealthChanged(int current, int maximum)
        {
            if (healthText == null) return;
            healthText.text = $"HP: {current}/{maximum}";
            healthSlider.value = maximum <= 0 ? 0f : current / (float)maximum;
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = $"PUNTOS: {score}  |  KILLS: {GameManagerAccessor.Kills}";
        }

        private void HandleKillsChanged(int kills)
        {
            if (scoreText != null) scoreText.text = $"PUNTOS: {GameManagerAccessor.Score}  |  KILLS: {kills}";
        }

        private void HandleLevelChanged(int level)
        {
            if (levelText != null) levelText.text = $"NIVEL {level}";
        }

        private void HandleTimeChanged(float time)
        {
            if (waveText != null) waveText.text = $"OLEADA {GameManagerAccessor.Wave} — {FormatTime(time)}";
        }

        private void HandleWaveChanged(int wave)
        {
            if (waveText != null) waveText.text = $"OLEADA {wave} — {FormatTime(GameManagerAccessor.ElapsedTime)}";
        }

        private void Update()
        {
            if (!built || runtime.Player == null) return;
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                ApplyResponsiveLayout();
            }

            WeaponController weapon = runtime.Player.Weapon;
            weaponText.text = $"ARMA  {weapon.CurrentConfig.Damage} DMG  ·  {weapon.CurrentConfig.FireRate:0.0}/s  ·  {weapon.ActiveEffectsDescription}";
            float cooldown = runtime.Player.Skill.RemainingCooldown;
            skillText.text = cooldown <= 0f ? "Q  HABILIDAD LISTA" : $"Q  COOLDOWN {cooldown:0.0}s";
        }

        private void ApplyResponsiveLayout()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            if (canvas == null || lastScreenHeight <= 0)
            {
                return;
            }

            bool compact = lastScreenWidth < 1100 || lastScreenWidth / (float)lastScreenHeight < 1.55f;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.matchWidthOrHeight = compact ? 0f : 0.5f;

            SetSize(healthText, compact ? 220f : 260f, 34f);
            SetSize(healthSlider, compact ? 220f : 260f, 18f);
            SetSize(waveText, compact ? 300f : 360f, 34f);
            SetSize(scoreText, compact ? 300f : 360f, 34f);
            SetSize(levelText, compact ? 300f : 360f, 28f);
            SetSize(weaponText, compact ? 300f : 380f, 30f);
            SetSize(skillText, compact ? 300f : 400f, 30f);

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                RectTransform rect = upgradeButtons[i].GetComponent<RectTransform>();
                if (compact)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.53f - i * 0.16f);
                    rect.anchorMax = rect.anchorMin;
                    rect.sizeDelta = new Vector2(500f, 82f);
                }
                else
                {
                    rect.anchorMin = new Vector2(0.28f + i * 0.22f, 0.45f);
                    rect.anchorMax = rect.anchorMin;
                    rect.sizeDelta = new Vector2(260f, 112f);
                }

                Text buttonText = upgradeButtons[i].GetComponentInChildren<Text>();
                SetSize(buttonText, rect.sizeDelta.x - 18f, rect.sizeDelta.y - 10f);
            }

            patternPanel.ApplyResponsiveLayout(compact);
            Canvas.ForceUpdateCanvases();
        }

        private static void SetSize(Component component, float width, float height)
        {
            if (component == null)
            {
                return;
            }

            RectTransform rect = component.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
        }

        private static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int remainder = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{remainder:00}";
        }

        private static class GameManagerAccessor
        {
            public static int Score => Patterns.Singleton.GameManager.Instance == null ? 0 : Patterns.Singleton.GameManager.Instance.Score;
            public static int Kills => Patterns.Singleton.GameManager.Instance == null ? 0 : Patterns.Singleton.GameManager.Instance.Kills;
            public static float ElapsedTime => Patterns.Singleton.GameManager.Instance == null ? 0f : Patterns.Singleton.GameManager.Instance.ElapsedTime;
            public static int Wave => Patterns.Singleton.GameManager.Instance == null ? 1 : Patterns.Singleton.GameManager.Instance.Wave;
        }
    }
}
