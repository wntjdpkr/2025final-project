using UnityEngine;
using System.Collections;

public class TutorialAttackTrigger : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject crossDamagePrefab;
    [SerializeField] private Transform attackCenter;
    [SerializeField] private float areaCrossWidth = 2f;
    [SerializeField] private float areaCrossLength = 10f;
    [SerializeField] private float areaCrossDeadZone = 0.9f;
    [SerializeField] private float areaWarningDuration = 1.0f;
    [SerializeField] private float areaDamageDuration = 0.5f;
    [SerializeField] private GameObject crossWarningPrefab;

    [Header("Dodge Tracking")]
    [SerializeField] private DodgeSystem dodgeSystem;
    [SerializeField] private Transform player;

    [Header("Tutorial Settings - MUST BE SET!")]
    [SerializeField] private int totalAttacks = 8;
    [SerializeField] private float attackInterval = 3f;

    private TutorialManager tutorialManager;
    private int currentAttackCount = 0;
    private bool isAttacking = false;
    private bool hasBeenHit = false;
    private bool playerWasHit = false;

    void Awake()
    {
        Debug.Log($"=== TutorialAttackTrigger AWAKE ===");
        Debug.Log($"totalAttacks in Awake: {totalAttacks}");
        Debug.Log($"attackInterval in Awake: {attackInterval}");
    }

    void Start()
    {
        Debug.Log($"=== TutorialAttackTrigger START ===");
        Debug.Log($"totalAttacks in Start: {totalAttacks}");
        Debug.Log($"attackInterval in Start: {attackInterval}");

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Player found and assigned.");
            }
            else
            {
                Debug.LogError("Player not found!");
            }
        }

        if (dodgeSystem == null)
        {
            dodgeSystem = FindObjectOfType<DodgeSystem>();
            if (dodgeSystem != null)
            {
                Debug.Log("DodgeSystem found and assigned.");
            }
            else
            {
                Debug.LogWarning("DodgeSystem not found!");
            }
        }

        if (attackCenter == null)
        {
            attackCenter = transform;
            Debug.Log("AttackCenter set to self.");
        }

        tutorialManager = FindObjectOfType<TutorialManager>();
        if (tutorialManager != null)
        {
            Debug.Log("TutorialManager found and assigned.");
        }

        Debug.Log($"=== INITIALIZATION COMPLETE ===");
        Debug.Log($"FINAL totalAttacks: {totalAttacks}");
        Debug.Log($"FINAL attackInterval: {attackInterval}");
    }

    void OnEnable()
    {
        Debug.Log($"=== TutorialAttackTrigger ENABLED ===");
        Debug.Log($"totalAttacks when enabled: {totalAttacks}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter called. Collider: {other.name}");

        if (hasBeenHit)
        {
            Debug.Log("Already hit. Ignoring.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Collider is Player (main body). Checking for weapon...");
            GameObject playerObj = other.gameObject;
            PlayerWeaponCollider weaponCollider = playerObj.GetComponentInChildren<PlayerWeaponCollider>();

            if (weaponCollider != null)
            {
                Debug.Log("Player main body hit, but has weapon collider. Ignoring main body collision.");
                return;
            }
        }

        PlayerWeaponCollider weapon = other.GetComponent<PlayerWeaponCollider>();
        if (weapon != null)
        {
            Debug.Log("=== WEAPON HIT DETECTED ===");
            hasBeenHit = true;

            Debug.Log($"BEFORE STARTING: totalAttacks = {totalAttacks}");
            Debug.Log($"BEFORE STARTING: currentAttackCount = {currentAttackCount}");
            Debug.Log($"BEFORE STARTING: attackInterval = {attackInterval}");

            StartCoroutine(AttackSequenceCoroutine());
        }
        else
        {
            Debug.Log($"Collider {other.name} is neither Player weapon nor proper trigger.");
        }
    }

    IEnumerator AttackSequenceCoroutine()
    {
        Debug.Log($"=== ATTACK SEQUENCE STARTED ===");
        Debug.Log($"Target: {totalAttacks} attacks");
        Debug.Log($"Interval: {attackInterval} seconds");

        while (currentAttackCount < totalAttacks)
        {
            Debug.Log($"--- Starting attack {currentAttackCount + 1}/{totalAttacks} ---");

            yield return StartCoroutine(ExecuteCrossAttack());

            currentAttackCount++;
            Debug.Log($"=== Attack {currentAttackCount}/{totalAttacks} completed ===");

            if (currentAttackCount < totalAttacks)
            {
                Debug.Log($"Waiting {attackInterval} seconds before next attack...");
                yield return new WaitForSeconds(attackInterval);
            }
        }

        Debug.Log("=== ALL ATTACKS COMPLETED ===");

        if (tutorialManager != null)
        {
            Debug.Log("Notifying TutorialManager...");
            tutorialManager.OnAttackSequenceComplete();
        }
        else
        {
            Debug.LogWarning("TutorialManager is null! Cannot notify.");
        }

        Debug.Log("Deactivating AttackTrigger...");
        gameObject.SetActive(false);
    }

    IEnumerator ExecuteCrossAttack()
    {
        Vector3 crossCenter = attackCenter.position;
        crossCenter.y = 0f;

        float rotationAngle = 0f;

        Debug.Log($"Creating cross attack at {crossCenter} (Y forced to 0)");

        GameObject[] warnings = ShowCrossWarning(crossCenter, rotationAngle);

        bool isPlayerInCrossArea = IsPlayerInCrossArea(player.position, crossCenter, rotationAngle);

        if (dodgeSystem != null && isPlayerInCrossArea)
        {
            dodgeSystem.StartDodgeTracking(BossAttackSystem.AttackPattern.AreaCross, crossCenter, true, rotationAngle);
            Debug.Log("Player in cross area! Dodge tracking started.");
        }
        else if (dodgeSystem != null && !isPlayerInCrossArea)
        {
            dodgeSystem.StartDodgeTracking(BossAttackSystem.AttackPattern.AreaCross, crossCenter, false, rotationAngle);
            Debug.Log("Player outside cross area.");
        }

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackStart(BossAttackSystem.AttackPattern.AreaCross);
        }

        Debug.Log($"Showing warning for {areaWarningDuration} seconds...");
        yield return new WaitForSeconds(areaWarningDuration);

        Debug.Log("Destroying warning objects...");
        foreach (GameObject warning in warnings)
        {
            if (warning != null)
                Destroy(warning);
        }

        Debug.Log("Creating damage effect...");
        GameObject damageEffect = ShowCrossDamage(crossCenter, rotationAngle);

        isAttacking = true;
        playerWasHit = false;

        Debug.Log($"Damage active for {areaDamageDuration} seconds...");

        float elapsed = 0f;
        while (elapsed < areaDamageDuration)
        {
            if (!playerWasHit && IsPlayerInCrossArea(player.position, crossCenter, rotationAngle))
            {
                playerWasHit = true;
                Debug.Log("TutorialAttackTrigger: Player hit detected!");
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;

        Debug.Log("Destroying damage effect...");
        if (damageEffect != null)
            Destroy(damageEffect);

        if (dodgeSystem != null)
            dodgeSystem.EndDodgeTracking(playerWasHit);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackEnd(BossAttackSystem.AttackPattern.AreaCross, playerWasHit);
        }

        Debug.Log($"Cross attack execution complete. Player hit: {playerWasHit}");
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
            return false;
        }

        float halfWidth = areaCrossWidth / 2f;

        bool inUpArea = localPlayerPos.x >= -halfWidth &&
                        localPlayerPos.x <= halfWidth &&
                        localPlayerPos.z >= 0 &&
                        localPlayerPos.z <= areaCrossLength;

        bool inDownArea = localPlayerPos.x >= -halfWidth &&
                          localPlayerPos.x <= halfWidth &&
                          localPlayerPos.z >= -areaCrossLength &&
                          localPlayerPos.z <= 0;

        bool inLeftArea = localPlayerPos.z >= -halfWidth &&
                          localPlayerPos.z <= halfWidth &&
                          localPlayerPos.x >= -areaCrossLength &&
                          localPlayerPos.x <= 0;

        bool inRightArea = localPlayerPos.z >= -halfWidth &&
                           localPlayerPos.z <= halfWidth &&
                           localPlayerPos.x >= 0 &&
                           localPlayerPos.x <= areaCrossLength;

        return inUpArea || inDownArea || inLeftArea || inRightArea;
    }

    GameObject[] ShowCrossWarning(Vector3 center, float rotationAngle)
    {
        GameObject[] warnings = new GameObject[4];

        if (crossWarningPrefab == null)
        {
            Debug.LogError("!!! crossWarningPrefab is NULL !!!");
            return warnings;
        }

        Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);

        float effectiveLength = areaCrossLength - areaCrossDeadZone;
        float centerOffset = areaCrossDeadZone + effectiveLength / 2;

        Debug.Log($"Creating warning prefabs. effectiveLength: {effectiveLength}, centerOffset: {centerOffset}");

        Vector3 upOffset = rotation * (Vector3.forward * centerOffset);
        warnings[0] = Instantiate(crossWarningPrefab, center + upOffset, rotation);
        warnings[0].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveLength);
        Debug.Log($"Up warning created at {center + upOffset}");

        Vector3 downOffset = rotation * (Vector3.back * centerOffset);
        warnings[1] = Instantiate(crossWarningPrefab, center + downOffset, rotation);
        warnings[1].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveLength);
        Debug.Log($"Down warning created at {center + downOffset}");

        Vector3 leftOffset = rotation * (Vector3.left * centerOffset);
        warnings[2] = Instantiate(crossWarningPrefab, center + leftOffset, rotation * Quaternion.Euler(0, 90, 0));
        warnings[2].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveLength);
        Debug.Log($"Left warning created at {center + leftOffset}");

        Vector3 rightOffset = rotation * (Vector3.right * centerOffset);
        warnings[3] = Instantiate(crossWarningPrefab, center + rightOffset, rotation * Quaternion.Euler(0, 90, 0));
        warnings[3].transform.localScale = new Vector3(areaCrossWidth, 0.1f, effectiveLength);
        Debug.Log($"Right warning created at {center + rightOffset}");

        Debug.Log("All 4 warning prefabs created successfully.");
        return warnings;
    }

    GameObject ShowCrossDamage(Vector3 center, float rotationAngle)
    {
        if (crossDamagePrefab == null)
        {
            Debug.LogError("!!! crossDamagePrefab is NULL !!!");
            return null;
        }

        Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);
        GameObject damageEffect = Instantiate(crossDamagePrefab, center, rotation);
        Debug.Log($"Damage effect created at {center}");

        float heightOffset = 0.03f;

        float effectiveLength = areaCrossLength - areaCrossDeadZone;
        float centerOffset = areaCrossDeadZone + effectiveLength / 2;

        Transform upPlane = damageEffect.transform.Find("AwayPlane");
        if (upPlane != null)
        {
            upPlane.localPosition = new Vector3(0, heightOffset, centerOffset);
            upPlane.localScale = new Vector3(areaCrossWidth, effectiveLength, 0.1f);
            Debug.Log("AwayPlane configured.");
        }
        else
        {
            Debug.LogWarning("AwayPlane not found in damage prefab!");
        }

        Transform downPlane = damageEffect.transform.Find("TowardPlane");
        if (downPlane != null)
        {
            downPlane.localPosition = new Vector3(0, heightOffset, -centerOffset);
            downPlane.localScale = new Vector3(areaCrossWidth, effectiveLength, 0.1f);
            Debug.Log("TowardPlane configured.");
        }
        else
        {
            Debug.LogWarning("TowardPlane not found in damage prefab!");
        }

        Transform leftPlane = damageEffect.transform.Find("LeftPlane");
        if (leftPlane != null)
        {
            leftPlane.localPosition = new Vector3(-centerOffset, heightOffset, 0);
            leftPlane.localScale = new Vector3(effectiveLength, areaCrossWidth, 0.1f);
            Debug.Log("LeftPlane configured.");
        }
        else
        {
            Debug.LogWarning("LeftPlane not found in damage prefab!");
        }

        Transform rightPlane = damageEffect.transform.Find("RightPlane");
        if (rightPlane != null)
        {
            rightPlane.localPosition = new Vector3(centerOffset, heightOffset, 0);
            rightPlane.localScale = new Vector3(effectiveLength, areaCrossWidth, 0.1f);
            Debug.Log("RightPlane configured.");
        }
        else
        {
            Debug.LogWarning("RightPlane not found in damage prefab!");
        }

        AreaCrossCollider[] colliders = damageEffect.GetComponentsInChildren<AreaCrossCollider>();
        foreach (AreaCrossCollider collider in colliders)
        {
            Destroy(collider);
        }
        Debug.Log($"Removed {colliders.Length} AreaCrossCollider components.");

        return damageEffect;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        Debug.Log("TutorialAttackTrigger: Disabled and stopped all coroutines");
    }
}