using UnityEngine;

public class DodgeSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DodgeCounter dodgeCounter;
    [SerializeField] private Transform player;
    [SerializeField] private Transform boss;

    private bool isTrackingDodge = false;
    private bool isInDangerZone = false;
    private Vector3 attackStartPosition;
    private BossAttackSystem.AttackPattern currentAttackPattern;
    private Vector3 dodgeDirection;
    private Vector3 crossCenter;
    private Vector3 crossRightDirection;
    private Vector3 playerStartPosition;
    private float crossRotationAngle;

    void Start()
    {
        if (dodgeCounter == null)
        {
            dodgeCounter = FindObjectOfType<DodgeCounter>();
            if (dodgeCounter == null)
            {
                Debug.LogWarning("DodgeSystem: DodgeCounter not found!");
            }
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("DodgeSystem: Player not found!");
            }
        }

        if (boss == null)
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj != null)
            {
                boss = bossObj.transform;
            }
            else
            {
                Debug.LogWarning("DodgeSystem: Boss not found!");
            }
        }
    }

    public void StartDodgeTracking(BossAttackSystem.AttackPattern pattern, Vector3 center, bool inDangerZone)
    {
        StartDodgeTracking(pattern, center, inDangerZone, 0f);
    }

    public void StartDodgeTracking(BossAttackSystem.AttackPattern pattern, Vector3 center, bool inDangerZone, float rotationAngle)
    {
        if (player == null) return;

        isTrackingDodge = true;
        isInDangerZone = inDangerZone;
        currentAttackPattern = pattern;
        attackStartPosition = player.position;
        crossCenter = center;
        crossRotationAngle = rotationAngle;

        // 십자 장판 공격인 경우 추가 정보 저장
        if (pattern == BossAttackSystem.AttackPattern.AreaCross)
        {
            playerStartPosition = player.position;
            Debug.Log($"DodgeSystem: 회피 시작 위치 저장 - {playerStartPosition}");
        }

        if (isInDangerZone)
        {
            Debug.Log($"DodgeSystem: Started tracking dodge (IN DANGER ZONE). Pattern: {pattern}, Player position: {attackStartPosition}, Rotation: {rotationAngle:F1}°");
        }
        else
        {
            Debug.Log($"DodgeSystem: Started tracking dodge (OUTSIDE DANGER ZONE - No recording). Pattern: {pattern}");
        }
    }

    public void EndDodgeTracking(bool playerWasHit)
    {
        if (!isTrackingDodge) return;

        if (isInDangerZone)
        {
            bool dodgeSuccess = !playerWasHit;

            // 회피 성공 + 십자 장판인 경우에만 방향 기록
            if (dodgeSuccess && currentAttackPattern == BossAttackSystem.AttackPattern.AreaCross)
            {
                CalculateDodgeDirection();

                // Vector3.zero가 아닐 때만 기록 (앞뒤 회피는 제외)
                if (dodgeDirection != Vector3.zero)
                {
                    CombatSessionDataStore.RecordDodgeDirection(dodgeDirection);
                }
            }

            RecordDodgeAttempt(dodgeSuccess);
            CombatSessionDataStore.RecordAttackPattern(currentAttackPattern, playerWasHit);

            Debug.Log($"DodgeSystem: Ended tracking dodge (RECORDED). Success: {dodgeSuccess}, Pattern: {currentAttackPattern}");
        }
        else
        {
            Debug.Log($"DodgeSystem: Ended tracking dodge (NOT RECORDED - was outside danger zone). Pattern: {currentAttackPattern}");
        }

        isTrackingDodge = false;
        isInDangerZone = false;
        crossRotationAngle = 0f;
    }

    void CalculateDodgeDirection()
    {
        if (player == null) return;

        // 플레이어 이동 벡터 계산
        Vector3 moveVector = player.position - playerStartPosition;
        moveVector.y = 0; // XZ 평면만 고려

        Debug.Log($"DodgeSystem: === 회피 방향 계산 시작 ===");
        Debug.Log($"DodgeSystem: 시작 위치: {playerStartPosition}");
        Debug.Log($"DodgeSystem: 종료 위치: {player.position}");
        Debug.Log($"DodgeSystem: 이동 벡터: {moveVector}");

        // 이동 거리가 너무 작으면 방향 판정 불가
        if (moveVector.magnitude < 0.1f)
        {
            Debug.LogWarning("DodgeSystem: 이동 거리가 너무 작아 방향 판정 불가");
            dodgeDirection = Vector3.zero;
            return;
        }

        // 십자 장판의 회전을 고려한 로컬 좌표계로 변환
        // crossRotationAngle만큼 역회전시켜서 장판 기준 좌표로 변환
        Quaternion inverseRotation = Quaternion.Euler(0, -crossRotationAngle, 0);
        Vector3 localMoveVector = inverseRotation * moveVector;

        Debug.Log($"DodgeSystem: 장판 회전 각도: {crossRotationAngle:F1}°");
        Debug.Log($"DodgeSystem: 로컬 이동 벡터 (장판 기준): {localMoveVector}");

        // 로컬 좌표계에서 좌우 판정
        // X축 양수 = 우측, X축 음수 = 좌측
        float lateralMovement = localMoveVector.x;

        Debug.Log($"DodgeSystem: 좌우 이동 성분 (X): {lateralMovement:F2}");

        // 판정 (절대값 비교로 좌우 이동이 더 큰지 확인)
        if (Mathf.Abs(lateralMovement) < 0.1f)
        {
            // 좌우 이동이 거의 없음 (앞뒤로만 이동)
            Debug.Log("DodgeSystem: ★ 좌우 이동이 거의 없음 (앞뒤 회피) - 기록하지 않음");
            dodgeDirection = Vector3.zero; // 방향 기록 안 함
        }
        else if (lateralMovement > 0)
        {
            dodgeDirection = Vector3.right;
            Debug.Log($"DodgeSystem: ★★★ 회피 방향 - 우측 (좌우 성분: {lateralMovement:F2}) ★★★");
        }
        else
        {
            dodgeDirection = Vector3.left;
            Debug.Log($"DodgeSystem: ★★★ 회피 방향 - 좌측 (좌우 성분: {lateralMovement:F2}) ★★★");
        }
    }

    void RecordDodgeAttempt(bool success)
    {
        if (dodgeCounter != null)
        {
            dodgeCounter.RecordDodge(success);
        }

        CombatSessionDataStore.RecordDodge(success);

        if (CombatSessionEventBus.Instance != null)
        {
            CombatSessionEventBus.Instance.PublishDodgeAttempt(success);
        }
        else
        {
            Debug.LogWarning("DodgeSystem: Cannot publish dodge attempt - EventBus is null!");
        }
    }

    public bool IsTrackingDodge()
    {
        return isTrackingDodge;
    }

    public bool IsInDangerZone()
    {
        return isInDangerZone;
    }

    public BossAttackSystem.AttackPattern GetCurrentPattern()
    {
        return currentAttackPattern;
    }

    public Vector3 GetDodgeDirection()
    {
        return dodgeDirection;
    }
}