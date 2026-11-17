using System;
using PatternSurvivor.Gameplay;

namespace PatternSurvivor.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> HealthChanged;
        public static event Action<int> ScoreChanged;
        public static event Action<int> KillsChanged;
        public static event Action<int> LevelChanged;
        public static event Action<float> TimeChanged;
        public static event Action<int> WaveChanged;
        public static event Action<PlayerController> PlayerDied;
        public static event Action<Enemy> EnemySpawned;

        public static void RaiseHealthChanged(int current, int maximum) => HealthChanged?.Invoke(current, maximum);
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
        public static void RaiseKillsChanged(int kills) => KillsChanged?.Invoke(kills);
        public static void RaiseLevelChanged(int level) => LevelChanged?.Invoke(level);
        public static void RaiseTimeChanged(float time) => TimeChanged?.Invoke(time);
        public static void RaiseWaveChanged(int wave) => WaveChanged?.Invoke(wave);
        public static void RaisePlayerDied(PlayerController player) => PlayerDied?.Invoke(player);
        public static void RaiseEnemySpawned(Enemy enemy) => EnemySpawned?.Invoke(enemy);
    }
}
