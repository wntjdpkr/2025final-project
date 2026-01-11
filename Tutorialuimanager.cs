using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dodgeStatsText;

    [Header("Game References")]
    [SerializeField] private DodgeCounter dodgeCounter;

    void Start()
    {
        if (dodgeCounter == null)
        {
            dodgeCounter = FindObjectOfType<DodgeCounter>();
            if (dodgeCounter == null)
            {
                Debug.LogWarning("TutorialUIManager: DodgeCounter not found!");
            }
        }

        SubscribeToEvents();

        UpdateDodgeStatsText();

        Debug.Log("TutorialUIManager: Initialized.");
    }

    void OnEnable()
    {
        // OnEnable에서도 이벤트 구독 시도 (씬 전환 시를 위해)
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void SubscribeToEvents()
    {
        if (CombatSessionEventBus.Instance == null)
        {
            Debug.LogWarning("TutorialUIManager: EventBus not yet available, will retry.");
            return;
        }

        // 중복 구독 방지를 위해 먼저 해제
        CombatSessionEventBus.Instance.OnDodgeAttempt -= OnDodgeAttempt;

        // 재구독
        CombatSessionEventBus.Instance.OnDodgeAttempt += OnDodgeAttempt;

        Debug.Log("TutorialUIManager: Subscribed to events.");
    }

    void UnsubscribeFromEvents()
    {
        if (CombatSessionEventBus.Instance == null) return;

        CombatSessionEventBus.Instance.OnDodgeAttempt -= OnDodgeAttempt;

        Debug.Log("TutorialUIManager: Unsubscribed from events.");
    }

    void OnDodgeAttempt(bool success)
    {
        Debug.Log($"TutorialUIManager: Dodge attempt received - {success}");
        UpdateDodgeStatsText();
    }

    void UpdateDodgeStatsText()
    {
        if (dodgeCounter != null && dodgeStatsText != null)
        {
            int successful = dodgeCounter.GetSuccessfulDodges();
            int total = dodgeCounter.GetTotalAttempts();
            float successRate = dodgeCounter.GetSuccessRate();

            dodgeStatsText.text = $"Dodges: {successful}/{total} ({successRate:F1}%)";
            Debug.Log($"TutorialUIManager: Stats updated - {dodgeStatsText.text}");
        }
        else
        {
            if (dodgeCounter == null)
                Debug.LogWarning("TutorialUIManager: DodgeCounter is null!");
            if (dodgeStatsText == null)
                Debug.LogWarning("TutorialUIManager: dodgeStatsText is null!");
        }
    }
}