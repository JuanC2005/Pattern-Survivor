using System;
using System.Collections.Generic;
using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using PatternSurvivor.Patterns.Facade;
using PatternSurvivor.Patterns.Prototype;
using PatternSurvivor.Patterns.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace PatternSurvivor.UI
{
    public sealed class PatternDebugPanel : MonoBehaviour
    {
        private sealed class PatternDefinition
        {
            public string Name;
            public string Category;
            public string Definition;
            public string Classes;
            public string Usage;
            public string Action;
        }

        private readonly List<PatternDefinition> definitions = new List<PatternDefinition>
        {
            new PatternDefinition { Name = "Singleton", Category = "Creacional", Definition = "Una única instancia coordina la partida.", Classes = "GameManager", Usage = "Estado, puntuación, tiempo y referencias globales.", Action = "Pulsa Jugar y observa GameManager.Instance." },
            new PatternDefinition { Name = "Factory Method", Category = "Creacional", Definition = "Los creadores concretos producen variantes de enemigo.", Classes = "EnemyCreator, NormalEnemyCreator, FastEnemyCreator, TankEnemyCreator", Usage = "EnemySpawner selecciona un creador según la oleada.", Action = "Espera apariciones o pulsa PROBAR para crear una demostración." },
            new PatternDefinition { Name = "Prototype", Category = "Creacional", Definition = "Clona una configuración y modifica la copia independientemente.", Classes = "EnemyPrototype, EnemyPrototypeCatalog", Usage = "Cada enemigo ajusta HP y velocidad por oleada desde un clon.", Action = "Espera una aparición o pulsa PROBAR." },
            new PatternDefinition { Name = "Builder", Category = "Creacional", Definition = "Construye configuraciones válidas de armas paso a paso.", Classes = "WeaponBuilder, WeaponDirector, WeaponConfig", Usage = "El arma inicial y las mejoras reconstruyen WeaponConfig.", Action = "Pulsa PROBAR para construir Basic, Rapid y Heavy." },
            new PatternDefinition { Name = "Flyweight", Category = "Estructural", Definition = "Comparte datos intrínsecos inmutables entre enemigos.", Classes = "EnemySharedData, EnemyDataCache", Usage = "Normal, Fast y Tank reutilizan tres objetos compartidos.", Action = "Espera enemigos y comprueba vivos frente a flyweights." },
            new PatternDefinition { Name = "Composite", Category = "Estructural", Definition = "Una habilidad compuesta trata habilidades simples con el mismo contrato.", Classes = "ISkill, HealSkill, AreaBlastSkill, CompositeSkill", Usage = "Q ejecuta curación y daño de área como una sola habilidad.", Action = "Durante la partida, pulsa Q cerca de enemigos." },
            new PatternDefinition { Name = "Decorator", Category = "Estructural", Definition = "Envuelve el arma para añadir efectos sin cambiar su base.", Classes = "IWeapon, BaseWeapon, WeaponDecorator, FireWeaponDecorator, IceWeaponDecorator", Usage = "Las mejoras de fuego y hielo agregan efectos reales a proyectiles.", Action = "Elige fuego/hielo al subir de nivel y dispara." },
            new PatternDefinition { Name = "Facade", Category = "Estructural", Definition = "Expone operaciones simples para coordinar subsistemas.", Classes = "GameFacade, GameUIController", Usage = "Jugar, pausar, reiniciar, menú y mejoras pasan por la fachada.", Action = "Usa cualquier botón de ciclo de vida." },
            new PatternDefinition { Name = "Observer", Category = "Comportamiento", Definition = "Los sistemas publican eventos que observan HUD y panel.", Classes = "GameEvents, PlayerHealth, GameUIController", Usage = "El daño emite HealthChanged y el HUD se actualiza sin referencia directa.", Action = "Deja que un enemigo golpee al jugador." },
            new PatternDefinition { Name = "State", Category = "Comportamiento", Definition = "El jugador cambia entre estados con conductas exclusivas.", Classes = "IPlayerState, PlayerStateMachine, NormalState, InvulnerableState, DeadState", Usage = "Invulnerabilidad ignora golpes consecutivos y Dead bloquea controles.", Action = "Recibe daño dos veces o deja llegar la vida a cero." },
            new PatternDefinition { Name = "Command", Category = "Comportamiento", Definition = "La entrada se convierte en objetos de acción desacoplados.", Classes = "IGameCommand, MoveCommand, ShootCommand, UseSkillCommand, PauseCommand", Usage = "PlayerInputReader ejecuta comandos, no acciones concretas directamente.", Action = "Mueve, dispara, pulsa Q o pulsa Esc." }
        };

        private GameplayRuntime runtime;
        private GameFacade facade;
        private GameUIController ui;
        private GameObject panel;
        private Dropdown selector;
        private Text detailText;
        private Text evidenceText;
        private Button testButton;

        private void OnEnable() => PatternEventLogger.EventRecorded += HandleEventRecorded;
        private void OnDisable() => PatternEventLogger.EventRecorded -= HandleEventRecorded;

        public GameObject Build(Transform parent, Color accent, Color panelColor, Color backgroundColor)
        {
            panel = CreatePanel(parent, backgroundColor);
            CreateText(panel.transform, "11 PATRONES · EVIDENCIA REAL", 34, accent, new Vector2(0.5f, 0.89f), new Vector2(900f, 50f), TextAnchor.MiddleCenter);
            selector = CreateDropdown(panel.transform, new Vector2(0.5f, 0.79f));
            foreach (PatternDefinition definition in definitions)
            {
                selector.options.Add(new Dropdown.OptionData(definition.Name));
            }
            selector.onValueChanged.AddListener(_ => Refresh());
            detailText = CreateText(panel.transform, string.Empty, 19, Color.white, new Vector2(0.5f, 0.57f), new Vector2(980f, 260f), TextAnchor.UpperCenter);
            evidenceText = CreateText(panel.transform, string.Empty, 17, new Color(0.75f, 0.9f, 1f), new Vector2(0.5f, 0.25f), new Vector2(980f, 120f), TextAnchor.MiddleCenter);
            testButton = CreateButton(panel.transform, "PROBAR", new Vector2(0.40f, 0.10f), new Color(0.2f, 0.55f, 0.8f), new Vector2(230f, 52f));
            testButton.onClick.AddListener(RunSafeDemo);
            Button close = CreateButton(panel.transform, "CERRAR", new Vector2(0.60f, 0.10f), new Color(0.5f, 0.2f, 0.35f), new Vector2(230f, 52f));
            close.onClick.AddListener(facade.ClosePatterns);
            panel.SetActive(false);
            return panel;
        }

        public void Configure(GameplayRuntime gameplayRuntime, GameFacade gameFacade, GameUIController uiController)
        {
            runtime = gameplayRuntime;
            facade = gameFacade;
            ui = uiController;
            Refresh();
        }

        public void Show() => panel.SetActive(true);
        public void Hide() => panel.SetActive(false);

        public void Refresh()
        {
            if (selector == null || detailText == null || definitions.Count == 0)
            {
                return;
            }

            int index = Mathf.Clamp(selector.value, 0, definitions.Count - 1);
            PatternDefinition definition = definitions[index];
            PatternEventRecord latest = PatternEventLogger.LatestFor(definition.Name);
            detailText.text = $"{definition.Name}  ·  {definition.Category}\n\n{definition.Definition}\n\nCLASES\n{definition.Classes}\n\nUSO EN EL JUEGO\n{definition.Usage}\n\nCÓMO COMPROBARLO\n{definition.Action}";
            evidenceText.text = latest == null
                ? "EVIDENCIA: todavía no hay un evento real registrado para este patrón."
                : $"EVIDENCIA #{latest.Index}  ·  t={latest.Time:0.00}s\n{latest.Message}";
            testButton.gameObject.SetActive(index == 0 || index == 1 || index == 2 || index == 3 || index == 4);
        }

        private void HandleEventRecorded(PatternEventRecord record)
        {
            if (panel != null && panel.activeSelf)
            {
                Refresh();
            }
        }

        private void RunSafeDemo()
        {
            int index = selector.value;
            switch (index)
            {
                case 0:
                    PatternEventLogger.Record("Singleton", $"GameManager.Instance activo: {GameManager.Instance != null}, instancia única comprobada");
                    break;
                case 1:
                    runtime.Spawner.DemonstrateFactory();
                    break;
                case 2:
                    EnemyPrototype original = EnemyPrototypeCatalog.Get("Tank");
                    EnemyPrototype clone = original.Clone();
                    clone.CurrentHealth += 7;
                    PatternEventLogger.Record("Prototype", $"Demo independiente: original {original.CurrentHealth} HP, clon {clone.CurrentHealth} HP");
                    break;
                case 3:
                    runtime.Player.Weapon.DemonstrateBuilderProfiles();
                    break;
                case 4:
                    PatternEventLogger.Record("Flyweight", $"Demo real: {Enemy.ActiveCount} enemigos vivos comparten {Patterns.Flyweight.EnemyDataCache.Count} flyweights");
                    break;
            }

            Refresh();
        }

        private static GameObject CreatePanel(Transform parent, Color color)
        {
            GameObject result = new GameObject("PatternDebugPanel");
            result.transform.SetParent(parent, false);
            Image image = result.AddComponent<Image>();
            image.color = color;
            RectTransform rect = result.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return result;
        }

        private static Text CreateText(Transform parent, string content, int size, Color color, Vector2 anchor, Vector2 dimensions, TextAnchor alignment)
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
            rect.sizeDelta = dimensions;
            return text;
        }

        private static Dropdown CreateDropdown(Transform parent, Vector2 anchor)
        {
            GameObject dropdownObject = new GameObject("PatternSelector");
            dropdownObject.transform.SetParent(parent, false);
            Image image = dropdownObject.AddComponent<Image>();
            image.color = new Color(0.1f, 0.16f, 0.27f, 1f);
            Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
            dropdown.captionText = CreateText(dropdownObject.transform, "Singleton", 18, Color.white, new Vector2(0.5f, 0.5f), new Vector2(500f, 38f), TextAnchor.MiddleCenter);
            RectTransform rect = dropdownObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(500f, 44f);

            GameObject templateObject = new GameObject("Template");
            templateObject.transform.SetParent(dropdownObject.transform, false);
            Image templateImage = templateObject.AddComponent<Image>();
            templateImage.color = new Color(0.06f, 0.1f, 0.18f, 1f);
            ScrollRect scrollRect = templateObject.AddComponent<ScrollRect>();
            RectTransform templateRect = templateObject.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0f, -2f);
            templateRect.sizeDelta = new Vector2(0f, 260f);

            GameObject viewportObject = new GameObject("Viewport");
            viewportObject.transform.SetParent(templateObject.transform, false);
            Image viewportImage = viewportObject.AddComponent<Image>();
            viewportImage.color = Color.clear;
            viewportObject.AddComponent<Mask>().showMaskGraphic = false;
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            GameObject contentObject = new GameObject("Content");
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform contentRect = contentObject.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 36f);
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;

            GameObject itemObject = new GameObject("Item");
            itemObject.transform.SetParent(contentObject.transform, false);
            Toggle itemToggle = itemObject.AddComponent<Toggle>();
            Image itemBackground = itemObject.AddComponent<Image>();
            itemBackground.color = new Color(0.12f, 0.2f, 0.32f, 1f);
            itemToggle.targetGraphic = itemBackground;
            RectTransform itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);
            itemRect.sizeDelta = new Vector2(0f, 36f);
            Text itemText = CreateText(itemObject.transform, "Pattern", 16, Color.white, new Vector2(0.5f, 0.5f), new Vector2(480f, 34f), TextAnchor.MiddleCenter);
            dropdown.template = templateRect;
            dropdown.itemText = itemText;
            templateObject.SetActive(false);
            return dropdown;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor, Color color, Vector2 dimensions)
        {
            GameObject buttonObject = new GameObject($"Button_{label}");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = dimensions;
            Text text = CreateText(buttonObject.transform, label, 18, Color.white, new Vector2(0.5f, 0.5f), dimensions, TextAnchor.MiddleCenter);
            text.resizeTextForBestFit = true;
            return button;
        }
    }
}
