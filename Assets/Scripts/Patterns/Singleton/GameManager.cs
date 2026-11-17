using PatternSurvivor.Gameplay;
using PatternSurvivor.Core;
using UnityEngine;

namespace PatternSurvivor.Patterns.Singleton
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private Rect arenaBounds = new Rect(-8f, -4.5f, 16f, 9f);

        private float lastReportedSecond = -1f;

        public GamePhase Phase { get; private set; } = GamePhase.Menu;
        public int Score { get; private set; }
        public int Kills { get; private set; }
        public float ElapsedTime { get; private set; }
        public int Wave { get; private set; } = 1;
        public Rect ArenaBounds => arenaBounds;
        public PlayerController Player { get; private set; }
        public EnemySpawner Spawner { get; private set; }
        public ExperienceSystem Experience { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            PatternEventLogger.Record("Singleton", "GameManager.Instance activo (1 instancia)");
        }

        private void Update()
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            ElapsedTime += Time.deltaTime;
            float currentSecond = Mathf.Floor(ElapsedTime);
            if (currentSecond != lastReportedSecond)
            {
                lastReportedSecond = currentSecond;
                GameEvents.RaiseTimeChanged(ElapsedTime);
            }
        }

        public void RegisterRuntime(PlayerController player, EnemySpawner spawner, ExperienceSystem experience)
        {
            Player = player;
            Spawner = spawner;
            Experience = experience;
        }

        public void ResetSession()
        {
            Score = 0;
            Kills = 0;
            ElapsedTime = 0f;
            Wave = 1;
            lastReportedSecond = -1f;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseKillsChanged(Kills);
            GameEvents.RaiseTimeChanged(ElapsedTime);
            GameEvents.RaiseWaveChanged(Wave);
        }

        public void SetPhase(GamePhase phase)
        {
            Phase = phase;
        }

        public void SetWave(int wave)
        {
            if (Wave == wave)
            {
                return;
            }

            Wave = wave;
            GameEvents.RaiseWaveChanged(Wave);
        }

        public void RegisterEnemyDefeated(Enemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            Score += enemy.PointValue;
            Kills++;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseKillsChanged(Kills);
        }
    }
}
