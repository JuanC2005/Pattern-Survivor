#if UNITY_EDITOR
using PatternSurvivor.Gameplay;
using PatternSurvivor.Patterns.Facade;
using PatternSurvivor.Patterns.Singleton;
using PatternSurvivor.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PatternSurvivor.Editor
{
    public static class PatternSurvivorProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/GameScene.unity";

        [MenuItem("Tools/Pattern Survivor/Generate Demo")]
        public static void GenerateDemo()
        {
            if (System.IO.File.Exists(ScenePath) &&
                !Application.isBatchMode &&
                !EditorUtility.DisplayDialog(
                    "Pattern Survivor",
                    "GameScene.unity ya existe. ¿Quieres regenerarla? La escena generada será reemplazada.",
                    "Regenerar",
                    "Cancelar"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("PatternSurvivor");
            root.AddComponent<GameBootstrap>();
            root.AddComponent<GameManager>();
            root.AddComponent<GameFacade>();
            root.AddComponent<GameplayRuntime>();
            root.AddComponent<GameUIController>();
            root.AddComponent<PatternDebugPanel>();

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.015f, 0.025f, 0.06f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();

            GameObject lightObject = new GameObject("Global Light 2D");
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
            light.color = Color.white;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = root;
            Debug.Log($"Pattern Survivor generado en {ScenePath}. Pulsa Play para probarlo.");
        }
    }
}
#endif
