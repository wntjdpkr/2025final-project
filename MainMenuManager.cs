using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button debugSecondBattleButton;

    [Header("Debug UI")]
    [SerializeField] private SecondBattleDebugUI debugUI;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CombatSessionDataStore.ResetSession();
        CombatManager.ResetBattleCount();

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("MainMenuManager: Start Button is not assigned!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        else
        {
            Debug.LogError("MainMenuManager: Quit Button is not assigned!");
        }

        if (debugSecondBattleButton != null)
        {
            debugSecondBattleButton.onClick.AddListener(OnDebugSecondBattleClicked);
            Debug.Log("MainMenuManager: Debug Second Battle button registered");
        }

        if (debugUI == null)
        {
            debugUI = FindObjectOfType<SecondBattleDebugUI>();
            if (debugUI == null)
            {
                Debug.LogWarning("MainMenuManager: SecondBattleDebugUI not found!");
            }
        }

        Debug.Log("MainMenu loaded successfully");
    }

    void OnStartButtonClicked()
    {
        Debug.Log("Starting game...");
        SceneTransitionManager.LoadTutorial();
    }

    void OnQuitButtonClicked()
    {
        Debug.Log("Quitting game...");
        SceneTransitionManager.QuitGame();
    }

    void OnDebugSecondBattleClicked()
    {
        Debug.Log("MainMenuManager: Debug Second Battle button clicked");
        if (debugUI != null)
        {
            debugUI.ShowDebugPanel();
        }
        else
        {
            Debug.LogError("MainMenuManager: SecondBattleDebugUI is not available!");
        }
    }

    void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }

        if (debugSecondBattleButton != null)
        {
            debugSecondBattleButton.onClick.RemoveListener(OnDebugSecondBattleClicked);
        }
    }
}