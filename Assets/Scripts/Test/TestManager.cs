using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public FireSimulationManager fireSimulationManager;
    public EscapeAgentManager escapeAgentManager;
    public EscapeLogger escapeLogger;
    public FireSimulationLogger fireSimulationLogger;
    public List<int> seeds = new List<int>();
    bool isLogEnabled = true;

    void Start()
    {
        int seed = seeds[0];
        seeds.RemoveAt(0);

        fireSimulationManager.seed = seed;
        escapeAgentManager.seed = seed;
    }
    void Update()
    {
        if (escapeAgentManager.IsAllAgentExit() == true && seeds.Count >= 0)
        {
            if (isLogEnabled == true)
            {
                escapeLogger.LogSimulationResult();
            }

            if (seeds.Count > 0) { Reset(); }
            else if (seeds.Count == 0) { isLogEnabled = false; fireSimulationLogger.isLogEnabled = false; }
        }
    }


    void Reset()
    {
        int seed = seeds[0];
        seeds.RemoveAt(0);

        fireSimulationManager.seed = seed;
        escapeAgentManager.seed = seed;
        escapeAgentManager.Initialize();
        fireSimulationManager.Initialize();
    }


}
