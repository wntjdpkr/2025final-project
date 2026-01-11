using UnityEngine;

public class SlowMotionManager : MonoBehaviour
{
    private static SlowMotionManager instance;

    public static SlowMotionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerObject = new GameObject("SlowMotionManager");
                instance = managerObject.AddComponent<SlowMotionManager>();

                // ★ 중요: 런타임 생성이므로 명시적으로 값 설정 필수!
                // Inspector에서 설정한 값은 런타임 생성 시 적용되지 않음
                instance.slowMotionFactor = 0.1f;

                DontDestroyOnLoad(managerObject);
                Debug.Log("SlowMotionManager: Singleton instance created.");
                Debug.Log($"SlowMotionManager: ★★★ slowMotionFactor FORCEFULLY SET to {instance.slowMotionFactor} ★★★");
            }
            return instance;
        }
    }

    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionFactor = 0.1f;

    private bool isSlowMotionActive = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SlowMotionManager: Instance initialized in Awake.");
            Debug.Log($"SlowMotionManager: Current slowMotionFactor = {slowMotionFactor}");
        }
        else if (instance != this)
        {
            Debug.Log("SlowMotionManager: Duplicate instance destroyed.");
            Destroy(gameObject);
        }
    }

    public void ActivateSlowMotion()
    {
        if (isSlowMotionActive)
        {
            Debug.LogWarning("SlowMotionManager: Slow motion already active!");
            return;
        }

        isSlowMotionActive = true;
        Debug.Log($"SlowMotionManager: ★★★ SLOW MOTION ACTIVATED ★★★");
        Debug.Log($"SlowMotionManager: Factor = {slowMotionFactor} (Player speed will be {slowMotionFactor * 100}%)");
        Debug.Log($"SlowMotionManager: Expected player move speed = {5f * slowMotionFactor}m/s (from 5m/s)");
    }

    public void DeactivateSlowMotion()
    {
        if (!isSlowMotionActive)
        {
            Debug.LogWarning("SlowMotionManager: Slow motion already inactive!");
            return;
        }

        isSlowMotionActive = false;
        Debug.Log("SlowMotionManager: ★★★ SLOW MOTION DEACTIVATED ★★★");
    }

    public float GetPlayerSpeedMultiplier()
    {
        float multiplier = isSlowMotionActive ? slowMotionFactor : 1f;

        // 매 프레임마다는 로그하지 않지만, 중요한 순간에 확인용
        if (Time.frameCount % 60 == 0 && isSlowMotionActive)
        {
            Debug.Log($"SlowMotionManager: GetPlayerSpeedMultiplier() returning {multiplier} (isActive: {isSlowMotionActive})");
        }

        return multiplier;
    }

    public bool IsSlowMotionActive()
    {
        return isSlowMotionActive;
    }

    public void SetSlowMotionFactor(float factor)
    {
        slowMotionFactor = Mathf.Clamp(factor, 0.1f, 1f);
        Debug.Log($"SlowMotionManager: Slow motion factor set to {slowMotionFactor}");
    }
}