using UnityEngine;

public static class CombatSessionDataStore
{
    // 회피 통계
    public static int totalDodgeAttempts = 0;
    public static int successfulDodges = 0;
    public static int failedDodges = 0;

    // 패턴별 피격 통계
    public static int meleeAttackHits = 0;      // 무기 휘두르기 공격 피격 횟수
    public static int areaAttackHits = 0;       // 장판 공격 피격 횟수
    public static int meleeAttackTotal = 0;     // 무기 휘두르기 공격 총 횟수
    public static int areaAttackTotal = 0;      // 장판 공격 총 횟수

    // 십자 장판 회피 방향 통계 (좌우만)
    public static int areaCrossLeftDodges = 0;  // 플레이어가 왼쪽으로 회피
    public static int areaCrossRightDodges = 0; // 플레이어가 오른쪽으로 회피

    // 전투 결과
    public static int playerDeaths = 0;
    public static int bossDefeatsCount = 0;
    public static float combatDuration = 0f;

    // 피격 통계
    public static int totalDamageTaken = 0;
    public static int totalHits = 0;

    // 회피 데이터 기록
    public static void RecordDodge(bool success)
    {
        totalDodgeAttempts++;

        if (success)
        {
            successfulDodges++;
        }
        else
        {
            failedDodges++;
        }

        Debug.Log($"CombatSessionDataStore: Dodge recorded. Success: {success}. Total: {totalDodgeAttempts}");
    }

    // 패턴별 피격 데이터 기록
    public static void RecordAttackPattern(BossAttackSystem.AttackPattern pattern, bool playerWasHit)
    {
        if (pattern == BossAttackSystem.AttackPattern.MeleeSwing)
        {
            meleeAttackTotal++;
            if (playerWasHit)
            {
                meleeAttackHits++;
            }
            Debug.Log($"CombatSessionDataStore: Melee attack recorded. Hit: {playerWasHit}. Total: {meleeAttackTotal}, Hits: {meleeAttackHits}");
        }
        else if (pattern == BossAttackSystem.AttackPattern.AreaCross)
        {
            areaAttackTotal++;
            if (playerWasHit)
            {
                areaAttackHits++;
            }
            Debug.Log($"CombatSessionDataStore: Area attack recorded. Hit: {playerWasHit}. Total: {areaAttackTotal}, Hits: {areaAttackHits}");
        }
    }

    // 십자 장판 회피 방향 기록 (좌우만)
    public static void RecordDodgeDirection(Vector3 direction)
    {
        Debug.Log($"CombatSessionDataStore: ★★★ RecordDodgeDirection 호출됨 - 방향: {direction} ★★★");
        Debug.Log($"CombatSessionDataStore: Vector3.left와 비교: {direction == Vector3.left}");
        Debug.Log($"CombatSessionDataStore: Vector3.right와 비교: {direction == Vector3.right}");
        Debug.Log($"CombatSessionDataStore: Vector3.zero와 비교: {direction == Vector3.zero}");

        if (direction == Vector3.left)
        {
            areaCrossLeftDodges++;
            Debug.Log($"CombatSessionDataStore: ■■■ 좌측 회피 기록 완료! 현재 좌측 횟수: {areaCrossLeftDodges} ■■■");
        }
        else if (direction == Vector3.right)
        {
            areaCrossRightDodges++;
            Debug.Log($"CombatSessionDataStore: ■■■ 우측 회피 기록 완료! 현재 우측 횟수: {areaCrossRightDodges} ■■■");
        }
        else if (direction == Vector3.zero)
        {
            Debug.LogWarning("CombatSessionDataStore: Vector3.zero가 전달됨 - 기록하지 않음 (앞뒤 회피)");
        }
        else
        {
            Debug.LogWarning($"CombatSessionDataStore: 예상치 못한 방향 벡터 - {direction}");
        }

        Debug.Log($"CombatSessionDataStore: 현재 누적 - 좌측: {areaCrossLeftDodges}, 우측: {areaCrossRightDodges}");
    }

    // 피격 데이터 기록
    public static void RecordDamage(int damage)
    {
        totalDamageTaken += damage;
        totalHits++;
        Debug.Log($"CombatSessionDataStore: Damage recorded. Amount: {damage}. Total damage: {totalDamageTaken}");
    }

    // 전투 종료 기록
    public static void RecordCombatEnd(bool playerWon)
    {
        if (playerWon)
        {
            bossDefeatsCount++;
            Debug.Log("CombatSessionDataStore: Boss defeat recorded.");
        }
        else
        {
            playerDeaths++;
            Debug.Log("CombatSessionDataStore: Player death recorded.");
        }
    }

    // 회피 성공률 계산
    public static float GetSuccessRate()
    {
        if (totalDodgeAttempts == 0) return 0f;
        return (float)successfulDodges / totalDodgeAttempts * 100f;
    }

    // 무기 휘두르기 피격률 계산
    public static float GetMeleeHitRate()
    {
        if (meleeAttackTotal == 0) return 0f;
        return (float)meleeAttackHits / meleeAttackTotal * 100f;
    }

    // 장판 공격 피격률 계산
    public static float GetAreaHitRate()
    {
        if (areaAttackTotal == 0) return 0f;
        return (float)areaAttackHits / areaAttackTotal * 100f;
    }

    // 가장 많이 사용한 회피 방향 반환 (좌우만)
    public static Vector3 GetMostFrequentDodgeDirection()
    {
        int max = Mathf.Max(areaCrossLeftDodges, areaCrossRightDodges);

        if (max == 0)
        {
            Debug.Log("CombatSessionDataStore: No dodge direction data.");
            return Vector3.zero;
        }

        if (max == areaCrossLeftDodges)
        {
            Debug.Log("CombatSessionDataStore: Most frequent dodge direction: Left");
            return Vector3.left;
        }

        Debug.Log("CombatSessionDataStore: Most frequent dodge direction: Right");
        return Vector3.right;
    }

    // 세션 데이터 초기화
    public static void ResetSession()
    {
        totalDodgeAttempts = 0;
        successfulDodges = 0;
        failedDodges = 0;

        meleeAttackHits = 0;
        areaAttackHits = 0;
        meleeAttackTotal = 0;
        areaAttackTotal = 0;

        areaCrossLeftDodges = 0;
        areaCrossRightDodges = 0;

        playerDeaths = 0;
        bossDefeatsCount = 0;
        combatDuration = 0f;

        totalDamageTaken = 0;
        totalHits = 0;

        Debug.Log("CombatSessionDataStore: Session data reset.");
    }

    // 세션 요약 데이터 반환
    public static string GetSessionSummary()
    {
        string summary = "=== Combat Session Summary ===\n";
        summary += $"Total Dodge Attempts: {totalDodgeAttempts}\n";
        summary += $"Successful Dodges: {successfulDodges}\n";
        summary += $"Failed Dodges: {failedDodges}\n";
        summary += $"Dodge Success Rate: {GetSuccessRate():F1}%\n\n";

        summary += $"Melee Attack Hits: {meleeAttackHits} / {meleeAttackTotal}\n";
        summary += $"Melee Hit Rate: {GetMeleeHitRate():F1}%\n";
        summary += $"Area Attack Hits: {areaAttackHits} / {areaAttackTotal}\n";
        summary += $"Area Hit Rate: {GetAreaHitRate():F1}%\n\n";

        summary += $"Area Cross Dodge Directions:\n";
        summary += $"  Left: {areaCrossLeftDodges}\n";
        summary += $"  Right: {areaCrossRightDodges}\n\n";

        summary += $"Total Damage Taken: {totalDamageTaken}\n";
        summary += $"Total Hits: {totalHits}\n";
        summary += $"Player Deaths: {playerDeaths}\n";
        summary += $"Boss Defeats: {bossDefeatsCount}\n";
        summary += $"Combat Duration: {combatDuration:F1}s\n";

        return summary;
    }
}