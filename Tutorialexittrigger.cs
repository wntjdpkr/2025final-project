using UnityEngine;

public class TutorialExitTrigger : MonoBehaviour
{
    private TutorialManager tutorialManager;
    private bool hasBeenHit = false;

    void Start()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();

        if (tutorialManager == null)
        {
            Debug.LogError("TutorialExitTrigger: TutorialManager not found!");
        }

        // Collider 확인
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            Debug.Log($"TutorialExitTrigger: Collider setup - isTrigger: {col.isTrigger}");
        }
        else
        {
            Debug.LogError("TutorialExitTrigger: No collider found!");
        }

        Debug.Log("TutorialExitTrigger: Ready. Attack this object to proceed to Combat.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TutorialExitTrigger: OnTriggerEnter called. GameObject: {other.gameObject.name}, Tag: {other.tag}");

        if (hasBeenHit)
        {
            Debug.Log("TutorialExitTrigger: Already hit. Ignoring.");
            return;
        }

        // 무기 콜라이더 체크
        PlayerWeaponCollider weapon = other.GetComponent<PlayerWeaponCollider>();
        if (weapon != null)
        {
            Debug.Log("=== TutorialExitTrigger: WEAPON HIT DETECTED ===");
            hasBeenHit = true;

            if (tutorialManager != null)
            {
                Debug.Log("TutorialExitTrigger: Notifying TutorialManager to proceed to Combat...");
                tutorialManager.OnTutorialComplete();
            }
            else
            {
                Debug.LogWarning("TutorialExitTrigger: TutorialManager is null, loading Combat directly.");
                SceneTransitionManager.LoadCombat();
            }
            return;
        }

        // 플레이어 본체와의 충돌은 무시 (무기만 감지)
        if (other.CompareTag("Player"))
        {
            Debug.Log("TutorialExitTrigger: Player body collision detected but ignored (weapon only).");
            return;
        }

        Debug.Log($"TutorialExitTrigger: Unrecognized collider - {other.gameObject.name}");
    }
}