using UnityEngine;

public class DodgeCounter : MonoBehaviour
{
    [Header("Dodge Statistics")]
    [SerializeField] private int totalDodgeAttempts = 0;
    [SerializeField] private int successfulDodges = 0;
    [SerializeField] private int failedDodges = 0;

    public void RecordDodge(bool success)
    {
        totalDodgeAttempts++;

        if (success)
        {
            successfulDodges++;
            Debug.Log($"DodgeCounter: Successful dodge! Total: {successfulDodges}/{totalDodgeAttempts}");
        }
        else
        {
            failedDodges++;
            Debug.Log($"DodgeCounter: Failed dodge! Total: {failedDodges}/{totalDodgeAttempts}");
        }
    }

    public float GetSuccessRate()
    {
        if (totalDodgeAttempts == 0)
        {
            return 0f;
        }

        float successRate = (float)successfulDodges / totalDodgeAttempts * 100f;
        return successRate;
    }

    public void ResetStats()
    {
        totalDodgeAttempts = 0;
        successfulDodges = 0;
        failedDodges = 0;

        Debug.Log("DodgeCounter: Statistics reset.");
    }

    public int GetTotalAttempts()
    {
        return totalDodgeAttempts;
    }

    public int GetSuccessfulDodges()
    {
        return successfulDodges;
    }

    public int GetFailedDodges()
    {
        return failedDodges;
    }

    public string GetStatsString()
    {
        string stats = $"Dodge Statistics:\n";
        stats += $"Total Attempts: {totalDodgeAttempts}\n";
        stats += $"Successful: {successfulDodges}\n";
        stats += $"Failed: {failedDodges}\n";
        stats += $"Success Rate: {GetSuccessRate():F1}%";

        return stats;
    }

    void OnEnable()
    {
        // 씬 시작 시 통계 초기화 (선택적)
        // ResetStats();
    }
}