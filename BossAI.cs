using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Chasing,
        Attacking
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private BossAttackSystem attackSystem;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Animator animator;

    [Header("AI Settings")]
    [SerializeField] private float attackInterval = 3f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float meleeAttackRange = 3f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Attack Weights")]
    [Tooltip("근접 공격(무기 휘두르기) 가중치 - 1차 전투 기본값")]
    [SerializeField][Range(0f, 1f)] private float meleeAttackWeight = 0.5f;
    [Tooltip("장판 공격(십자 영역) 가중치 - 1차 전투 기본값")]
    [SerializeField][Range(0f, 1f)] private float areaAttackWeight = 0.5f;

    private BossState currentState = BossState.Idle;
    private bool isDead = false;
    private float lastAttackTime = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;

            if (rb.isKinematic)
            {
                Debug.LogWarning("BossAI: Rigidbody is Kinematic! Setting to non-kinematic for movement.");
                rb.isKinematic = false;
            }
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
                Debug.LogError("BossAI: Player not found!");
            }
        }

        if (attackSystem == null)
        {
            attackSystem = GetComponent<BossAttackSystem>();
            if (attackSystem == null)
            {
                Debug.LogError("BossAI: BossAttackSystem not found!");
            }
        }

        if (bossHealth == null)
        {
            bossHealth = GetComponent<BossHealth>();
            if (bossHealth == null)
            {
                Debug.LogWarning("BossAI: BossHealth not found!");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("BossAI: Animator not found!");
            }
        }

        TransitionToState(BossState.Idle);
    }

    void Update()
    {
        if (isDead) return;

        if (bossHealth != null && bossHealth.IsDead())
        {
            isDead = true;
            StopAI();
            return;
        }

        LookAtPlayer();

        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdleState();
                break;
            case BossState.Chasing:
                UpdateChasingState();
                break;
            case BossState.Attacking:
                break;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (currentState == BossState.Chasing)
        {
            MoveTowardsPlayer();
        }
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void UpdateIdleState()
    {
        if (!IsPlayerInRange()) return;

        if (!CanAttack()) return;

        BossAttackSystem.AttackPattern selectedPattern = SelectAttackPattern();

        if (selectedPattern == BossAttackSystem.AttackPattern.MeleeSwing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > meleeAttackRange)
            {
                Debug.Log($"BossAI: Player too far for melee ({distanceToPlayer:F2}m). Starting chase.");
                TransitionToState(BossState.Chasing);
            }
            else
            {
                Debug.Log($"BossAI: Player in melee range ({distanceToPlayer:F2}m). Attacking.");
                StartAttack(selectedPattern);
            }
        }
        else
        {
            Debug.Log("BossAI: Using area attack.");
            StartAttack(selectedPattern);
        }
    }

    void UpdateChasingState()
    {
        if (!IsPlayerInRange())
        {
            Debug.Log("BossAI: Player out of detection range. Returning to Idle.");
            TransitionToState(BossState.Idle);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= meleeAttackRange)
        {
            Debug.Log($"BossAI: Reached attack range ({distanceToPlayer:F2}m). Starting attack.");
            StartAttack(BossAttackSystem.AttackPattern.MeleeSwing);
        }
    }

    void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (rb != null)
        {
            Vector3 newVelocity = direction * moveSpeed;
            newVelocity.y = rb.velocity.y;
            rb.velocity = newVelocity;
        }
        else
        {
            transform.position += direction * moveSpeed * Time.fixedDeltaTime;
        }
    }

    void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void StartAttack(BossAttackSystem.AttackPattern pattern)
    {
        TransitionToState(BossState.Attacking);
        StartCoroutine(AttackCoroutine(pattern));
    }

    IEnumerator AttackCoroutine(BossAttackSystem.AttackPattern pattern)
    {
        if (pattern == BossAttackSystem.AttackPattern.MeleeSwing)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > meleeAttackRange)
            {
                Debug.Log("BossAI: Player escaped before attack! Returning to Idle.");
                TransitionToState(BossState.Idle);
                yield break;
            }
        }

        yield return attackSystem.ExecuteAttack(pattern);

        lastAttackTime = Time.time;

        Debug.Log("BossAI: Attack complete. Returning to Idle.");
        TransitionToState(BossState.Idle);
    }

    void TransitionToState(BossState newState)
    {
        if (currentState == newState) return;

        ExitState(currentState);
        currentState = newState;
        EnterState(newState);

        Debug.Log($"BossAI: State changed to {newState}");
    }

    void EnterState(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                StopMovement();
                if (animator != null)
                {
                    animator.SetBool("IsWalking", false);
                }
                break;

            case BossState.Chasing:
                if (animator != null)
                {
                    animator.SetBool("IsWalking", true);
                }
                break;

            case BossState.Attacking:
                StopMovement();
                if (animator != null)
                {
                    animator.SetBool("IsWalking", false);
                }
                break;
        }
    }

    void ExitState(BossState state)
    {
        switch (state)
        {
            case BossState.Chasing:
                StopMovement();
                if (animator != null)
                {
                    animator.SetBool("IsWalking", false);
                }
                break;
        }
    }

    bool IsPlayerInRange()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= detectionRange;
    }

    bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackInterval;
    }

    BossAttackSystem.AttackPattern SelectAttackPattern()
    {
        float totalWeight = meleeAttackWeight + areaAttackWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < meleeAttackWeight)
        {
            return BossAttackSystem.AttackPattern.MeleeSwing;
        }
        else
        {
            return BossAttackSystem.AttackPattern.AreaCross;
        }
    }

    public void AdjustAttackWeights(float meleeWeight, float areaWeight)
    {
        meleeAttackWeight = meleeWeight;
        areaAttackWeight = areaWeight;
        Debug.Log($"BossAI: Attack weights adjusted - Melee: {meleeWeight}, Area: {areaWeight}");
    }

    public void ResetAttackCooldown()
    {
        lastAttackTime = Time.time;
        Debug.Log("BossAI: Attack cooldown reset");
    }

    public void StopAI()
    {
        isDead = true;
        TransitionToState(BossState.Idle);
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

}