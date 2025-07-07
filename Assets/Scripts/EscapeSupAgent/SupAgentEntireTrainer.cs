using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class SupAgentEntireTrainer : MonoBehaviour
{
    public int timeLimit = 1000;
    public SupAgentEntire agent;
    public EscapeAgentManager escapeAgentManager;
    public FireSimulationManager fireSimulationManager;
    public int timeStep = 0;
    private int oldAgentExitCount = 0;
    private int oldDeadCount = 0;

    void Awake() { }
    void Update()
    {
        timeStep++;
        float totalReward = 0.0f;
        if (escapeAgentManager.IsAllAgentExit() || timeStep >= timeLimit)
        {
            int agentExitCount = escapeAgentManager.agentExitCount - oldAgentExitCount;
            int agentDeadCount = escapeAgentManager.agentDeadFireCount + escapeAgentManager.agentDeadSmokeCount - oldDeadCount;
            oldAgentExitCount = escapeAgentManager.agentExitCount;
            oldDeadCount = escapeAgentManager.agentDeadFireCount + escapeAgentManager.agentDeadSmokeCount;

            if (agentDeadCount == 0)
            {
                totalReward += 5.0f;
            }
            else if (agentExitCount <= agentDeadCount)
            {
                totalReward += -5.0f;
            }

            float rewardByRatio = (float)(agentExitCount - agentDeadCount) / (agentExitCount + agentDeadCount + 1e-5f);
            totalReward += rewardByRatio * 5.0f;
            if (timeStep >= timeLimit)
            {
                totalReward -= 5.0f;
            }
            agent.SetReward(totalReward);
            agent.EndEpisode();
            ResetEpisode();
        }
        else
        {
            totalReward -= 0.1f;
            int agentExitCount = escapeAgentManager.agentExitCount - oldAgentExitCount;
            int agentDeadCount = escapeAgentManager.agentDeadFireCount + escapeAgentManager.agentDeadSmokeCount - oldDeadCount;
            oldAgentExitCount = escapeAgentManager.agentExitCount;
            oldDeadCount = escapeAgentManager.agentDeadFireCount + escapeAgentManager.agentDeadSmokeCount;
            totalReward += agentExitCount;
            totalReward -= agentDeadCount;
            agent.SetReward(totalReward);
        }
    }

    private void ResetEpisode()
    {
        timeStep = 0;
        escapeAgentManager.Initialize();
        fireSimulationManager.Initialize();
        agent.OnEpisodeBegin();
    }
}
