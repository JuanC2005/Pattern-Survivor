#if UNITY_EDITOR
using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using PatternSurvivor.Patterns.Facade;
using PatternSurvivor.Patterns.Singleton;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PatternSurvivor.Editor
{
    public static class PatternSurvivorSmokeTest
    {
        private const string ScenePath = "Assets/Scenes/GameScene.unity";
        private static double deadline;
        private static int stage;

        [MenuItem("Tools/Pattern Survivor/Run Smoke Test")]
        public static void Run()
        {
            EditorSceneManager.OpenScene(ScenePath);
            stage = 0;
            deadline = EditorApplication.timeSinceStartup + 8d;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup < deadline)
            {
                return;
            }

            try
            {
                GameManager manager = Object.FindAnyObjectByType<GameManager>();
                GameFacade facade = Object.FindAnyObjectByType<GameFacade>();
                GameplayRuntime runtime = Object.FindAnyObjectByType<GameplayRuntime>();
                Require(manager != null, "GameManager no fue creado.");
                Require(facade != null, "GameFacade no fue creado.");
                Require(runtime != null && runtime.Player != null, "GameplayRuntime no construyó jugador.");

                if (stage == 0)
                {
                    Require(manager.Phase == GamePhase.Menu, "La escena no terminó en menú inicial.");
                    facade.StartNewGame();
                    Require(manager.Phase == GamePhase.Playing, "StartNewGame no inició Playing.");
                    stage = 1;
                    deadline = EditorApplication.timeSinceStartup + 4d;
                    return;
                }

                Require(runtime.Spawner != null, "EnemySpawner no fue configurado.");
                Require(Object.FindObjectsByType<Enemy>().Length > 0, "No apareció ningún enemigo después de 4 segundos.");
                runtime.Player.Health.TakeDamage(10);
                Require(runtime.Player.Health.CurrentHealth == 90, "El primer daño no redujo HP a 90.");
                runtime.Player.Health.TakeDamage(10);
                Require(runtime.Player.Health.CurrentHealth == 90, "La invulnerabilidad no bloqueó el segundo daño.");
                Require(runtime.Player.StateMachine.CurrentState.Name == "InvulnerableState", "No se activó InvulnerableState.");
                runtime.Player.Weapon.DemonstrateBuilderProfiles();
                Require(PatternEventLogger.LatestFor("Builder") != null, "Builder no registró evidencia.");
                runtime.Player.UseSkill();
                Require(PatternEventLogger.LatestFor("Composite") != null, "Composite no registró evidencia.");
                Require(GameObject.Find("SkillPulse") != null, "La habilidad no creó el pulso visual.");
                facade.OpenPatterns();
                Require(manager.Phase == GamePhase.Paused, "OpenPatterns no pausó una partida activa.");
                facade.ClosePatterns();
                Require(manager.Phase == GamePhase.Playing, "ClosePatterns no reanudó la partida.");
                facade.RestartGame();
                Require(manager.Phase == GamePhase.Playing, "RestartGame no reanudó Playing.");
                Debug.Log("PATTERN SURVIVOR SMOKE TEST PASSED");
                Finish(0);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("PATTERN SURVIVOR SMOKE TEST FAILED");
                Finish(1);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.InvalidOperationException(message);
            }
        }

        private static void Finish(int exitCode)
        {
            EditorApplication.update -= Tick;
            EditorApplication.isPlaying = false;
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }
    }
}
#endif
