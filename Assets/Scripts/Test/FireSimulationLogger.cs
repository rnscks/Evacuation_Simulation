using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class FireSimulationLogger : MonoBehaviour
{
    public float captureInterval = 10.0f; // 시간 간격 (초)
    public FireSimulationManager fireSimulationManager;
    public bool isLogEnabled = true;
    public string logFileName = "FireSimulationLog";
    private string logPath;
    private int captureStep = 0;
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
            string header = "floor_index,i,j,is_burning,is_smoking,is_burnt,capture_step\n";
            File.WriteAllText(logPath, header);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLogEnabled)
            return;
        timeAccumulator += Time.deltaTime;

        if (timeAccumulator >= captureInterval)
        {
            SaveSimulationResult();
            Debug.Log($"[Logger] 결과 저장 완료: {logPath}");
            timeAccumulator = 0f;
        }

    }

    private void SaveSimulationResult()
    {
        captureStep += 1;
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            foreach (Grid2D grid in fireSimulationManager.grids)
            {
                int floorIndex = fireSimulationManager.grids.IndexOf(grid);
                foreach (Node node in grid.GetValidNodes())
                {
                    int i = node.i;
                    int j = node.j;
                    bool isBurning = node.IsBurning();
                    bool isSmoking = node.IsSmoking();
                    bool isBurnt = node.IsBurned();
                    string line = $"{floorIndex},{i},{j},{isBurning},{isSmoking},{isBurnt},{captureStep}\n";
                    writer.Write(line);
                }
            }
        }
    }
}
