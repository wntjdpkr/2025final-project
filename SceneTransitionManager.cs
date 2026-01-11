using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;

    public const string MAIN_MENU = "MainMenu";
    public const string TUTORIAL = "Tutorial";
    public const string COMBAT = "Combat";
    public const string INTERMISSION = "Intermission";
    public const string ENDING = "Ending";

    [SerializeField] private float transitionDelay = 0.5f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void LoadMainMenu()
    {
        if (instance != null)
        {
            instance.LoadSceneAsync(MAIN_MENU);
        }
    }

    public static void LoadTutorial()
    {
        if (instance != null)
        {
            instance.LoadSceneAsync(TUTORIAL);
        }
    }

    public static void LoadCombat()
    {
        if (instance != null)
        {
            instance.LoadSceneAsync(COMBAT);
        }
    }

    public static void LoadIntermission()
    {
        if (instance != null)
        {
            instance.LoadSceneAsync(INTERMISSION);
        }
    }

    public static void LoadEnding()
    {
        if (instance != null)
        {
            instance.LoadSceneAsync(ENDING);
        }
    }

    public static void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return new WaitForSeconds(transitionDelay);

        Debug.Log($"Loading scene: {sceneName}");

        if (CombatSessionEventBus.Instance != null && sceneName != COMBAT && sceneName != TUTORIAL)
        {
            Debug.Log("SceneTransitionManager: Clearing event listeners before scene transition");
            CombatSessionEventBus.Instance.ClearAllListeners();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return null;

        RefreshLighting();

        Debug.Log($"Scene {sceneName} loaded successfully");
    }

    private void RefreshLighting()
    {
        DynamicGI.UpdateEnvironment();
        Debug.Log("Lighting refreshed for new scene");
    }
}