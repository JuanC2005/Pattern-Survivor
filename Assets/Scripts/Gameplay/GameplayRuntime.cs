using PatternSurvivor.Infrastructure;
using PatternSurvivor.Patterns.Facade;
using PatternSurvivor.Patterns.Singleton;
using PatternSurvivor.Patterns.State;
using PatternSurvivor.UI;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class GameplayRuntime : MonoBehaviour
    {
        private GameManager gameManager;
        private GameFacade facade;
        private GameObject playerObject;
        private PlayerInputReader inputReader;

        public PlayerController Player { get; private set; }
        public EnemySpawner Spawner { get; private set; }
        public ExperienceSystem Experience { get; private set; }
        public UpgradeSystem UpgradeSystem { get; private set; }

        public void Build(GameManager manager, GameFacade gameFacade)
        {
            if (Player != null)
            {
                return;
            }

            gameManager = manager;
            facade = gameFacade;
            Spawner = GetComponent<EnemySpawner>() ?? gameObject.AddComponent<EnemySpawner>();
            Experience = GetComponent<ExperienceSystem>() ?? gameObject.AddComponent<ExperienceSystem>();

            GameObject arena = new GameObject("ArenaVisual");
            arena.transform.SetParent(transform);
            arena.transform.position = new Vector3(0f, 0f, 1f);
            arena.transform.localScale = new Vector3(16f, 9f, 1f);
            SpriteRenderer arenaRenderer = arena.AddComponent<SpriteRenderer>();
            arenaRenderer.sprite = RuntimeSpriteLibrary.GetSquare("Arena", new Color(0.035f, 0.055f, 0.11f));
            RuntimeSpriteLibrary.ApplySpriteMaterial(arenaRenderer);
            arenaRenderer.sortingOrder = -20;

            playerObject = new GameObject("Player");
            playerObject.transform.SetParent(transform);
            playerObject.transform.position = Vector3.zero;
            SpriteRenderer playerRenderer = playerObject.AddComponent<SpriteRenderer>();
            playerRenderer.sprite = RuntimeSpriteLibrary.GetCircle("Player", new Color(0.2f, 0.9f, 0.8f), 38);
            RuntimeSpriteLibrary.ApplySpriteMaterial(playerRenderer);
            playerRenderer.sortingOrder = 8;
            CircleCollider2D playerCollider = playerObject.AddComponent<CircleCollider2D>();
            playerCollider.isTrigger = true;
            playerCollider.radius = 0.42f;

            Player = playerObject.AddComponent<PlayerController>();
            PlayerHealth health = playerObject.AddComponent<PlayerHealth>();
            PlayerStateMachine stateMachine = playerObject.AddComponent<PlayerStateMachine>();
            WeaponController weapon = playerObject.AddComponent<WeaponController>();
            SkillController skill = playerObject.AddComponent<SkillController>();
            inputReader = playerObject.AddComponent<PlayerInputReader>();

            Player.Configure(health, stateMachine, weapon, skill);
            health.Configure(stateMachine);
            stateMachine.Configure(Player, health, playerRenderer);
            weapon.Configure(Player);
            skill.Configure(Player);
            inputReader.Configure(Player, facade);

            UpgradeSystem = new UpgradeSystem(weapon, Experience);
            Spawner.Configure(gameManager, Player, Experience);
            gameManager.RegisterRuntime(Player, Spawner, Experience);
        }

        public void SetGameplayActive(bool active)
        {
            if (playerObject != null)
            {
                playerObject.SetActive(active);
            }

            if (Spawner != null)
            {
                Spawner.enabled = active;
            }

            if (!active)
            {
                Spawner?.ClearEnemies();
                Projectile.ClearAll();
            }
        }

        public void ResetSession()
        {
            Projectile.ClearAll();
            Spawner.ResetSession();
            Experience.ResetSession();
            Player.ResetSession();
        }
    }

    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
            GameManager gameManager = GetComponent<GameManager>();
            GameFacade facade = GetComponent<GameFacade>();
            GameplayRuntime runtime = GetComponent<GameplayRuntime>();
            GameUIController ui = GetComponent<GameUIController>();
            PatternDebugPanel patternPanel = GetComponent<PatternDebugPanel>();

            runtime.Build(gameManager, facade);
            patternPanel.Configure(runtime, facade, ui);
            ui.Build(facade, runtime, patternPanel);
            facade.Configure(gameManager, runtime, ui);
            facade.ReturnToMenu();
        }
    }
}
