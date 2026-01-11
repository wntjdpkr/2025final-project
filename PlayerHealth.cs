using UnityEngine;
using Cinemachine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 8;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Visual Feedback")]
    [SerializeField] private CombatVisualSystem visualSystem; // ← 변경됨! (DamageVignetteSystem 제거)

    private int currentHealth;
    public bool isDead = false;
    private CinemachineImpulseSource impulseSource;

    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        currentHealth = maxHealth;
        isDead = false;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // CombatVisualSystem 자동 찾기 (없으면 경고)
        if (visualSystem == null)
        {
            visualSystem = FindObjectOfType<CombatVisualSystem>();
            if (visualSystem == null)
            {
                Debug.LogWarning("PlayerHealth: CombatVisualSystem not found in scene!");
            }
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishPlayerDamaged(currentHealth);
            Debug.Log($"PlayerHealth: Initial health state published - {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.LogWarning("PlayerHealth: Cannot publish initial health - EventBus is null!");
        }

        if (impulseSource == null)
        {
            Debug.LogWarning("PlayerHealth: CinemachineImpulseSource not found!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}/{maxHealth}");

        CombatSessionDataStore.RecordDamage(damage);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishPlayerDamaged(currentHealth);
        }
        else
        {
            Debug.LogWarning("PlayerHealth: Cannot publish player damaged - EventBus is null!");
        }

        // ========== 시각 피드백 (수정됨) ==========

        // 1. Post-Processing 비네트 효과 (새로 추가)
        if (visualSystem != null)
        {
            visualSystem.ShowDamageEffect();
            Debug.Log("PlayerHealth: Vignette damage effect triggered");
        }
        else
        {
            Debug.LogWarning("PlayerHealth: CombatVisualSystem is null, no vignette effect!");
        }

        // 2. 카메라 셰이크
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
            Debug.Log("PlayerHealth: Camera shake triggered!");
        }

        // ==========================================

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("GetHit");
            }
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Debug.Log("Player died!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishPlayerDeath();
        }
        else
        {
            Debug.LogWarning("PlayerHealth: Cannot publish player death - EventBus is null!");
        }

        PlayerInputManager inputManager = GetComponent<PlayerInputManager>();
        if (inputManager != null)
        {
            inputManager.enabled = false;
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }
}