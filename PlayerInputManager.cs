using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    private Vector2 moveInput;

    private void Awake()
    {
        // PlayerMovement 참조 확인
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found!");
        }
    }

    private void Update()
    {
        // 매 프레임 이동 입력 처리
        if (playerMovement != null)
        {
            playerMovement.ExecuteMove(moveInput);
        }
    }

    // Send Messages 방식: Unity Input System이 자동으로 호출
    // 메서드명: "On + 액션명" (예: OnMove, OnDash)
    // 파라미터: InputValue 타입
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log($"Move Input: {moveInput}");
    }

    private void OnDash(InputValue value)
    {
        if (value.isPressed && playerMovement != null)
        {
            Debug.Log("Dash Input");
            playerMovement.ExecuteDash();
        }
    }

    private void OnJump(InputValue value)
    {
        if (value.isPressed && playerMovement != null)
        {
            Debug.Log("Jump Input");
            playerMovement.ExecuteJump();
        }
    }

    private void OnAttack(InputValue value)
    {
        if (value.isPressed && playerMovement != null)
        {
            Debug.Log("Attack Input");
            playerMovement.ExecuteAttack();
        }
    }
}