using TrashCatcher;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TrashCatcherSceneSetup
{
    private const string LevelOneScenePath = "Assets/Scenes/TrashCatcherLevel1.unity";
    private const string LevelTwoScenePath = "Assets/Scenes/TrashCatcherLevel2.unity";

    [MenuItem("Tools/Trash Catcher/Setup Two Level Prototype Scenes")]
    public static void SetupPrototypeScene()
    {
        CreateScene(LevelOneScenePath);
        CreateScene(LevelTwoScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(LevelOneScenePath, true),
            new EditorBuildSettingsScene(LevelTwoScenePath, true)
        };

        EditorSceneManager.OpenScene(LevelOneScenePath);
        Debug.Log("Trash Catcher level scenes created. Opened " + LevelOneScenePath);
    }

    [MenuItem("Tools/Trash Catcher/Setup Prototype Scene")]
    public static void SetupPrototypeSceneLegacy()
    {
        SetupPrototypeScene();
    }

    private static void CreateScene(string scenePath)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        EditorSceneManager.SaveScene(scene, scenePath);
    }
}
