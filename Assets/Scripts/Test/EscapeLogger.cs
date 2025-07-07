using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EscapeLogger : MonoBehaviour
{
    public string logFileName = "EscapeLog";
    public bool isLogEnabled = true;
    private string logPath;
    public EscapeAgentManager escapeAgentManager;
    public float captureInterval = 10.0f; // 시간 간격 (초)
    private float timeAccumulator = 0f;

    void Start()
    {
        if (!isLogEnabled)
        {
            Debug.Log("[Logger] 로깅이 비활성화되어 있습니다.");
            return;
        }
        // 저장 경로 설정
        string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        logPath = Path.Combine("C:\\Users\\mplng\\EvacuationSimulation\\Assets", dateTime + logFileName + ".csv");

        // 파일이 없으면 헤더 추가
        if (!File.Exists(logPath))
        {
            string header = "seed, max_escape_time, avg_escape_time, num_dead(fire), num_dead(smoke), num_exit, total_agents\n";
            File.WriteAllText(logPath, header);
        }
    }

    void Update()
    {
        if (!isLogEnabled)
            return;

        timeAccumulator += Time.deltaTime;
        if (timeAccumulator >= captureInterval)
        {
            LogSimulationResult();
            Debug.Log($"[Logger] 결과 저장 완료: {logPath}");
            timeAccumulator = 0f;
        }
    }

    public void LogSimulationResult()
    {
        if (escapeAgentManager == null)
        {
            Debug.LogError("[Logger] EscapeManager 참조가 없습니다!");
            return;
        }

        int seed = escapeAgentManager.seed;
        float maxTime = escapeAgentManager.maxEscapeTime;
        float avgTime = escapeAgentManager.avgEscapeTime;
        int deadwithFire = escapeAgentManager.agentDeadFireCount;
        int deadwithSmoke = escapeAgentManager.agentDeadSmokeCount;
        int exitCount = escapeAgentManager.agentExitCount;
        int total = escapeAgentManager.nAgent;

        string line = $"{seed},{maxTime},{avgTime},{deadwithFire},{deadwithSmoke},{exitCount},{total}\n";
        File.AppendAllText(logPath, line);
        Debug.Log($"[Logger] 결과 저장 완료: {logPath}");
    }
}
