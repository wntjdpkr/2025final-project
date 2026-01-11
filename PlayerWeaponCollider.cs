using UnityEngine;

public class PlayerWeaponCollider : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int weaponDamage = 10;

    private Collider weaponCollider;
    private bool isAttacking = false;
    private bool hasDealtDamage = false;

    private void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider == null)
        {
            Debug.LogError("PlayerWeaponCollider: Collider component not found!");
        }

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    public void EnableWeapon()
    {
        if (weaponCollider != null && weaponCollider.enabled)
        {
            Debug.LogWarning("PlayerWeaponCollider: Weapon already enabled! Ignoring duplicate call.");
            return;
        }

        isAttacking = true;
        hasDealtDamage = false;

        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            Debug.Log("Player weapon enabled");
        }
    }

    public void DisableWeapon()
    {
        if (weaponCollider != null && !weaponCollider.enabled)
        {
            Debug.LogWarning("PlayerWeaponCollider: Weapon already disabled! Ignoring duplicate call.");
            return;
        }

        isAttacking = false;

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            Debug.Log("Player weapon disabled");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAttacking && !hasDealtDamage && other.CompareTag("Boss"))
        {
            BossHealth bossHealth = other.GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(weaponDamage);
                hasDealtDamage = true;
                Debug.Log($"Player hit Boss for {weaponDamage} damage!");
            }
            else
            {
                Debug.LogWarning("Boss does not have BossHealth component!");
            }
        }
    }
}