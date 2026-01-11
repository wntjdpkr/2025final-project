using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 80;
    private int currentHealth;
    private bool isDead = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 2.0f;

    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("BossHealth: Animator not found!");
            }
        }

        Debug.Log($"BossHealth: Initialized with {currentHealth} HP");

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossDamaged(currentHealth);
            Debug.Log($"BossHealth: Initial health state published - {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.LogWarning("BossHealth: Cannot publish initial health - EventBus is null!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"BossHealth: Took {damage} damage. Current HP: {currentHealth}/{maxHealth}");

        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger("TakeDamage");
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossDamaged(currentHealth);
        }
        else
        {
            Debug.LogWarning("BossHealth: Cannot publish boss damaged - EventBus is null!");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("BossHealth: Boss defeated!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetBool("IsAlive", false);
        }

        BossAI bossAI = GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.StopAI();
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossDefeated();
        }
        else
        {
            Debug.LogWarning("BossHealth: Cannot publish boss defeated - EventBus is null!");
        }

        Invoke(nameof(DeactivateBoss), deathDelay);
    }

    void DeactivateBoss()
    {
        gameObject.SetActive(false);
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

    public bool IsDead()
    {
        return isDead;
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        Debug.Log($"BossHealth: Max health set to {maxHealth}");
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"BossHealth: Healed {amount}. Current HP: {currentHealth}/{maxHealth}");

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossDamaged(currentHealth);
        }
        else
        {
            Debug.LogWarning("BossHealth: Cannot publish boss damaged (heal) - EventBus is null!");
        }
    }
}
