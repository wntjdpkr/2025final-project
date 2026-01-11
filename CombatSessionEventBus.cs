using UnityEngine;
using System;

public class CombatSessionEventBus : MonoBehaviour
{
    private static CombatSessionEventBus instance;

    public static CombatSessionEventBus Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject eventBusObject = new GameObject("CombatSessionEventBus");
                instance = eventBusObject.AddComponent<CombatSessionEventBus>();
                DontDestroyOnLoad(eventBusObject);
                Debug.Log("CombatSessionEventBus: Singleton instance created.");
            }
            return instance;
        }
    }

    public event Action<bool> OnDodgeAttempt;
    public event Action<int> OnPlayerDamaged;
    public event Action OnPlayerDeath;
    public event Action<BossAttackSystem.AttackPattern> OnBossAttackStart;
    public event Action<BossAttackSystem.AttackPattern, bool> OnBossAttackEnd;
    public event Action<int> OnBossDamaged;
    public event Action OnBossDefeated;
    public event Action OnCombatStart;
    public event Action OnCombatEnd;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("CombatSessionEventBus: Instance initialized in Awake.");
        }
        else if (instance != this)
        {
            Debug.Log("CombatSessionEventBus: Duplicate instance destroyed.");
            Destroy(gameObject);
        }
    }

    public void PublishDodgeAttempt(bool success)
    {
        OnDodgeAttempt?.Invoke(success);
        Debug.Log($"CombatSessionEventBus: Dodge attempt published. Success: {success}");
    }

    public void PublishPlayerDamaged(int remainingHealth)
    {
        OnPlayerDamaged?.Invoke(remainingHealth);
        Debug.Log($"CombatSessionEventBus: Player damaged. Remaining health: {remainingHealth}");
    }

    public void PublishPlayerDeath()
    {
        OnPlayerDeath?.Invoke();
        Debug.Log("CombatSessionEventBus: Player death published.");
    }

    public void PublishBossAttackStart(BossAttackSystem.AttackPattern pattern)
    {
        OnBossAttackStart?.Invoke(pattern);
        Debug.Log($"CombatSessionEventBus: Boss attack started. Pattern: {pattern}");
    }

    public void PublishBossAttackEnd(BossAttackSystem.AttackPattern pattern, bool playerWasHit)
    {
        OnBossAttackEnd?.Invoke(pattern, playerWasHit);
        Debug.Log($"CombatSessionEventBus: Boss attack ended. Pattern: {pattern}, Player hit: {playerWasHit}");
    }

    public void PublishBossDamaged(int remainingHealth)
    {
        OnBossDamaged?.Invoke(remainingHealth);
        Debug.Log($"CombatSessionEventBus: Boss damaged. Remaining health: {remainingHealth}");
    }

    public void PublishBossDefeated()
    {
        OnBossDefeated?.Invoke();
        Debug.Log("CombatSessionEventBus: Boss defeated published.");
    }

    public void PublishCombatStart()
    {
        OnCombatStart?.Invoke();
        Debug.Log("CombatSessionEventBus: Combat started.");
    }

    public void PublishCombatEnd()
    {
        OnCombatEnd?.Invoke();
        Debug.Log("CombatSessionEventBus: Combat ended.");
    }

    public void ClearAllListeners()
    {
        OnDodgeAttempt = null;
        OnPlayerDamaged = null;
        OnPlayerDeath = null;
        OnBossAttackStart = null;
        OnBossAttackEnd = null;
        OnBossDamaged = null;
        OnBossDefeated = null;
        OnCombatStart = null;
        OnCombatEnd = null;

        Debug.Log("CombatSessionEventBus: All event listeners cleared.");
    }
}
