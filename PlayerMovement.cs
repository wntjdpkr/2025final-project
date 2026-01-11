using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public enum ActionState
    {
        Idle,
        Moving,
        Dashing,
        Jumping,
        Attacking
    }

    [Header("Components")]
    private Rigidbody rb;
    private Animator animator;

    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float jumpForce = 4f;

    [Header("Action Durations")]
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float attackDuration = 0.5f;

    [Header("State")]
    public ActionState currentActionState = ActionState.Idle;

    [Header("Weapon")]
    [SerializeField] private PlayerWeaponCollider weaponCollider;

    private Vector3 moveDirection;
    private Vector3 jumpDirection;
    private Vector2 currentInput;
    private bool isGrounded = true;
    private float currentSpeedMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (weaponCollider == null)
        {
            weaponCollider = GetComponentInChildren<PlayerWeaponCollider>();
            if (weaponCollider == null)
            {
                Debug.LogWarning("[PlayerMovement] PlayerWeaponCollider not found in children!");
            }
        }

        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
                Debug.Log("[PlayerMovement] Camera found automatically");
            }
            else
            {
                Debug.LogError("[PlayerMovement] Camera not found!");
            }
        }
    }

    private void Start()
    {
        currentActionState = ActionState.Idle;

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Debug.Log($"[PlayerMovement] ★ Initialized - moveSpeed: {moveSpeed}, dashSpeed: {dashSpeed}");
        Debug.Log($"[PlayerMovement] ★ Initial speedMultiplier: {currentSpeedMultiplier}");
    }

    private void Update()
    {
        UpdateSpeedMultiplier();
        UpdateAnimatorSpeed();
    }

    private void UpdateSpeedMultiplier()
    {
        if (SlowMotionManager.Instance != null)
        {
            float newMultiplier = SlowMotionManager.Instance.GetPlayerSpeedMultiplier();
            if (newMultiplier != currentSpeedMultiplier)
            {
                Debug.Log($"[PlayerMovement] ★ Speed multiplier changed: {currentSpeedMultiplier:F2} → {newMultiplier:F2}");
                Debug.Log($"[PlayerMovement] Move speed: {moveSpeed}m/s × {newMultiplier:F2} = {moveSpeed * newMultiplier:F2}m/s");
                Debug.Log($"[PlayerMovement] Dash speed: {dashSpeed}m/s (unaffected by slow motion)");
            }
            currentSpeedMultiplier = newMultiplier;
        }
        else
        {
            currentSpeedMultiplier = 1f;
        }
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator != null)
        {
            if (currentActionState == ActionState.Dashing)
            {
                animator.speed = 1f;
            }
            else
            {
                animator.speed = currentSpeedMultiplier;
            }
        }
    }

    public void ExecuteMove(Vector2 input)
    {
        currentInput = input;

        if (currentActionState == ActionState.Dashing ||
            currentActionState == ActionState.Attacking ||
            currentActionState == ActionState.Jumping)
        {
            return;
        }

        if (cameraTransform != null && input.magnitude > 0.1f)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * input.y + right * input.x).normalized;
            currentActionState = ActionState.Moving;

            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            moveDirection = Vector3.zero;
            currentActionState = ActionState.Idle;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentActionState == ActionState.Dashing ||
            currentActionState == ActionState.Attacking)
        {
            return;
        }

        if (currentActionState == ActionState.Jumping)
        {
            if (jumpDirection.magnitude > 0.1f)
            {
                Vector3 targetVelocity = jumpDirection * moveSpeed * currentSpeedMultiplier;
                rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            }
            return;
        }

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = moveDirection * moveSpeed * currentSpeedMultiplier;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

            // 슬로우 모션 중일 때만 로그 출력
            if (currentSpeedMultiplier < 1f)
            {
                Debug.Log($"[PlayerMovement] Moving with slow motion: speed = {targetVelocity.magnitude:F2}m/s (multiplier: {currentSpeedMultiplier:F2})");
            }
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    public void ExecuteDash()
    {
        if (currentActionState == ActionState.Dashing ||
            currentActionState == ActionState.Attacking)
        {
            return;
        }

        Vector3 dashDirection;

        if (moveDirection.magnitude > 0.1f)
        {
            dashDirection = moveDirection.normalized;
        }
        else if (cameraTransform != null)
        {
            dashDirection = cameraTransform.forward;
            dashDirection.y = 0f;
            dashDirection.Normalize();
        }
        else
        {
            dashDirection = transform.forward;
        }

        if (dashDirection.magnitude < 0.1f)
        {
            return;
        }

        StartCoroutine(DashCoroutine(dashDirection));
    }

    public void ExecuteJump()
    {
        if (currentActionState == ActionState.Dashing ||
            currentActionState == ActionState.Attacking)
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        jumpDirection = moveDirection;

        rb.velocity = new Vector3(rb.velocity.x, jumpForce * currentSpeedMultiplier, rb.velocity.z);
        currentActionState = ActionState.Jumping;
        isGrounded = false;

        Debug.Log($"[PlayerMovement] Jump executed. Direction stored: {jumpDirection}");
    }

    public void ExecuteAttack()
    {
        if (!CanExecuteAction())
        {
            return;
        }

        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator DashCoroutine(Vector3 direction)
    {
        currentActionState = ActionState.Dashing;

        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            // 대시는 슬로우 모션 영향을 받지 않음 (항상 정상 속도)
            rb.velocity = new Vector3(
                direction.x * dashSpeed,
                rb.velocity.y,
                direction.z * dashSpeed
            );

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

        if (!isGrounded)
        {
            currentActionState = ActionState.Jumping;
        }
        else
        {
            currentActionState = ActionState.Idle;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    private IEnumerator AttackCoroutine()
    {
        currentActionState = ActionState.Attacking;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

        Debug.Log($"[PlayerMovement] Attack started at frame {Time.frameCount}, time {Time.time:F2}s");

        yield return new WaitForSeconds(attackDuration);

        currentActionState = ActionState.Idle;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }

        Debug.Log($"[PlayerMovement] Attack completed at frame {Time.frameCount}, time {Time.time:F2}s");
    }

    public void StartPlayerAttackHit()
    {
        if (weaponCollider != null)
        {
            weaponCollider.EnableWeapon();
        }
        Debug.Log($"★ ANIMATION EVENT: StartPlayerAttackHit called at frame {Time.frameCount}, time {Time.time:F2}s");
    }

    public void EndPlayerAttackHit()
    {
        if (weaponCollider != null)
        {
            weaponCollider.DisableWeapon();
        }
        Debug.Log($"★ ANIMATION EVENT: EndPlayerAttackHit called at frame {Time.frameCount}, time {Time.time:F2}s");
    }

    public void OnPlayerAttackComplete()
    {
        Debug.Log($"★ ANIMATION EVENT: OnPlayerAttackComplete called at frame {Time.frameCount}, time {Time.time:F2}s");
    }

    private bool CanExecuteAction()
    {
        if (currentActionState == ActionState.Idle ||
            currentActionState == ActionState.Moving)
        {
            return true;
        }

        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            if (currentActionState == ActionState.Jumping)
            {
                jumpDirection = Vector3.zero;

                if (moveDirection.magnitude > 0.1f)
                {
                    currentActionState = ActionState.Moving;

                    if (animator != null)
                    {
                        animator.SetBool("IsMoving", true);
                    }
                }
                else
                {
                    currentActionState = ActionState.Idle;

                    if (animator != null)
                    {
                        animator.SetBool("IsMoving", false);
                    }
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}