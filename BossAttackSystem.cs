using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAttackSystem : MonoBehaviour
{
    public enum AttackPattern
    {
        MeleeSwing,
        AreaCross
    }

    [Header("References")]
    [SerializeField] private Transform bossTransform;
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private DodgeSystem dodgeSystem;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private AfterimageAttackSystem afterimageAttackSystem;
    [SerializeField] private SlowMotionChargeSystem slowMotionChargeSystem;

    [Header("Melee Attack Settings")]
    [SerializeField] private GameObject meleeWeapon;
    [SerializeField] private Collider meleeWeaponCollider;
    [SerializeField] private float meleeDuration = 0.8f;
    [SerializeField] private float meleeDangerRange = 2f;

    [Header("Area Cross Attack Settings")]
    [SerializeField] private float areaCrossWidth = 2f;
    [SerializeField] private float areaCrossLength = 10f;
    [SerializeField] private float areaCrossDeadZone = 0.9f;
    [SerializeField] private float areaWarningDuration = 1.0f;
    [SerializeField] private float areaDamageDuration = 0.5f;
    [SerializeField] private GameObject crossWarningPrefab;
    [SerializeField] private GameObject crossDamagePrefab;

    [Header("Area Cross Multipliers (Left-Right Only)")]
    [SerializeField] private float areaCrossLeftMultiplier = 1.0f;
    [SerializeField] private float areaCrossRightMultiplier = 1.0f;

    [Header("Common Settings")]
    [SerializeField] private int attackDamage = 1;

    private bool isMeleeAttacking = false;
    private bool hasDealtDamage = false;
    private bool isAreaAttacking = false;
    private bool hasAreaDealtDamage = false;
    private bool playerWasHit = false;

    private List<GameObject> activeWarnings = new List<GameObject>();
    private GameObject activeDamageEffect = null;
    private Coroutine currentAttackCoroutine = null;

    void Start()
    {
        ValidateReferences();

        if (meleeWeaponCollider != null)
            meleeWeaponCollider.enabled = false;
    }

    void ValidateReferences()
    {
        if (bossTransform == null)
            bossTransform = transform;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("BossAttackSystem: Player not found! Ensure Player has 'Player' tag.");
                enabled = false;
                return;
            }
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (dodgeSystem == null)
        {
            dodgeSystem = FindObjectOfType<DodgeSystem>();
            if (dodgeSystem == null)
            {
                Debug.LogWarning("BossAttackSystem: DodgeSystem not found!");
            }
        }

        if (playerHealth == null)
        {
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }

            if (playerHealth == null)
            {
                Debug.LogError("BossAttackSystem: PlayerHealth not found!");
            }
        }

        if (afterimageAttackSystem == null)
        {
            afterimageAttackSystem = GetComponent<AfterimageAttackSystem>();
            if (afterimageAttackSystem == null)
            {
                Debug.LogWarning("BossAttackSystem: AfterimageAttackSystem not found!");
            }
        }

        if (slowMotionChargeSystem == null)
        {
            slowMotionChargeSystem = GetComponent<SlowMotionChargeSystem>();
            if (slowMotionChargeSystem == null)
            {
                Debug.LogWarning("BossAttackSystem: SlowMotionChargeSystem not found!");
            }
        }
    }

    public IEnumerator ExecuteAttack(AttackPattern pattern)
    {
        if (currentAttackCoroutine != null)
        {
            Debug.LogWarning("BossAttackSystem: Attack already in progress, skipping new attack.");
            yield break;
        }

        switch (pattern)
        {
            case AttackPattern.MeleeSwing:
                currentAttackCoroutine = StartCoroutine(MeleeSwingCoroutine());
                yield return currentAttackCoroutine;
                break;
            case AttackPattern.AreaCross:
                currentAttackCoroutine = StartCoroutine(AreaCrossCoroutine());
                yield return currentAttackCoroutine;
                break;
        }

        currentAttackCoroutine = null;
    }

    IEnumerator MeleeSwingCoroutine()
    {
        playerWasHit = false;

        bool isPlayerInDangerZone = IsPlayerInMeleeRange();

        if (dodgeSystem != null && isPlayerInDangerZone)
        {
            dodgeSystem.StartDodgeTracking(AttackPattern.MeleeSwing, bossTransform.position, true);
            Debug.Log($"BossAttackSystem: Player in danger zone! Distance: {Vector3.Distance(bossTransform.position, player.position):F2}");
        }
        else if (dodgeSystem != null && !isPlayerInDangerZone)
        {
            dodgeSystem.StartDodgeTracking(AttackPattern.MeleeSwing, bossTransform.position, false);
            Debug.Log($"BossAttackSystem: Player outside danger zone. Distance: {Vector3.Distance(bossTransform.position, player.position):F2}");
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackStart(AttackPattern.MeleeSwing);
        }
        else
        {
            Debug.LogWarning("BossAttackSystem: Cannot publish attack start - EventBus is null!");
        }

        if (animator != null)
            animator.SetTrigger("MeleeAttack");

        Debug.Log("BossAttackSystem: Melee attack started! Animation events will control weapon collider.");

        yield return new WaitForSeconds(meleeDuration);

        if (dodgeSystem != null)
            dodgeSystem.EndDodgeTracking(playerWasHit);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackEnd(AttackPattern.MeleeSwing, playerWasHit);
        }
        else
        {
            Debug.LogWarning("BossAttackSystem: Cannot publish attack end - EventBus is null!");
        }

        if (isPlayerInDangerZone)
        {
            Debug.Log($"BossAttackSystem: Melee attack ended. Player was hit: {playerWasHit}");
        }
        else
        {
            Debug.Log($"BossAttackSystem: Melee attack ended. Player was outside danger zone (no dodge tracking)");
        }
    }

    IEnumerator AreaCrossCoroutine()
    {
        playerWasHit = false;
        Vector3 crossCenter = bossTransform.position;

        float rotationAngle = bossTransform.eulerAngles.y;
        Debug.Log($"BossAttackSystem: Cross will rotate to match boss direction ({rotationAngle:F1} degrees)");

        GameObject[] warnings = ShowCrossWarning(crossCenter, rotationAngle);

        bool isPlayerInCrossArea = IsPlayerInCrossArea(player.position, crossCenter, rotationAngle);

        if (dodgeSystem != null && isPlayerInCrossArea)
        {
            dodgeSystem.StartDodgeTracking(AttackPattern.AreaCross, crossCenter, true, rotationAngle);
            Debug.Log("BossAttackSystem: Player in cross area! Dodge tracking started.");
        }
        else if (dodgeSystem != null && !isPlayerInCrossArea)
        {
            dodgeSystem.StartDodgeTracking(AttackPattern.AreaCross, crossCenter, false, rotationAngle);
            Debug.Log("BossAttackSystem: Player outside cross area. No dodge tracking.");
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackStart(AttackPattern.AreaCross);
        }
        else
        {
            Debug.LogWarning("BossAttackSystem: Cannot publish attack start - EventBus is null!");
        }

        Debug.Log("BossAttackSystem: Area cross attack started!");

        yield return new WaitForSeconds(areaWarningDuration);

        ClearWarnings();

        if (animator != null)
            animator.SetTrigger("AreaAttack");

        GameObject damageEffect = ShowCrossDamage(crossCenter, rotationAngle);

        isAreaAttacking = true;
        hasAreaDealtDamage = false;

        yield return new WaitForSeconds(areaDamageDuration);

        isAreaAttacking = false;

        ClearDamageEffect();

        if (dodgeSystem != null)
            dodgeSystem.EndDodgeTracking(playerWasHit);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackEnd(AttackPattern.AreaCross, playerWasHit);
        }
        else
        {
            Debug.LogWarning("BossAttackSystem: Cannot publish attack end - EventBus is null!");
        }

        if (isPlayerInCrossArea)
        {
            Debug.Log($"BossAttackSystem: Area cross attack ended. Player was hit: {playerWasHit}");
        }
        else
        {
            Debug.Log("BossAttackSystem: Area cross attack ended. Player was outside cross area (no dodge tracking)");
        }

        if (CombatManager.GetBattleCount() == 2 && !playerWasHit && dodgeSystem != null)
        {
            Vector3 currentDodgeDirection = dodgeSystem.GetDodgeDirection();

            bool shouldTriggerAfterimage = afterimageAttackSystem != null &&
                                          afterimageAttackSystem.ShouldTriggerAfterimageAttack(currentDodgeDirection);

            bool shouldTriggerSlowMotionCharge = slowMotionChargeSystem != null &&
                                                slowMotionChargeSystem.ShouldTriggerSlowMotionCharge(currentDodgeDirection);

            if (shouldTriggerAfterimage)
            {
                Vector3 playerLastPosition = player.position;
                Debug.Log($"BossAttackSystem: Triggering afterimage attack at player position {playerLastPosition}");
                yield return afterimageAttackSystem.ExecuteAfterimageAttack(playerLastPosition);
            }
            else if (shouldTriggerSlowMotionCharge)
            {
                Debug.Log("BossAttackSystem: Triggering slow motion charge attack");
                yield return slowMotionChargeSystem.ExecuteSlowMotionCharge();
            }
        }
    }

    bool IsPlayerInMeleeRange()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(bossTransform.position, player.position);
        return distanceToPlayer <= meleeDangerRange;
    }

    bool IsPlayerInCrossArea(Vector3 playerPos, Vector3 crossCenter, float rotationAngle)
    {
        if (player == null) return false;

        Vector3 localPlayerPos = playerPos - crossCenter;

        if (rotationAngle != 0f)
        {
            Quaternion inverseRotation = Quaternion.Euler(0, -rotationAngle, 0);
            localPlayerPos = inverseRotation * localPlayerPos;
        }

        float distanceFromCenter = Mathf.Sqrt(localPlayerPos.x * localPlayerPos.x + localPlayerPos.z * localPlayerPos.z);
        if (distanceFromCenter < areaCrossDeadZone)
        {
            Debug.Log($"BossAttackSystem: Player is in dead zone (distance: {distanceFromCenter:F2})");
            return false;
        }

        float leftLength = areaCrossLength * areaCrossLeftMultiplier;
        float rightLength = areaCrossLength * areaCrossRightMultiplier;
        float halfWidth = areaCrossWidth / 2f;

        bool inLeftArea = localPlayerPos.z >= -halfWidth &&
                          localPlayerPos.z <= halfWidth &&
                          localPlayerPos.x >= -leftLength &&
                          localPlayerPos.x <= 0;

        bool inRightArea = localPlayerPos.z >= -halfWidth &&
                           localPlayerPos.z <= halfWidth &&
                           localPlayerPos.x >= 0 &&
                           localPlayerPos.x <= rightLength;

        bool inUpArea = localPlayerPos.x >= -halfWidth &&
                        localPlayerPos.x <= halfWidth &&
                        localPlayerPos.z >= 0 &&
                        localPlayerPos.z <= areaCrossLength;

        bool inDownArea = localPlayerPos.x >= -halfWidth &&
                          localPlayerPos.x <= halfWidth &&
                          localPlayerPos.z >= -areaCrossLength &&
                          localPlayerPos.z <= 0;

        bool isInArea = inLeftArea || inRightArea || inUpArea || inDownArea;

        if (isInArea)
        {
            Debug.Log($"BossAttackSystem: Player is in cross area at position {playerPos} (local: {localPlayerPos}, rotation: {rotationAngle:F1}°)");
        }

        return isInArea;
    }

    public void EnableMeleeWeaponCollider()
    {
        if (meleeWeaponCollider != null)
        {
            meleeWeaponCollider.enabled = true;
            isMeleeAttacking = true;
            hasDealtDamage = false;
            Debug.Log("BossAttackSystem: Melee weapon collider ENABLED");
        }
        else
        {
            Debug.LogWarning("BossAttackSystem: Cannot enable weapon collider - meleeWeaponCollider is null!");
        }
    }

    public void DisableMeleeWeaponCollider()
    {
        if (meleeWeaponCollider != null)
        {
            meleeWeaponCollider.enabled = false;
            isMeleeAttacking = false;
            Debug.Log("BossAttackSystem: Melee weapon collider DISABLED");
        }
    }

    public void StartMeleeHit()
    {
        EnableMeleeWeaponCollider();
        Debug.Log($"★ ANIMATION EVENT: StartMeleeHit called at frame {Time.frameCount}, time {Time.time:F2}s");
    }

    public void EndMeleeHit()
    {
        DisableMeleeWeaponCollider();
        Debug.Log($"★ ANIMATION EVENT: EndMeleeHit called at frame {Time.frameCount}, time {Time.time:F2}s");
    }

    public void OnMeleeAttackComplete()
    {
        Debug.Log($"★ ANIMATION EVENT: OnMeleeAttackComplete called at frame {Time.frameCount}, time {Time.time:F2}s");
    }

    public void OnMeleeWeaponHit(Collider other)
    {
        if (isMeleeAttacking && !hasDealtDamage && other.CompareTag("Player"))
        {
            playerWasHit = true;
            DealDamage();
            hasDealtDamage = true;
            Debug.Log("BossAttackSystem: Melee weapon hit player!");
        }
    }

    public void OnAreaCrossHit(Collider other)
    {
        if (isAreaAttacking && !hasAreaDealtDamage && other.CompareTag("Player"))
        {
            playerWasHit = true;
            DealDamage();
            hasAreaDealtDamage = true;
            Debug.Log("BossAttackSystem: Area cross hit player!");
        }
    }

    GameObject[] ShowCrossWarning(Vector3 center, float rotationAngle)
    {
        ClearWarnings();

        GameObject[] warnings = new GameObject[4];

        if (crossWarningPrefab == null)
        {
            Debug.LogWarning("BossAttackSystem: Cross warning prefab not assigned!");
            return warnings;
        }

        float leftLength = areaCrossLength * areaCrossLeftMultiplier;
        float rightLength = areaCrossLength * areaCrossRightMultiplier;

        Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);

        float effectiveUpLength = areaCrossLength - areaCrossDeadZone;
        float effectiveDownLength = areaCrossLength - areaCrossDeadZone;
        float effectiveLeftLength = leftLength - areaCrossDeadZone;
        float effectiveRightLength = rightLength - areaCrossDeadZone;

        float upCenterOffset = areaCrossDeadZone + effectiveUpLength / 2;
        Vector3 upOffset = rotation * (Vector3.forward * upCenterOffset);
        warnings[0] = Instantiate(crossWarningPrefab, center + upOffset, rotation);
        warnings[0].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveUpLength);
        activeWarnings.Add(warnings[0]);

        float downCenterOffset = areaCrossDeadZone + effectiveDownLength / 2;
        Vector3 downOffset = rotation * (Vector3.back * downCenterOffset);
        warnings[1] = Instantiate(crossWarningPrefab, center + downOffset, rotation);
        warnings[1].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveDownLength);
        activeWarnings.Add(warnings[1]);

        float leftCenterOffset = areaCrossDeadZone + effectiveLeftLength / 2;
        Vector3 leftOffset = rotation * (Vector3.left * leftCenterOffset);
        warnings[2] = Instantiate(crossWarningPrefab, center + leftOffset, rotation * Quaternion.Euler(0, 90, 0));
        warnings[2].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveLeftLength);
        activeWarnings.Add(warnings[2]);

        float rightCenterOffset = areaCrossDeadZone + effectiveRightLength / 2;
        Vector3 rightOffset = rotation * (Vector3.right * rightCenterOffset);
        warnings[3] = Instantiate(crossWarningPrefab, center + rightOffset, rotation * Quaternion.Euler(0, 90, 0));
        warnings[3].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveRightLength);
        activeWarnings.Add(warnings[3]);

        Debug.Log($"BossAttackSystem: Created {activeWarnings.Count} warning objects (left/right with multipliers)");

        return warnings;
    }

    GameObject ShowCrossDamage(Vector3 center, float rotationAngle)
    {
        ClearDamageEffect();

        if (crossDamagePrefab == null)
        {
            Debug.LogWarning("BossAttackSystem: Cross damage prefab not assigned!");
            return null;
        }

        Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);
        GameObject damageEffect = Instantiate(crossDamagePrefab, center, rotation);
        activeDamageEffect = damageEffect;

        float leftLength = areaCrossLength * areaCrossLeftMultiplier;
        float rightLength = areaCrossLength * areaCrossRightMultiplier;

        float heightOffset = 0.02f;

        float effectiveUpLength = areaCrossLength - areaCrossDeadZone;
        float effectiveDownLength = areaCrossLength - areaCrossDeadZone;
        float effectiveLeftLength = leftLength - areaCrossDeadZone;
        float effectiveRightLength = rightLength - areaCrossDeadZone;

        Transform upPlane = damageEffect.transform.Find("AwayPlane");
        if (upPlane != null)
        {
            float upCenterOffset = areaCrossDeadZone + effectiveUpLength / 2;
            upPlane.localPosition = new Vector3(0, heightOffset, upCenterOffset);
            upPlane.localScale = new Vector3(areaCrossWidth, effectiveUpLength, 0.1f);
        }

        Transform downPlane = damageEffect.transform.Find("TowardPlane");
        if (downPlane != null)
        {
            float downCenterOffset = areaCrossDeadZone + effectiveDownLength / 2;
            downPlane.localPosition = new Vector3(0, heightOffset, -downCenterOffset);
            downPlane.localScale = new Vector3(areaCrossWidth, effectiveDownLength, 0.1f);
        }

        Transform leftPlane = damageEffect.transform.Find("LeftPlane");
        if (leftPlane != null)
        {
            float leftCenterOffset = areaCrossDeadZone + effectiveLeftLength / 2;
            leftPlane.localPosition = new Vector3(-leftCenterOffset, heightOffset, 0);
            leftPlane.localScale = new Vector3(effectiveLeftLength, areaCrossWidth, 0.1f);
        }

        Transform rightPlane = damageEffect.transform.Find("RightPlane");
        if (rightPlane != null)
        {
            float rightCenterOffset = areaCrossDeadZone + effectiveRightLength / 2;
            rightPlane.localPosition = new Vector3(rightCenterOffset, heightOffset, 0);
            rightPlane.localScale = new Vector3(effectiveRightLength, areaCrossWidth, 0.1f);
        }

        AreaCrossCollider[] colliders = damageEffect.GetComponentsInChildren<AreaCrossCollider>();
        foreach (AreaCrossCollider collider in colliders)
        {
            collider.SetAttackSystem(this);
        }

        Debug.Log("BossAttackSystem: Created damage effect (left/right with multipliers)");

        return damageEffect;
    }

    void ClearWarnings()
    {
        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null)
            {
                Destroy(warning);
            }
        }
        activeWarnings.Clear();
        Debug.Log("BossAttackSystem: Cleared all warning objects");
    }

    void ClearDamageEffect()
    {
        if (activeDamageEffect != null)
        {
            Destroy(activeDamageEffect);
            activeDamageEffect = null;
            Debug.Log("BossAttackSystem: Cleared damage effect");
        }
    }

    void DealDamage()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning("BossAttackSystem: Cannot deal damage, PlayerHealth is null!");
        }
    }

    public void AdjustAreaCrossSize(Vector3 preferredDirection, float multiplier)
    {
        if (preferredDirection == Vector3.left)
            areaCrossLeftMultiplier = multiplier;
        else if (preferredDirection == Vector3.right)
            areaCrossRightMultiplier = multiplier;

        Debug.Log($"BossAttackSystem: Area cross size adjusted. Direction: {preferredDirection}, Multiplier: {multiplier}");
    }
    public IEnumerator ForceExecuteMeleeAttack()
    {
        Debug.Log("BossAttackSystem: Force executing melee attack (bypassing currentAttackCoroutine check)");
        yield return MeleeSwingCoroutine();
    }
    void OnDestroy()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }

        ClearWarnings();
        ClearDamageEffect();

        Debug.Log("BossAttackSystem: Destroyed and cleaned up all objects");
    }

    void OnDisable()
    {
        ClearWarnings();
        ClearDamageEffect();

        Debug.Log("BossAttackSystem: Disabled and cleaned up all objects");
    }
}