using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.MLAgents;
using UnityEngine;

public class SupAgentPlaneTrainer : MonoBehaviour
{
    public int timeLimit = 1000;
    public SimpleMultiAgentGroup agentGroup;
    public List<SupAgentPlane> agents = new List<SupAgentPlane>();
    public EscapeAgentManager escapeAgentManager;
    public FireSimulationManager fireSimulationManager;
    public int timeStep = 0;
    private int oldagentExitCount = 0;
    private int oldDeadCount = 0;

    void Awake()
    {
        agentGroup = new SimpleMultiAgentGroup();
        foreach (SupAgentPlane agent in agents)
        {
            agentGroup.RegisterAgent(agent);
        }
    }
    void Update()
    {
        timeStep++;
        float totalReward = 0.0f;
        if (escapeAgentManager.IsAllAgentExit() || timeStep >= timeLimit)
        {
            int agentExitCount = escapeAgentManager.agentExitCount - oldagentExitCount;
            int agentDeadCount = escapeAgentManager.agentDeadFireCount + escapeAgentManager.agentDeadSmokeCount - oldDeadCount;
            oldagentExitCount = escapeAgentManager.agentExitCount;
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
            agentGroup.SetGroupReward(totalReward);
            agentGroup.EndGroupEpisode();
            ResetEpisode();
        }
        else
        {
            totalReward -= 0.1f;
            int agentExitCount = escapeAgentManager.agentExitCount - oldagentExitCount;
            int agentDeadCount = escapeAgentManager.agentDeadFireCount + escapeAgentManager.agentDeadSmokeCount - oldDeadCount;
            oldagentExitCount = escapeAgentManager.agentExitCount;
            oldDeadCount = escapeAgentManager.agentDeadFireCount + escapeAgentManager.agentDeadSmokeCount;
            totalReward += agentExitCount;
            totalReward -= agentDeadCount;
            agentGroup.SetGroupReward(totalReward);
        }
    }

    private void ResetEpisode()
    {
        timeStep = 0;
        oldagentExitCount = 0;
        oldDeadCount = 0;

        escapeAgentManager.Initialize();
        fireSimulationManager.Initialize();

        foreach (SupAgentPlane agent in agents)
        {
            agent.OnEpisodeBegin();
        }
    }
}
