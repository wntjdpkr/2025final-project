using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CombatUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private Slider bossHealthBar;
    [SerializeField] private TextMeshProUGUI dodgeStatsText;

    [Header("Game References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private DodgeCounter dodgeCounter;

    void Start()
    {
        ValidateReferences();

        SubscribeToEvents();

        StartCoroutine(DelayedInitialize());
    }

    IEnumerator DelayedInitialize()
    {
        yield return null;

        InitializeUI();

        Debug.Log("CombatUIManager: Delayed initialization completed");
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void ValidateReferences()
    {
        if (playerHealthBar == null)
            Debug.LogError("CombatUIManager: Player Health Bar is not assigned!");

        if (bossHealthBar == null)
            Debug.LogError("CombatUIManager: Boss Health Bar is not assigned!");

        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null)
                Debug.LogError("CombatUIManager: PlayerHealth not found!");
        }

        if (bossHealth == null)
        {
            bossHealth = FindObjectOfType<BossHealth>();
            if (bossHealth == null)
                Debug.LogError("CombatUIManager: BossHealth not found!");
        }

        if (dodgeCounter == null)
        {
            dodgeCounter = FindObjectOfType<DodgeCounter>();
            if (dodgeCounter == null)
                Debug.LogWarning("CombatUIManager: DodgeCounter not found! Dodge stats will not be displayed.");
        }
    }

    void InitializeUI()
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = 1f;
            playerHealthBar.value = 1f;
        }

        if (bossHealthBar != null)
        {
            bossHealthBar.maxValue = 1f;
            bossHealthBar.value = 1f;
        }

        UpdatePlayerHealthBar();
        UpdateBossHealthBar();
        UpdateDodgeStatsText();

        Debug.Log("CombatUIManager: UI initialized.");
    }

    void SubscribeToEvents()
    {
        if (CombatSessionEventBus.Instance == null)
        {
            Debug.LogError("CombatUIManager: Cannot subscribe - EventBus is null!");
            return;
        }

        CombatSessionEventBus.Instance.OnPlayerDamaged += OnPlayerDamaged;
        CombatSessionEventBus.Instance.OnBossDamaged += OnBossDamaged;
        CombatSessionEventBus.Instance.OnDodgeAttempt += OnDodgeAttempt;

        Debug.Log("CombatUIManager: Subscribed to events.");
    }

    void UnsubscribeFromEvents()
    {
        if (CombatSessionEventBus.Instance == null) return;

        CombatSessionEventBus.Instance.OnPlayerDamaged -= OnPlayerDamaged;
        CombatSessionEventBus.Instance.OnBossDamaged -= OnBossDamaged;
        CombatSessionEventBus.Instance.OnDodgeAttempt -= OnDodgeAttempt;

        Debug.Log("CombatUIManager: Unsubscribed from events.");
    }

    void OnPlayerDamaged(int remainingHealth)
    {
        UpdatePlayerHealthBar();
    }

    void OnBossDamaged(int remainingHealth)
    {
        UpdateBossHealthBar();
    }

    void OnDodgeAttempt(bool success)
    {
        UpdateDodgeStatsText();
    }

    void UpdatePlayerHealthBar()
    {
        if (playerHealth != null && playerHealthBar != null)
        {
            playerHealthBar.value = playerHealth.GetHealthPercentage();
        }
    }

    void UpdateBossHealthBar()
    {
        if (bossHealth != null && bossHealthBar != null)
        {
            bossHealthBar.value = bossHealth.GetHealthPercentage();
        }
    }

    void UpdateDodgeStatsText()
    {
        if (dodgeCounter != null && dodgeStatsText != null)
        {
            int successful = dodgeCounter.GetSuccessfulDodges();
            int total = dodgeCounter.GetTotalAttempts();
            float successRate = dodgeCounter.GetSuccessRate();

            dodgeStatsText.text = $"Dodges: {successful}/{total} ({successRate:F1}%)";
        }
    }
}
