using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AfterimageAttackSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private DodgeSystem dodgeSystem;
    [SerializeField] private GameObject bossModelPrefab;

    [Header("Afterimage Settings")]
    [SerializeField] private Material afterimageMaterial;
    [SerializeField] private float afterimageSpacing = 0.75f;
    [SerializeField] private float afterimageAlpha = 0.6f;

    [Header("Circle Attack Settings")]
    [SerializeField] private GameObject circleWarningPrefab;
    [SerializeField] private GameObject circleDamagePrefab;
    [SerializeField] private float circleRadius = 2.0f;
    [SerializeField] private float warningDuration = 1.0f;
    [SerializeField] private float damageDuration = 0.5f;

    [Header("Common Settings")]
    [SerializeField] private int attackDamage = 1;

    private List<GameObject> activeAfterimages = new List<GameObject>();
    private bool isAfterimageAttacking = false;

    void Start()
    {
        ValidateReferences();
    }

    void ValidateReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("AfterimageAttackSystem: Player not found!");
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
                Debug.LogError("AfterimageAttackSystem: PlayerHealth not found!");
            }
        }

        if (dodgeSystem == null)
        {
            dodgeSystem = FindObjectOfType<DodgeSystem>();
            if (dodgeSystem == null)
            {
                Debug.LogWarning("AfterimageAttackSystem: DodgeSystem not found!");
            }
        }

        if (bossModelPrefab == null)
        {
            Debug.LogError("AfterimageAttackSystem: Boss Model Prefab not assigned!");
        }

        if (afterimageMaterial == null)
        {
            Debug.LogWarning("AfterimageAttackSystem: Afterimage Material not assigned!");
        }

        if (circleWarningPrefab == null)
        {
            Debug.LogWarning("AfterimageAttackSystem: Circle Warning Prefab not assigned!");
        }

        if (circleDamagePrefab == null)
        {
            Debug.LogWarning("AfterimageAttackSystem: Circle Damage Prefab not assigned!");
        }
    }

    public bool ShouldTriggerAfterimageAttack(Vector3 currentDodgeDirection)
    {
        Vector3 preferredDirection = CombatSessionDataStore.GetMostFrequentDodgeDirection();

        if (preferredDirection == Vector3.zero)
        {
            Debug.Log("AfterimageAttackSystem: No preferred dodge direction data.");
            return false;
        }

        if (preferredDirection != Vector3.left && preferredDirection != Vector3.right)
        {
            Debug.Log($"AfterimageAttackSystem: Preferred direction {preferredDirection} is not left/right.");
            return false;
        }

        bool isSameDirection = (currentDodgeDirection == preferredDirection);

        if (isSameDirection)
        {
            Debug.Log($"AfterimageAttackSystem: Player dodged in preferred direction ({preferredDirection}). Triggering afterimage attack!");
        }
        else
        {
            Debug.Log($"AfterimageAttackSystem: Player dodged in different direction. Current: {currentDodgeDirection}, Preferred: {preferredDirection}");
        }

        return isSameDirection;
    }

    public IEnumerator ExecuteAfterimageAttack(Vector3 playerLastPosition)
    {
        if (isAfterimageAttacking)
        {
            Debug.LogWarning("AfterimageAttackSystem: Already executing afterimage attack!");
            yield break;
        }

        isAfterimageAttacking = true;

        Debug.Log($"AfterimageAttackSystem: Starting afterimage attack at position {playerLastPosition}");

        Vector3[] afterimagePositions = CalculateAfterimagePositions(playerLastPosition);

        SpawnAfterimages(afterimagePositions);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackStart(BossAttackSystem.AttackPattern.AreaCross);
        }

        yield return StartCoroutine(AfterimageAttackCoroutine(afterimagePositions));

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishBossAttackEnd(BossAttackSystem.AttackPattern.AreaCross, false);
        }

        isAfterimageAttacking = false;

        Debug.Log("AfterimageAttackSystem: Afterimage attack completed.");
    }

    Vector3[] CalculateAfterimagePositions(Vector3 center)
    {
        Vector3[] positions = new Vector3[3];

        float angle = 0f;
        for (int i = 0; i < 3; i++)
        {
            float radian = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(radian) * afterimageSpacing,
                0f,
                Mathf.Sin(radian) * afterimageSpacing
            );
            positions[i] = center + offset;
            positions[i].y = 0f;

            angle += 120f;
        }

        Debug.Log($"AfterimageAttackSystem: Calculated {positions.Length} afterimage positions in triangular formation.");

        return positions;
    }

    void SpawnAfterimages(Vector3[] positions)
    {
        if (bossModelPrefab == null)
        {
            Debug.LogError("AfterimageAttackSystem: Cannot spawn afterimages - Boss Model Prefab is null!");
            return;
        }

        ClearAfterimages();

        Animator sourceBossAnimator = GetComponent<Animator>();
        if (sourceBossAnimator == null)
        {
            Debug.LogWarning("AfterimageAttackSystem: Source boss has no Animator!");
        }

        foreach (Vector3 position in positions)
        {
            GameObject afterimage = Instantiate(bossModelPrefab, position, Quaternion.identity);
            afterimage.name = "BossAfterimage";

            Animator afterimageAnimator = afterimage.GetComponent<Animator>();
            if (afterimageAnimator != null && sourceBossAnimator != null)
            {
                AnimatorStateInfo currentState = sourceBossAnimator.GetCurrentAnimatorStateInfo(0);

                afterimageAnimator.Play(currentState.fullPathHash, 0, currentState.normalizedTime);
                afterimageAnimator.speed = 0f;

                Debug.Log($"AfterimageAttackSystem: Copied animation state - Hash: {currentState.fullPathHash}, Time: {currentState.normalizedTime:F2}");
            }
            else
            {
                if (afterimageAnimator != null)
                {
                    afterimageAnimator.enabled = false;
                    Debug.LogWarning("AfterimageAttackSystem: Afterimage animator disabled (no source animator)");
                }
            }

            ApplyAfterimageEffect(afterimage);

            activeAfterimages.Add(afterimage);
        }

        Debug.Log($"AfterimageAttackSystem: Spawned {activeAfterimages.Count} afterimages with animation state.");
    }

    void ApplyAfterimageEffect(GameObject afterimage)
    {
        Renderer[] renderers = afterimage.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (afterimageMaterial != null)
            {
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = afterimageMaterial;
                }
                renderer.materials = materials;
            }

            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_Alpha", afterimageAlpha);

            if (renderer.material.HasProperty("_Color"))
            {
                Color color = renderer.material.color;
                color.a = afterimageAlpha;
                propBlock.SetColor("_Color", color);
            }

            renderer.SetPropertyBlock(propBlock);
        }

        Debug.Log($"AfterimageAttackSystem: Applied afterimage effect with alpha {afterimageAlpha}");
    }

    IEnumerator AfterimageAttackCoroutine(Vector3[] positions)
    {
        GameObject[] warnings = ShowCircleWarnings(positions);

        if (dodgeSystem != null)
        {
            bool isPlayerInDanger = IsPlayerInAnyCircle(positions);
            dodgeSystem.StartDodgeTracking(BossAttackSystem.AttackPattern.AreaCross, positions[0], isPlayerInDanger);
            Debug.Log($"AfterimageAttackSystem: Dodge tracking started. Player in danger: {isPlayerInDanger}");
        }

        Debug.Log($"AfterimageAttackSystem: Showing warnings for {warningDuration} seconds...");
        yield return new WaitForSeconds(warningDuration);

        foreach (GameObject warning in warnings)
        {
            if (warning != null)
                Destroy(warning);
        }
        Debug.Log("AfterimageAttackSystem: Warnings destroyed.");

        GameObject[] damageEffects = ShowCircleDamageEffects(positions);

        bool playerWasHit = false;

        Debug.Log($"AfterimageAttackSystem: Damage phase active for {damageDuration} seconds...");
        float elapsed = 0f;
        bool damageDealt = false;

        while (elapsed < damageDuration)
        {
            if (!damageDealt && IsPlayerInAnyCircle(positions))
            {
                playerWasHit = true;
                DealDamage();
                damageDealt = true;
                Debug.Log("AfterimageAttackSystem: Player hit by afterimage attack!");
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (GameObject effect in damageEffects)
        {
            if (effect != null)
                Destroy(effect);
        }
        Debug.Log("AfterimageAttackSystem: Damage effects destroyed.");

        if (dodgeSystem != null)
        {
            dodgeSystem.EndDodgeTracking(playerWasHit);
        }

        ClearAfterimages();

        Debug.Log($"AfterimageAttackSystem: Afterimage attack ended. Player was hit: {playerWasHit}");
    }

    GameObject[] ShowCircleWarnings(Vector3[] positions)
    {
        GameObject[] warnings = new GameObject[positions.Length];

        if (circleWarningPrefab == null)
        {
            Debug.LogError("AfterimageAttackSystem: Circle Warning Prefab is null!");
            return warnings;
        }

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 spawnPosition = positions[i];
            spawnPosition.y = 0.01f;

            warnings[i] = Instantiate(circleWarningPrefab, spawnPosition, Quaternion.Euler(90, 0, 0));
            warnings[i].transform.localScale = new Vector3(circleRadius * 2, circleRadius * 2, 1f);
        }

        Debug.Log($"AfterimageAttackSystem: Created {warnings.Length} circle warning prefabs.");

        return warnings;
    }

    GameObject[] ShowCircleDamageEffects(Vector3[] positions)
    {
        GameObject[] effects = new GameObject[positions.Length];

        if (circleDamagePrefab == null)
        {
            Debug.LogError("AfterimageAttackSystem: Circle Damage Prefab is null!");
            return effects;
        }

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 spawnPosition = positions[i];
            spawnPosition.y = 0.01f;

            effects[i] = Instantiate(circleDamagePrefab, spawnPosition, Quaternion.Euler(90, 0, 0));
            effects[i].transform.localScale = new Vector3(circleRadius * 2, circleRadius * 2, 1f);
        }

        Debug.Log($"AfterimageAttackSystem: Created {effects.Length} circle damage effect prefabs.");

        return effects;
    }

    bool IsPlayerInAnyCircle(Vector3[] positions)
    {
        if (player == null) return false;

        Vector3 playerPos = player.position;

        foreach (Vector3 circleCenter in positions)
        {
            float distance = Vector3.Distance(
                new Vector3(playerPos.x, 0f, playerPos.z),
                new Vector3(circleCenter.x, 0f, circleCenter.z)
            );

            if (distance <= circleRadius)
            {
                return true;
            }
        }

        return false;
    }

    void DealDamage()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"AfterimageAttackSystem: Dealt {attackDamage} damage to player.");
        }
        else
        {
            Debug.LogWarning("AfterimageAttackSystem: Cannot deal damage, PlayerHealth is null!");
        }
    }

    void ClearAfterimages()
    {
        foreach (GameObject afterimage in activeAfterimages)
        {
            if (afterimage != null)
            {
                Destroy(afterimage);
            }
        }

        activeAfterimages.Clear();
        Debug.Log("AfterimageAttackSystem: All afterimages cleared.");
    }

    void OnDestroy()
    {
        ClearAfterimages();
    }
}