using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using PatternSurvivor.UI;
using PatternSurvivor.Patterns.Singleton;
using UnityEngine;

namespace PatternSurvivor.Patterns.Facade
{
    public sealed class GameFacade : MonoBehaviour
    {
        private GameManager gameManager;
        private GameplayRuntime runtime;
        private GameUIController ui;
        private bool patternsOpenedFromPlaying;

        private void OnEnable()
        {
            GameEvents.LevelChanged += HandleLevelChanged;
            GameEvents.PlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.LevelChanged -= HandleLevelChanged;
            GameEvents.PlayerDied -= HandlePlayerDied;
        }

        public void Configure(GameManager manager, GameplayRuntime gameplayRuntime, GameUIController uiController)
        {
            gameManager = manager;
            runtime = gameplayRuntime;
            ui = uiController;
        }

        public void StartNewGame()
        {
            if (gameManager == null || runtime == null || ui == null)
            {
                Debug.LogError("GameFacade no está configurado.");
                return;
            }

            Time.timeScale = 1f;
            PatternEventLogger.Clear();
            PatternEventLogger.Record("Singleton", "GameManager.Instance activo (1 instancia)");
            gameManager.SetPhase(GamePhase.Menu);
            runtime.SetGameplayActive(true);
            runtime.ResetSession();
            gameManager.ResetSession();
            gameManager.SetPhase(GamePhase.Playing);
            ui.ShowGameplay();
            PatternEventLogger.Record("Facade", "StartNewGame coordinó reinicio, UI y spawner");
        }

        public void PauseGame()
        {
            if (gameManager == null || gameManager.Phase != GamePhase.Playing)
            {
                return;
            }

            Time.timeScale = 0f;
            gameManager.SetPhase(GamePhase.Paused);
            ui.ShowPause();
            PatternEventLogger.Record("Facade", "PauseGame detuvo tiempo y mostró la pausa");
        }

        public void ResumeGame()
        {
            if (gameManager == null || gameManager.Phase != GamePhase.Paused)
            {
                return;
            }

            Time.timeScale = 1f;
            gameManager.SetPhase(GamePhase.Playing);
            ui.ShowGameplay();
            PatternEventLogger.Record("Facade", "ResumeGame reanudó partida y HUD");
        }

        public void RestartGame()
        {
            PatternEventLogger.Record("Facade", "RestartGame limpió enemigos, proyectiles y estadísticas");
            StartNewGame();
        }

        public void EndGame()
        {
            if (gameManager == null)
            {
                return;
            }

            Time.timeScale = 0f;
            runtime.SetGameplayActive(false);
            gameManager.SetPhase(GamePhase.GameOver);
            ui.ShowGameOver();
            PatternEventLogger.Record("Facade", "EndGame detuvo generación y mostró Game Over");
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            runtime.SetGameplayActive(false);
            gameManager.SetPhase(GamePhase.Menu);
            ui.ShowMenu();
            PatternEventLogger.Record("Facade", "ReturnToMenu activó la pantalla inicial");
        }

        public void OpenPatterns()
        {
            patternsOpenedFromPlaying = gameManager != null && gameManager.Phase == GamePhase.Playing;
            if (patternsOpenedFromPlaying)
            {
                Time.timeScale = 0f;
                gameManager.SetPhase(GamePhase.Paused);
            }

            ui.ShowPatterns();
            PatternEventLogger.Record("Facade", "OpenPatterns mostró el panel académico");
        }

        public void ClosePatterns()
        {
            ui.HidePatterns();
            if (patternsOpenedFromPlaying && gameManager != null)
            {
                Time.timeScale = 1f;
                gameManager.SetPhase(GamePhase.Playing);
                ui.ShowGameplay();
            }
            else if (gameManager != null && gameManager.Phase == GamePhase.Paused)
            {
                ui.ShowPause();
            }
            else
            {
                ui.ShowMenu();
            }

            patternsOpenedFromPlaying = false;
        }

        public void SelectUpgrade(UpgradeOption option)
        {
            runtime.UpgradeSystem.ApplyUpgrade(option);
            Time.timeScale = 1f;
            gameManager.SetPhase(GamePhase.Playing);
            ui.ShowGameplay();
            PatternEventLogger.Record("Facade", $"SelectUpgrade aplicó {option.Title} y reanudó la partida");
        }

        private void HandleLevelChanged(int level)
        {
            if (gameManager == null || gameManager.Phase != GamePhase.Playing)
            {
                return;
            }

            Time.timeScale = 0f;
            gameManager.SetPhase(GamePhase.LevelUp);
            ui.ShowUpgradeSelection(runtime.UpgradeSystem.CreateOptions());
            PatternEventLogger.Record("Facade", $"LevelUp abrió selección de mejoras para nivel {level}");
        }

        private void HandlePlayerDied(PlayerController player)
        {
            if (gameManager != null && gameManager.Phase == GamePhase.Playing)
            {
                EndGame();
            }
        }
    }
}
