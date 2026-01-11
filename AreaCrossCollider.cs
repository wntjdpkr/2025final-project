using UnityEngine;

public class AreaCrossCollider : MonoBehaviour
{
    private BossAttackSystem attackSystem;
    private Collider areaCollider;

    void Awake()
    {
        areaCollider = GetComponent<Collider>();

        if (areaCollider == null)
        {
            Debug.LogError("AreaCrossCollider: Collider component not found!");
            return;
        }

        if (!areaCollider.isTrigger)
        {
            Debug.LogWarning("AreaCrossCollider: Collider is not set as Trigger! Setting it now.");
            areaCollider.isTrigger = true;
        }
    }

    public void SetAttackSystem(BossAttackSystem system)
    {
        attackSystem = system;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (attackSystem != null)
            {
                attackSystem.OnAreaCrossHit(other);

#if UNITY_EDITOR
                Debug.Log($"AreaCrossCollider: Hit player at {Time.time:F2}s!");
#endif
            }
            else
            {
                Debug.LogWarning("AreaCrossCollider: BossAttackSystem reference is null!");
            }
        }
    }
}