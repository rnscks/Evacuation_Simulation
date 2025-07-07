using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;


public class EscapeAgentManager : MonoBehaviour
{
    public List<GameObject> floors = new List<GameObject>();
    public EscapeAgentFactory escapeAgentFactory;
    public int nAgent = 10;
    public int agentDeadFireCount = 0;
    public int agentDeadSmokeCount = 0;
    public int agentExitCount = 0;
    public int seed = 6;
    public float maxEscapeTime = 120f;
    public float avgEscapeTime = 0f;
    public float simlationTime = 0f;
    public bool isRandomInitialize = false;
    public bool captureRandomCondition = false;
    public string conditionJSONFile = "EscapeAgentConfig";
    private float cumulativeEscapeTime = 0f;

    private List<GameObject> agents = new List<GameObject>();
    private List<GameObject> planes = new List<GameObject>();


    void Start()
    {
        foreach (GameObject floor in floors)
        {
            for (int i = 0; i < floor.transform.childCount; i++)
            {
                GameObject child = floor.transform.GetChild(i).gameObject;
                if (child.GetComponent<PlaneObjects>() != null)
                {
                    planes.Add(child);
                }
            }
        }
        Initialize();
    }
    void Update()
    {
        simlationTime += Time.fixedDeltaTime;
        List<GameObject> agentToRemove = new List<GameObject>();
        foreach (GameObject agent in agents)
        {
            PlaneObjects planeObject = agent.GetComponent<EscapeAgentController>().currentPlane.GetComponent<PlaneObjects>();
            EscapeAgentStatus agentStatus = agent.GetComponent<EscapeAgentStatus>();
            if (agentStatus.isDead == true)
            {
                agentToRemove.Add(agent);
                if (agentStatus.HowToDie() == 1) // Agent dies from burning
                {
                    agentDeadFireCount++;
                }
                else if (agentStatus.HowToDie() == 2) // Agent dies from smoke
                {
                    agentDeadSmokeCount++;
                }
                continue;
            }

            if (planeObject.isExist == true)
            {
                agentToRemove.Add(agent);
                agentExitCount++;
                cumulativeEscapeTime += simlationTime;
                avgEscapeTime = cumulativeEscapeTime / agentExitCount;
                maxEscapeTime = Mathf.Max(maxEscapeTime, simlationTime);
                continue;
            }


        }

        foreach (GameObject agent in agentToRemove)
        {
            agents.Remove(agent);
            Destroy(agent);
        }

        if (IsAllAgentExit() == true)
        {
            if (avgEscapeTime > 0)
            {
                avgEscapeTime /= agentExitCount;
            }
            maxEscapeTime = simlationTime;
        }
    }
    public List<GameObject> GetEscapeAgents()
    {
        return agents;
    }

    public bool IsAllAgentExit()
    {
        return agents.Count == 0;
    }
    public void Initialize()
    {
        UnityEngine.Random.InitState(seed);
        ResetAgents();
        if (isRandomInitialize == true)
        {
            RandomInitialize(seed);
        }
        else
        {
            JSONInititalize(conditionJSONFile);
        }
    }

    private void ResetAgents()
    {
        foreach (GameObject agent in agents)
        {
            Destroy(agent);
        }
        agents.Clear();
        agentDeadFireCount = 0;
        agentDeadSmokeCount = 0;
        agentExitCount = 0;
        simlationTime = 0f;
        avgEscapeTime = 0f;
        maxEscapeTime = 0f;
    }
    private void RandomInitialize(int seed)
    {
        EscapeAgentConfig config = new EscapeAgentConfig();
        for (int i = 0; i < nAgent; i++)
        {
            while (true)
            {
                int sampledIndex = Random.Range(0, planes.Count);
                GameObject plane = planes[sampledIndex];
                if (plane.GetComponent<PlaneObjects>().isExist == true)
                {
                    continue;
                }
                config.agents.Add(new EscapeAgent { planeIndex = sampledIndex });
                Vector3 randomPosition = plane.GetComponent<PlaneObjects>().GetCenterPosition();
                GameObject agent = escapeAgentFactory.CreateEscapeAgent(randomPosition, plane);
                agents.Add(agent);
                break;
            }
        }

        if (captureRandomCondition == true)
        {
            SaveEscapeAgentConfig(config, conditionJSONFile);
        }
        return;
    }
    private void JSONInititalize(string jsonFileName)
    {
        EscapeAgentConfig config = LoadEscapeAgentConfig(jsonFileName);
        if (config == null)
        {
            Debug.LogError("Failed to load agent configuration.");
            return;
        }

        foreach (EscapeAgent agent in config.agents)
        {
            if (agent.planeIndex < 0 || agent.planeIndex >= planes.Count)
            {
                Debug.LogError($"Invalid plane index: {agent.planeIndex}");
                continue;
            }
            GameObject plane = planes[agent.planeIndex];
            Vector3 randomPosition = plane.GetComponent<PlaneObjects>().GetCenterPosition();
            GameObject escapeAgent = escapeAgentFactory.CreateEscapeAgent(randomPosition, plane);
            agents.Add(escapeAgent);
        }
    }
    private void SaveEscapeAgentConfig(EscapeAgentConfig config, string fileName)
    {
        string json = JsonUtility.ToJson(config, true);
        string filePath = Path.Combine(Application.dataPath, $"{fileName}.json");
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved agent config to: {filePath}");
    }
    private EscapeAgentConfig LoadEscapeAgentConfig(string fileName)
    {
        string filePath = Path.Combine(Application.dataPath, $"{fileName}.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Agent config not found: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        EscapeAgentConfig config = JsonUtility.FromJson<EscapeAgentConfig>(json);
        Debug.Log($"Loaded agent config from: {filePath}");
        return config;
    }
}
