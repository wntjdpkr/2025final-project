using UnityEngine;

public class MeleeWeaponCollider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossAttackSystem attackSystem;

    private Collider weaponCollider;

    void Awake()
    {
        // 콜라이더 참조 캐싱
        weaponCollider = GetComponent<Collider>();

        if (weaponCollider == null)
        {
            Debug.LogError("MeleeWeaponCollider: Collider component not found!");
            return;
        }

        // isTrigger 검증
        if (!weaponCollider.isTrigger)
        {
            Debug.LogWarning("MeleeWeaponCollider: Collider is not set as Trigger! Setting it now.");
            weaponCollider.isTrigger = true;
        }

        // 초기 비활성화
        weaponCollider.enabled = false;
    }

    void Start()
    {
        // BossAttackSystem 자동 찾기 (부모 오브젝트에서)
        if (attackSystem == null)
        {
            attackSystem = GetComponentInParent<BossAttackSystem>();
            if (attackSystem == null)
            {
                Debug.LogError("MeleeWeaponCollider: BossAttackSystem not found in parent!");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌했을 때만 처리
        if (other.CompareTag("Player"))
        {
            if (attackSystem != null)
            {
                attackSystem.OnMeleeWeaponHit(other);

#if UNITY_EDITOR
                Debug.Log($"MeleeWeaponCollider: Hit player at {Time.time:F2}s!");
#endif
            }
        }
    }
}