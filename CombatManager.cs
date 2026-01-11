using UnityEngine;

public class CombatManager : MonoBehaviour
{
    private static int battleCount = 0;

    [Header("References")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BossAI bossAI;
    [SerializeField] private BossAttackSystem bossAttackSystem;
    [SerializeField] private AdaptiveDifficultySystem adaptiveDifficultySystem;

    [Header("Ground Objects")]
    [Tooltip("1차 전투에서 사용할 그라운드 오브젝트")]
    [SerializeField] private GameObject firstBattleGround;
    [Tooltip("2차 전투에서 사용할 그라운드 오브젝트")]
    [SerializeField] private GameObject secondBattleGround;

    void Start()
    {
        battleCount++;
        Debug.Log($"Battle {battleCount} started");

        ValidateReferences();

        SwitchGround(battleCount);

        SubscribeToEvents();

        if (battleCount == 2 && adaptiveDifficultySystem != null)
        {
            adaptiveDifficultySystem.ApplyDifficulty(battleCount);
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishCombatStart();
        }
        else
        {
            Debug.LogWarning("CombatManager: Cannot publish combat start - EventBus is null!");
        }
    }

    void SwitchGround(int battle)
    {
        if (firstBattleGround == null && secondBattleGround == null)
        {
            Debug.LogWarning("CombatManager: No ground objects assigned. Skipping ground switch.");
            return;
        }

        if (battle == 1)
        {
            if (firstBattleGround != null)
            {
                firstBattleGround.SetActive(true);
                Debug.Log("CombatManager: First battle ground activated.");
            }

            if (secondBattleGround != null)
            {
                secondBattleGround.SetActive(false);
                Debug.Log("CombatManager: Second battle ground deactivated.");
            }
        }
        else if (battle == 2)
        {
            if (firstBattleGround != null)
            {
                firstBattleGround.SetActive(false);
                Debug.Log("CombatManager: First battle ground deactivated.");
            }

            if (secondBattleGround != null)
            {
                secondBattleGround.SetActive(true);
                Debug.Log("CombatManager: Second battle ground activated.");
            }
        }
        else
        {
            Debug.LogWarning($"CombatManager: Unexpected battle count {battle}. No ground switch performed.");
        }
    }

    void SubscribeToEvents()
    {
        if (CombatSessionEventBus.Instance == null)
        {
            Debug.LogError("CombatManager: Cannot subscribe - EventBus is null!");
            return;
        }

        CombatSessionEventBus.Instance.OnBossDefeated += OnBossDefeated;
        CombatSessionEventBus.Instance.OnPlayerDeath += OnPlayerDeath;

        Debug.Log("CombatManager: Subscribed to events.");
    }

    void OnDisable()
    {
        if (CombatSessionEventBus.Instance == null) return;

        CombatSessionEventBus.Instance.OnBossDefeated -= OnBossDefeated;
        CombatSessionEventBus.Instance.OnPlayerDeath -= OnPlayerDeath;

        Debug.Log("CombatManager: Unsubscribed from events.");
    }

    void ValidateReferences()
    {
        if (bossHealth == null)
        {
            bossHealth = FindObjectOfType<BossHealth>();
            if (bossHealth == null)
            {
                Debug.LogError("CombatManager: BossHealth not found!");
            }
        }

        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("CombatManager: PlayerHealth not found!");
            }
        }

        if (bossAI == null)
        {
            bossAI = FindObjectOfType<BossAI>();
            if (bossAI == null)
            {
                Debug.LogError("CombatManager: BossAI not found!");
            }
        }

        if (bossAttackSystem == null)
        {
            bossAttackSystem = FindObjectOfType<BossAttackSystem>();
            if (bossAttackSystem == null)
            {
                Debug.LogError("CombatManager: BossAttackSystem not found!");
            }
        }

        if (adaptiveDifficultySystem == null)
        {
            adaptiveDifficultySystem = FindObjectOfType<AdaptiveDifficultySystem>();
            if (adaptiveDifficultySystem == null)
            {
                Debug.LogWarning("CombatManager: AdaptiveDifficultySystem not found!");
            }
        }
    }

    void OnBossDefeated()
    {
        Debug.Log("Boss defeated!");
        CombatSessionDataStore.RecordCombatEnd(true);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishCombatEnd();
        }
        else
        {
            Debug.LogWarning("CombatManager: Cannot publish combat end - EventBus is null!");
        }

        if (battleCount == 1)
        {
            Debug.Log("Battle 1 complete. Loading Intermission...");
            Invoke(nameof(LoadIntermission), 2f);
        }
        else if (battleCount == 2)
        {
            Debug.Log("Battle 2 complete. Loading Ending...");
            Invoke(nameof(LoadEnding), 2f);
        }
    }

    void OnPlayerDeath()
    {
        Debug.Log("Player died!");
        CombatSessionDataStore.RecordCombatEnd(false);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishCombatEnd();
        }
        else
        {
            Debug.LogWarning("CombatManager: Cannot publish combat end - EventBus is null!");
        }

        Debug.Log("Loading Ending...");
        Invoke(nameof(LoadEnding), 2f);
    }

    void LoadIntermission()
    {
        SceneTransitionManager.LoadIntermission();
    }

    void LoadEnding()
    {
        SceneTransitionManager.LoadEnding();
    }

    public static void ResetBattleCount()
    {
        battleCount = 0;
        Debug.Log("Battle count reset");
    }

    public static int GetBattleCount()
    {
        return battleCount;
    }

    public static void IncrementBattleCount()
    {
        battleCount++;
        Debug.Log($"Battle count manually incremented to {battleCount}");
    }
}