using UnityEngine;
using System.Collections;

public class SlowMotionChargeSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform boss;
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody bossRigidbody;
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private CombatVisualSystem visualSystem;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Charge Settings")]
    [SerializeField] private float chargeSpeed = 20f;
    [SerializeField] private float maxChargeDuration = 2.5f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionDuration = 2f;
    [SerializeField] private float chargeDelay = 0.2f;

    [Header("Trail Settings")]
    [SerializeField] private float trailFadeOutTime = 0.5f;

    private bool isCharging = false;

    void Start()
    {
        ValidateReferences();
        InitializeTrail();
    }

    void ValidateReferences()
    {
        if (boss == null)
        {
            boss = transform;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("SlowMotionChargeSystem: Player not found!");
            }
        }

        if (bossRigidbody == null)
        {
            bossRigidbody = GetComponent<Rigidbody>();
            if (bossRigidbody == null)
            {
                Debug.LogError("SlowMotionChargeSystem: Rigidbody not found!");
            }
        }

        if (bossAnimator == null)
        {
            bossAnimator = GetComponent<Animator>();
            if (bossAnimator == null)
            {
                Debug.LogWarning("SlowMotionChargeSystem: Animator not found!");
            }
        }

        if (visualSystem == null)
        {
            visualSystem = FindObjectOfType<CombatVisualSystem>();
            if (visualSystem == null)
            {
                Debug.LogWarning("SlowMotionChargeSystem: CombatVisualSystem not found! Visual effects will not work.");
            }
        }

        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                Debug.LogWarning("SlowMotionChargeSystem: TrailRenderer not found! Trail effect will not work.");
            }
        }
    }

    void InitializeTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            Debug.Log("SlowMotionChargeSystem: Trail renderer initialized (disabled)");
        }
    }

    public bool ShouldTriggerSlowMotionCharge(Vector3 currentDodgeDirection)
    {
        Vector3 preferredDirection = CombatSessionDataStore.GetMostFrequentDodgeDirection();

        if (preferredDirection == Vector3.zero)
        {
            Debug.Log("SlowMotionChargeSystem: No preferred dodge direction data.");
            return false;
        }

        if (preferredDirection != Vector3.left && preferredDirection != Vector3.right)
        {
            Debug.Log($"SlowMotionChargeSystem: Preferred direction {preferredDirection} is not left/right.");
            return false;
        }

        bool isDifferentDirection = (currentDodgeDirection != preferredDirection);

        if (isDifferentDirection)
        {
            Debug.Log($"SlowMotionChargeSystem: Player dodged in different direction! Current: {currentDodgeDirection}, Preferred: {preferredDirection}. Triggering slow motion charge!");
        }
        else
        {
            Debug.Log($"SlowMotionChargeSystem: Player dodged in preferred direction. Current: {currentDodgeDirection}, Preferred: {preferredDirection}");
        }

        return isDifferentDirection;
    }

    public IEnumerator ExecuteSlowMotionCharge()
    {
        if (isCharging)
        {
            Debug.LogWarning("SlowMotionChargeSystem: Already charging!");
            yield break;
        }

        isCharging = true;

        Debug.Log("SlowMotionChargeSystem: Starting slow motion charge sequence");

        EnableTrail();

        if (SlowMotionManager.Instance != null)
        {
            SlowMotionManager.Instance.ActivateSlowMotion();
            Debug.Log("SlowMotionChargeSystem: Slow motion activated");
        }

        if (visualSystem != null)
        {
            visualSystem.ShowSlowMotionEffect();
            Debug.Log("SlowMotionChargeSystem: Visual effects activated");
        }

        yield return new WaitForSeconds(chargeDelay);

        if (player == null)
        {
            Debug.LogError("SlowMotionChargeSystem: Player is null, cannot charge!");
            DisableTrail();
            DeactivateSlowMotion();
            isCharging = false;
            yield break;
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IsWalking", true);
            Debug.Log("SlowMotionChargeSystem: RunForwardBattle animation started");
        }

        Debug.Log("SlowMotionChargeSystem: Starting player-tracking charge");

        float elapsed = 0f;

        while (true)
        {
            Vector3 chargeDirection = (player.position - boss.position).normalized;
            chargeDirection.y = 0f;

            if (chargeDirection.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(chargeDirection);
                boss.rotation = Quaternion.Slerp(boss.rotation, targetRotation, Time.deltaTime * 10f);
            }

            float distance = Vector3.Distance(
                new Vector3(boss.position.x, 0f, boss.position.z),
                new Vector3(player.position.x, 0f, player.position.z)
            );

            if (distance <= stopDistance)
            {
                Debug.Log($"SlowMotionChargeSystem: Reached stop distance ({distance:F2}m). Stopping charge.");
                break;
            }

            if (elapsed >= maxChargeDuration)
            {
                Debug.Log($"SlowMotionChargeSystem: Max charge duration ({maxChargeDuration}s) reached. Stopping charge.");
                break;
            }

            if (bossRigidbody != null)
            {
                Vector3 velocity = chargeDirection * chargeSpeed;
                velocity.y = bossRigidbody.velocity.y;
                bossRigidbody.velocity = velocity;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (bossRigidbody != null)
        {
            bossRigidbody.velocity = Vector3.zero;
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IsWalking", false);
            Debug.Log("SlowMotionChargeSystem: RunForwardBattle animation stopped");
        }

        DisableTrail();

        isCharging = false;

        Debug.Log("SlowMotionChargeSystem: Charge completed, executing immediate melee attack (slow motion continues)");

        BossAttackSystem attackSystem = GetComponent<BossAttackSystem>();
        if (attackSystem != null)
        {
            yield return StartCoroutine(attackSystem.ForceExecuteMeleeAttack());

            DeactivateSlowMotion();
            Debug.Log("SlowMotionChargeSystem: Melee attack completed, slow motion deactivated");

            BossAI bossAI = GetComponent<BossAI>();
            if (bossAI != null)
            {
                bossAI.ResetAttackCooldown();
                Debug.Log("SlowMotionChargeSystem: Reset BossAI attack cooldown");
            }
        }
        else
        {
            Debug.LogWarning("SlowMotionChargeSystem: BossAttackSystem not found, cannot execute melee attack!");
            DeactivateSlowMotion();
        }
    }

    void EnableTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = true;
            Debug.Log("SlowMotionChargeSystem: Trail renderer enabled");
        }
    }

    void DisableTrail()
    {
        if (trailRenderer != null)
        {
            StartCoroutine(FadeOutTrail());
        }
    }

    IEnumerator FadeOutTrail()
    {
        Debug.Log($"SlowMotionChargeSystem: Trail fading out over {trailFadeOutTime} seconds");
        yield return new WaitForSeconds(trailFadeOutTime);

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            Debug.Log("SlowMotionChargeSystem: Trail renderer disabled");
        }
    }

    void DeactivateSlowMotion()
    {
        if (SlowMotionManager.Instance != null)
        {
            SlowMotionManager.Instance.DeactivateSlowMotion();
            Debug.Log("SlowMotionChargeSystem: Slow motion deactivated");
        }

        if (visualSystem != null)
        {
            visualSystem.HideSlowMotionEffect();
            Debug.Log("SlowMotionChargeSystem: Visual effects deactivated");
        }
    }

    void OnDestroy()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
        DeactivateSlowMotion();
    }

    void OnDisable()
    {
        if (isCharging)
        {
            if (bossRigidbody != null)
            {
                bossRigidbody.velocity = Vector3.zero;
            }

            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
            }

            DeactivateSlowMotion();
            isCharging = false;
        }
    }
}