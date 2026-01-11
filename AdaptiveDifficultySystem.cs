using UnityEngine;

public class AdaptiveDifficultySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossAI bossAI;
    [SerializeField] private BossAttackSystem bossAttackSystem;

    [Header("Difficulty Settings")]
    [Tooltip("2차 전투 보스 체력 배율")]
    [SerializeField] private float healthMultiplier = 1.5f;
    [Tooltip("플레이어 선호 회피 방향의 장판 범위 증가 배율")]
    [SerializeField] private float areaDirectionMultiplier = 1.3f;

    [Header("Auto Adjustment Settings")]
    [Tooltip("2차 전투에서 1차 전투 데이터를 기반으로 공격 가중치를 자동 조정할지 여부")]
    [SerializeField] private bool enableAutoWeightAdjustment = true;

    void Start()
    {
        ValidateReferences();
    }

    void ValidateReferences()
    {
        if (bossHealth == null)
        {
            bossHealth = FindObjectOfType<BossHealth>();
            if (bossHealth == null)
            {
                Debug.LogError("AdaptiveDifficultySystem: BossHealth not found!");
            }
        }

        if (bossAI == null)
        {
            bossAI = FindObjectOfType<BossAI>();
            if (bossAI == null)
            {
                Debug.LogError("AdaptiveDifficultySystem: BossAI not found!");
            }
        }

        if (bossAttackSystem == null)
        {
            bossAttackSystem = FindObjectOfType<BossAttackSystem>();
            if (bossAttackSystem == null)
            {
                Debug.LogError("AdaptiveDifficultySystem: BossAttackSystem not found!");
            }
        }
    }

    public void ApplyDifficulty(int battleNumber)
    {
        if (battleNumber == 2)
        {
            Debug.Log("AdaptiveDifficultySystem: Applying adaptive difficulty for battle 2");

            AdjustBossHealth();
            AdjustAttackPatternWeights();
            AdjustAreaCrossByDodgeDirection();
        }
    }

    void AdjustBossHealth()
    {
        if (bossHealth == null)
        {
            Debug.LogError("AdaptiveDifficultySystem: Cannot adjust health - BossHealth is null!");
            return;
        }

        int newMaxHealth = Mathf.RoundToInt(100 * healthMultiplier);
        bossHealth.SetMaxHealth(newMaxHealth);
        Debug.Log($"AdaptiveDifficultySystem: Boss health increased to {newMaxHealth}");
    }

    void AdjustAttackPatternWeights()
    {
        if (bossAI == null)
        {
            Debug.LogError("AdaptiveDifficultySystem: Cannot adjust weights - BossAI is null!");
            return;
        }

        // 자동 조정이 비활성화되어 있으면 건너뛰기
        if (!enableAutoWeightAdjustment)
        {
            Debug.Log("AdaptiveDifficultySystem: Auto weight adjustment is disabled. Using default weights from BossAI.");
            return;
        }

        int meleeTotal = CombatSessionDataStore.meleeAttackTotal;
        int areaTotal = CombatSessionDataStore.areaAttackTotal;

        if (meleeTotal == 0 && areaTotal == 0)
        {
            Debug.LogWarning("AdaptiveDifficultySystem: No attack data available for weight adjustment");
            return;
        }

        float meleeHitRate = CombatSessionDataStore.GetMeleeHitRate() / 100f;
        float areaHitRate = CombatSessionDataStore.GetAreaHitRate() / 100f;

        float meleeWeight = 0.5f;
        float areaWeight = 0.5f;

        if (meleeTotal > 0 && areaTotal > 0)
        {
            float totalHitRate = meleeHitRate + areaHitRate;

            if (totalHitRate > 0)
            {
                meleeWeight = meleeHitRate / totalHitRate;
                areaWeight = areaHitRate / totalHitRate;
            }
        }

        bossAI.AdjustAttackWeights(meleeWeight, areaWeight);
        Debug.Log($"AdaptiveDifficultySystem: Attack weights adjusted - Melee: {meleeWeight:F2}, Area: {areaWeight:F2}");
    }

    void AdjustAreaCrossByDodgeDirection()
    {
        if (bossAttackSystem == null)
        {
            Debug.LogError("AdaptiveDifficultySystem: Cannot adjust area cross - BossAttackSystem is null!");
            return;
        }

        int areaTotal = CombatSessionDataStore.areaAttackTotal;
        int areaHits = CombatSessionDataStore.areaAttackHits;

        if (areaTotal == 0)
        {
            Debug.LogWarning("AdaptiveDifficultySystem: No area attack data for direction adjustment");
            return;
        }

        float areaDodgeRate = 1.0f - ((float)areaHits / areaTotal);

        if (areaDodgeRate >= 0.5f)
        {
            Vector3 preferredDirection = CombatSessionDataStore.GetMostFrequentDodgeDirection();

            // 좌우 방향만 조정 (Vector3.left 또는 Vector3.right만 허용)
            if (preferredDirection == Vector3.left || preferredDirection == Vector3.right)
            {
                bossAttackSystem.AdjustAreaCrossSize(preferredDirection, areaDirectionMultiplier);
                Debug.Log($"AdaptiveDifficultySystem: Area cross size adjusted in direction {preferredDirection} by {areaDirectionMultiplier}x");
            }
            else if (preferredDirection == Vector3.zero)
            {
                Debug.LogWarning("AdaptiveDifficultySystem: No preferred dodge direction data");
            }
            else
            {
                Debug.Log($"AdaptiveDifficultySystem: Preferred direction {preferredDirection} is not left/right, skipping adjustment");
            }
        }
        else
        {
            Debug.Log($"AdaptiveDifficultySystem: Area dodge rate ({areaDodgeRate:F2}) below threshold (0.5), no adjustment");
        }
    }
}