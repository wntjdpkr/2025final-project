using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecondBattleDebugUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button cancelButton;

    [Header("Button Labels")]
    [SerializeField] private TextMeshProUGUI leftButtonText;
    [SerializeField] private TextMeshProUGUI rightButtonText;

    [Header("Debug Settings")]
    [SerializeField] private int debugMeleeHits = 5;
    [SerializeField] private int debugMeleeTotal = 10;
    [SerializeField] private int debugAreaHits = 3;
    [SerializeField] private int debugAreaTotal = 10;
    [SerializeField] private int debugTotalDodges = 20;
    [SerializeField] private int debugSuccessfulDodges = 15;

    void Start()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.text = "SELECT PREFERRED DODGE DIRECTION\n";
        }

        if (leftButton != null)
        {
            leftButton.onClick.AddListener(OnLeftSelected);
        }

        if (rightButton != null)
        {
            rightButton.onClick.AddListener(OnRightSelected);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancel);
        }

        if (leftButtonText != null)
        {
            leftButtonText.text = "LEFT\n";
        }

        if (rightButtonText != null)
        {
            rightButtonText.text = "RIGHT\n";
        }

        Debug.Log("SecondBattleDebugUI: Initialized");
    }

    public void ShowDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(true);
            Debug.Log("SecondBattleDebugUI: Debug panel shown");
        }
    }

    void OnLeftSelected()
    {
        Debug.Log("SecondBattleDebugUI: LEFT direction selected");
        SetupDebugData(Vector3.left);
        StartSecondBattle();
    }

    void OnRightSelected()
    {
        Debug.Log("SecondBattleDebugUI: RIGHT direction selected");
        SetupDebugData(Vector3.right);
        StartSecondBattle();
    }

    void OnCancel()
    {
        Debug.Log("SecondBattleDebugUI: Cancelled");
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }

    void SetupDebugData(Vector3 preferredDirection)
    {
        Debug.Log("=== Setting up debug data for 2nd battle ===");

        CombatSessionDataStore.ResetSession();

        CombatSessionDataStore.meleeAttackHits = debugMeleeHits;
        CombatSessionDataStore.meleeAttackTotal = debugMeleeTotal;
        CombatSessionDataStore.areaAttackHits = debugAreaHits;
        CombatSessionDataStore.areaAttackTotal = debugAreaTotal;
        CombatSessionDataStore.totalDodgeAttempts = debugTotalDodges;
        CombatSessionDataStore.successfulDodges = debugSuccessfulDodges;
        CombatSessionDataStore.failedDodges = debugTotalDodges - debugSuccessfulDodges;

        if (preferredDirection == Vector3.left)
        {
            CombatSessionDataStore.areaCrossLeftDodges = 15;
            CombatSessionDataStore.areaCrossRightDodges = 5;
            Debug.Log("Debug data: Preferred direction set to LEFT (15 left dodges, 5 right dodges)");
        }
        else if (preferredDirection == Vector3.right)
        {
            CombatSessionDataStore.areaCrossLeftDodges = 5;
            CombatSessionDataStore.areaCrossRightDodges = 15;
            Debug.Log("Debug data: Preferred direction set to RIGHT (5 left dodges, 15 right dodges)");
        }

        CombatSessionDataStore.bossDefeatsCount = 1;

        Debug.Log($"Debug data summary:");
        Debug.Log($"  Melee: {debugMeleeHits}/{debugMeleeTotal} hits");
        Debug.Log($"  Area: {debugAreaHits}/{debugAreaTotal} hits");
        Debug.Log($"  Dodge success: {debugSuccessfulDodges}/{debugTotalDodges}");
        Debug.Log($"  Left dodges: {CombatSessionDataStore.areaCrossLeftDodges}");
        Debug.Log($"  Right dodges: {CombatSessionDataStore.areaCrossRightDodges}");
        Debug.Log($"  Boss defeats: {CombatSessionDataStore.bossDefeatsCount}");
    }

    void StartSecondBattle()
    {
        CombatManager.ResetBattleCount();

        CombatManager.IncrementBattleCount();

        Debug.Log("SecondBattleDebugUI: Starting 2nd battle (debug mode)");
        Debug.Log($"Battle count set to: {CombatManager.GetBattleCount()}");

        SceneTransitionManager.LoadCombat();
    }

    void OnDestroy()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveListener(OnLeftSelected);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveListener(OnRightSelected);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(OnCancel);
        }
    }
}