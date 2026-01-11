using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("Tutorial Objects")]
    [SerializeField] private TutorialAttackTrigger attackTrigger;
    [SerializeField] private TutorialExitTrigger exitTrigger;

    private bool hasStartedTutorial = false;
    private bool instructionsVisible = true;

    private const string INSTRUCTIONS_TEXT =
        "TUTORIAL\n\n" +
        "CONTROLS:\n" +
        "WASD - Move\n" +
        "Space - Jump\n" +
        "Shift - Dash\n" +
        "Left Click - Attack\n\n" +
        "GAME FEATURES:\n" +
        "- Dodge boss attacks to survive\n" +
        "- The boss learns from your patterns\n" +
        "- Adapt your strategy in the second battle\n\n" +
        "INSTRUCTIONS:\n" +
        "1. Attack the RED cube to start cross attacks\n" +
        "2. Dodge 8 cross attacks by moving away\n" +
        "3. Attack the GRAY cube to proceed to combat\n\n" +
        "Press any key to start";

    void Start()
    {
        // 커서 표시 (인스트럭션 읽는 동안)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }

        if (instructionsText != null)
        {
            instructionsText.text = INSTRUCTIONS_TEXT;
        }

        if (attackTrigger != null)
        {
            attackTrigger.gameObject.SetActive(false);
            Debug.Log("TutorialManager: AttackTrigger deactivated.");
        }
        else
        {
            Debug.LogError("TutorialManager: AttackTrigger is not assigned!");
        }

        if (exitTrigger != null)
        {
            exitTrigger.gameObject.SetActive(false);
            Debug.Log("TutorialManager: ExitTrigger deactivated (will activate with AttackTrigger).");
        }
        else
        {
            Debug.LogError("TutorialManager: ExitTrigger is not assigned!");
        }

        Debug.Log("TutorialManager: Instructions displayed. Waiting for input.");
    }

    void Update()
    {
        if (instructionsVisible && Input.anyKeyDown)
        {
            StartTutorial();
        }
    }

    void StartTutorial()
    {
        instructionsVisible = false;

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            Debug.Log("TutorialManager: Instructions panel hidden.");
        }

        // 커서 잠금 (게임플레이 시작)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("TutorialManager: Cursor locked.");

        if (attackTrigger != null)
        {
            attackTrigger.gameObject.SetActive(true);
            Debug.Log("TutorialManager: AttackTrigger activated.");
        }

        if (exitTrigger != null)
        {
            exitTrigger.gameObject.SetActive(true);
            Debug.Log("TutorialManager: ExitTrigger activated (visible from start).");
        }

        hasStartedTutorial = true;
        Debug.Log("TutorialManager: Tutorial started. Attack RED cube to begin, GREEN cube to exit.");
    }

    public void OnAttackSequenceComplete()
    {
        Debug.Log("TutorialManager: Attack sequence completed. ExitTrigger already visible.");
    }

    public void OnTutorialComplete()
    {
        Debug.Log("TutorialManager: Tutorial completed. Loading Combat scene...");

        // 커서 복원 (씬 전환 전)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneTransitionManager.LoadCombat();
    }
}