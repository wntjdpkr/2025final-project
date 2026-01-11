using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntermissionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button nextBattleButton;

    [Header("Layout Settings")]
    [SerializeField] private bool useCompactLayout = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (titleText != null)
        {
            titleText.text = "1st Combat Result";
        }

        DisplayStats();

        if (nextBattleButton != null)
        {
            nextBattleButton.onClick.AddListener(OnNextBattleClicked);
        }
        else
        {
            Debug.LogError("IntermissionUI: Next Battle Button is not assigned!");
        }
    }

    void DisplayStats()
    {
        if (statsText == null)
        {
            Debug.LogError("IntermissionUI: Stats Text is not assigned!");
            return;
        }

        string stats = useCompactLayout ? BuildCompactStats() : BuildDetailedStats();
        statsText.text = stats;

        Debug.Log("Intermission stats displayed");
    }

    string BuildCompactStats()
    {
        string stats = "";

        stats += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";

        stats += "DODGE PERFORMANCE\n";
        stats += $"  Attempts: {CombatSessionDataStore.totalDodgeAttempts}\n";
        stats += $"  Success: {CombatSessionDataStore.successfulDodges}";
        stats += $" ({CombatSessionDataStore.GetSuccessRate():F1}%)\n";
        stats += $"  Failed: {CombatSessionDataStore.failedDodges}\n\n";

        stats += "ATTACK PATTERN ANALYSIS\n";
        stats += $"  Melee: {CombatSessionDataStore.meleeAttackHits}/{CombatSessionDataStore.meleeAttackTotal}";
        if (CombatSessionDataStore.meleeAttackTotal > 0)
        {
            stats += $" ({CombatSessionDataStore.GetMeleeHitRate():F1}%)";
        }
        stats += "\n";

        stats += $"  Area: {CombatSessionDataStore.areaAttackHits}/{CombatSessionDataStore.areaAttackTotal}";
        if (CombatSessionDataStore.areaAttackTotal > 0)
        {
            stats += $" ({CombatSessionDataStore.GetAreaHitRate():F1}%)";
        }
        stats += "\n\n";

        stats += "DODGE DIRECTION PATTERN\n";
        stats += $"  Right: {CombatSessionDataStore.areaCrossLeftDodges}  ";
        stats += $"Left: {CombatSessionDataStore.areaCrossRightDodges}\n\n";

        stats += "DAMAGE TAKEN\n";
        stats += $"  Total: {CombatSessionDataStore.totalDamageTaken}";
        stats += $"  (Hits: {CombatSessionDataStore.totalHits})\n\n";

        stats += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━";

        return stats;
    }

    string BuildDetailedStats()
    {
        string stats = "\n";

        stats += "[ Dodge Stats ]\n";
        stats += $"  Total Dodge Attempts: {CombatSessionDataStore.totalDodgeAttempts}\n";
        stats += $"  Dodges Hit: {CombatSessionDataStore.successfulDodges}\n";
        stats += $"  Dodges Missed: {CombatSessionDataStore.failedDodges}\n";
        stats += $"  Dodge Rate: {CombatSessionDataStore.GetSuccessRate():F1}%\n\n";

        stats += "[ Hits Taken by Attack Pattern ]\n";
        stats += $"  Weapon Swing: {CombatSessionDataStore.meleeAttackHits} / {CombatSessionDataStore.meleeAttackTotal}";
        if (CombatSessionDataStore.meleeAttackTotal > 0)
        {
            stats += $" ({CombatSessionDataStore.GetMeleeHitRate():F1}%)";
        }
        stats += "\n";

        stats += $"  Area Attack: {CombatSessionDataStore.areaAttackHits} / {CombatSessionDataStore.areaAttackTotal}";
        if (CombatSessionDataStore.areaAttackTotal > 0)
        {
            stats += $" ({CombatSessionDataStore.GetAreaHitRate():F1}%)";
        }
        stats += "\n\n";

        stats += "[ Cross AoE Dodge Direction ]\n";
        stats += $"  Dodge Right: {CombatSessionDataStore.areaCrossLeftDodges}\n";
        stats += $"  Dodge Left: {CombatSessionDataStore.areaCrossRightDodges}\n\n";

        stats += "[ Damage Stats ]\n";
        stats += $"  Total Damage Taken: {CombatSessionDataStore.totalDamageTaken}\n";
        stats += $"  Total Hits Taken: {CombatSessionDataStore.totalHits}\n";

        return stats;
    }

    void OnNextBattleClicked()
    {
        Debug.Log("Starting next battle...");
        SceneTransitionManager.LoadCombat();
    }

    void OnDestroy()
    {
        if (nextBattleButton != null)
        {
            nextBattleButton.onClick.RemoveListener(OnNextBattleClicked);
        }
    }
}