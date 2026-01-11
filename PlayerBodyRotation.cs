using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerBodyRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private CinemachinePOV pov;
    private bool isGameplayScene = false;

    void Start()
    {
        // Combat 또는 Tutorial 씬인지 확인
        string sceneName = SceneManager.GetActiveScene().name;
        isGameplayScene = (sceneName == "Combat" || sceneName == "Tutorial");

        if (!isGameplayScene)
        {
            Debug.LogWarning("PlayerBodyRotation: Not in gameplay scene. Disabling.");
            enabled = false;
            return;
        }

        // Virtual Camera 자동 찾기
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        // POV 컴포넌트 가져오기
        if (virtualCamera != null)
        {
            pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        if (pov == null)
        {
            Debug.LogError("PlayerBodyRotation: CinemachinePOV not found!");
        }

        Debug.Log($"PlayerBodyRotation: Initialized in {sceneName} scene.");
    }

    void Update()
    {
        if (!isGameplayScene) return;

        // ESC 키로 커서 해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 마우스 클릭으로 다시 잠금 (커서가 이미 잠겨있지 않을 때만)
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (pov == null) return;

        // POV의 수평 회전값을 플레이어 Y축 회전에 적용
        float horizontalAngle = pov.m_HorizontalAxis.Value;
        transform.rotation = Quaternion.Euler(0f, horizontalAngle, 0f);
    }

    void OnDestroy()
    {
        // 스크립트가 파괴될 때 커서 상태 복원
        if (isGameplayScene)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}