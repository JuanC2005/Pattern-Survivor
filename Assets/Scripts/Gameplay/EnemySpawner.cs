using PatternSurvivor.Core;
using PatternSurvivor.Patterns.FactoryMethod;
using PatternSurvivor.Patterns.Flyweight;
using PatternSurvivor.Patterns.Singleton;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float initialSpawnInterval = 2f;
        [SerializeField] private float minimumSpawnInterval = 0.45f;

        private GameManager gameManager;
        private PlayerController target;
        private ExperienceSystem experience;
        private float spawnTimer;
        private int spawnCount;

        public void Configure(GameManager manager, PlayerController player, ExperienceSystem experienceSystem)
        {
            gameManager = manager;
            target = player;
            experience = experienceSystem;
            spawnTimer = 1f;
        }

        private void Update()
        {
            if (gameManager == null || target == null || gameManager.Phase != GamePhase.Playing)
            {
                return;
            }

            int wave = 1 + Mathf.FloorToInt(gameManager.ElapsedTime / 20f);
            gameManager.SetWave(wave);
            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            SpawnEnemy(wave);
            spawnTimer = Mathf.Max(minimumSpawnInterval, initialSpawnInterval - gameManager.ElapsedTime * 0.012f);
        }

        public void ResetSession()
        {
            ClearEnemies();
            spawnTimer = 1f;
            spawnCount = 0;
        }

        public void ClearEnemies()
        {
            Enemy[] enemies = Object.FindObjectsByType<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
        }

        public void DemonstrateFactory()
        {
            if (gameManager == null || target == null)
            {
                return;
            }

            Rect bounds = gameManager.ArenaBounds;
            EnemyCreator[] creators =
            {
                new NormalEnemyCreator(transform, target, gameManager.Wave, HandleEnemyDefeated),
                new FastEnemyCreator(transform, target, gameManager.Wave, HandleEnemyDefeated),
                new TankEnemyCreator(transform, target, gameManager.Wave, HandleEnemyDefeated)
            };
            Vector2[] positions =
            {
                new Vector2(bounds.xMin + 1.2f, bounds.yMin + 1.2f),
                new Vector2(bounds.xMax - 1.2f, bounds.yMin + 1.2f),
                new Vector2(bounds.xMax - 1.2f, bounds.yMax - 1.2f)
            };
            for (int i = 0; i < creators.Length; i++)
            {
                Enemy enemy = creators[i].Create(positions[i]);
                PatternEventLogger.Record("Factory Method", $"Demo creó {enemy.SharedData.TypeName} en posición controlada");
            }
        }

        private void SpawnEnemy(int wave)
        {
            spawnCount++;
            EnemyCreator creator;
            float roll = Random.value;
            if (wave >= 3 && (spawnCount % 5 == 0 || roll > 0.88f))
            {
                creator = new TankEnemyCreator(transform, target, wave, HandleEnemyDefeated);
            }
            else if (wave >= 2 && (spawnCount % 3 == 0 || roll > 0.62f))
            {
                creator = new FastEnemyCreator(transform, target, wave, HandleEnemyDefeated);
            }
            else
            {
                creator = new NormalEnemyCreator(transform, target, wave, HandleEnemyDefeated);
            }

            Enemy enemy = creator.Create(GetSpawnPosition());
            PatternEventLogger.Record(
                "Flyweight",
                $"sharedData {enemy.SharedData.TypeName} reutilizado; vivos {Enemy.ActiveCount}, flyweights {EnemyDataCache.Count}");
        }

        private void HandleEnemyDefeated(Enemy enemy)
        {
            if (gameManager == null || enemy == null)
            {
                return;
            }

            gameManager.RegisterEnemyDefeated(enemy);
            experience.AddExperience(enemy.ExperienceValue);
        }

        private Vector2 GetSpawnPosition()
        {
            Rect bounds = gameManager.ArenaBounds;
            int side = Random.Range(0, 4);
            switch (side)
            {
                case 0: return new Vector2(bounds.xMin + 0.3f, Random.Range(bounds.yMin, bounds.yMax));
                case 1: return new Vector2(bounds.xMax - 0.3f, Random.Range(bounds.yMin, bounds.yMax));
                case 2: return new Vector2(Random.Range(bounds.xMin, bounds.xMax), bounds.yMin + 0.3f);
                default: return new Vector2(Random.Range(bounds.xMin, bounds.xMax), bounds.yMax - 0.3f);
            }
        }
    }
}
