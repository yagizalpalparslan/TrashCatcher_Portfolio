using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrashCatcher
{
    public static class TrashCatcherBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadedCallback()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartPrototype()
        {
            EnsureGameExists();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureGameExists();
        }

        private static void EnsureGameExists()
        {
            if (Object.FindFirstObjectByType<TrashCatcherGame>() == null)
            {
                TrashCatcherGame.CreateGame();
            }
        }
    }
}
