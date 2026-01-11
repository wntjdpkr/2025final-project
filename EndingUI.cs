using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayResult();
        DisplayStats();

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        else
        {
            Debug.LogError("EndingUI: Main Menu Button is not assigned!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void DisplayResult()
    {
        if (titleText == null || resultText == null)
        {
            Debug.LogError("EndingUI: Title or Result Text is not assigned!");
            return;
        }

        // 수정된 승패 판정 로직:
        // 보스를 2번 처치해야 완전한 승리
        // 1차 전투만 이기고 2차 전투에서 죽으면 패배로 처리
        bool playerWon = CombatSessionDataStore.bossDefeatsCount >= 2;

        if (playerWon)
        {
            titleText.text = "VICTORY!";
            resultText.text = "BOSS SLAIN!";
            Debug.Log($"EndingUI: Victory! Boss defeats: {CombatSessionDataStore.bossDefeatsCount}");
        }
        else
        {
            titleText.text = "DEFEAT";
            resultText.text = "Give It Another Shot!";
            Debug.Log($"EndingUI: Defeat! Boss defeats: {CombatSessionDataStore.bossDefeatsCount}, Player deaths: {CombatSessionDataStore.playerDeaths}");
        }
    }

    void DisplayStats()
    {
        if (statsText == null)
        {
            Debug.LogError("EndingUI: Stats Text is not assigned!");
            return;
        }

        string stats = CombatSessionDataStore.GetSessionSummary();
        statsText.text = stats;

        Debug.Log("Ending stats displayed");
    }

    void OnMainMenuClicked()
    {
        Debug.Log("Returning to main menu...");
        SceneTransitionManager.LoadMainMenu();
    }

    void OnQuitClicked()
    {
        Debug.Log("Quitting game...");
        SceneTransitionManager.QuitGame();
    }

    void OnDestroy()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}