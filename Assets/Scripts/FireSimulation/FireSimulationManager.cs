using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using Unity.MLAgents;

public class FireSimulationManager : MonoBehaviour
{

    public List<Grid2D> grids = new List<Grid2D>();
    public List<GameObject> floors = new List<GameObject>();
    public bool isRandomInitialize = false;
    public bool captureRandomCondition = false;
    public int seed = 71;
    public float updateInterval = 12.0f;
    public string conditionJSONFile = "FireSimulationConfig";
    private float lastUpdateTime = 0.0f;
    private float simulationTime = 0.0f;

    void Start()
    {
        Initialize();
        List<GameObject> planes = new List<GameObject>();
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

        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObjects = plane.GetComponent<PlaneObjects>();
            foreach (Grid2D grid in grids)
            {
                foreach (Node node in grid.GetValidNodes())
                {
                    if (planeObjects.IsOnPlane(node.GetCenter()))
                    {
                        planeObjects.AddNode(node);
                        node.cellMaterial = "Concrete";
                    }
                }
            }
        }
    }
    void Update()
    {
        simulationTime += Time.fixedDeltaTime;
        if (simulationTime - lastUpdateTime >= updateInterval)
        {
            SpreadFire();
            SpreadSmoke();
            lastUpdateTime = simulationTime;
        }
    }

    public Node GetNodeFromPosition(Vector3 position)
    {
        Node closetNode = null;
        foreach (Grid2D grid in grids)
        {
            Node node = grid.GetNodeFromPosition(position);
            if (closetNode == null)
            {
                closetNode = node;
                continue;
            }
            if (node != null && Vector3.Distance(node.GetCenter(), position) < Vector3.Distance(closetNode.GetCenter(), position))
            {
                closetNode = node;
            }
        }
        return closetNode;
    }
    public void Initialize()
    {
        UnityEngine.Random.InitState(seed);
        if (isRandomInitialize == true)
        {
            RandomInitialize(seed);
        }
        else
        {
            JSONInititalize(conditionJSONFile);
        }
    }

    private void RandomInitialize(int seed)
    {
        foreach (Grid2D grid in grids)
        {
            grid.Reset();
        }

        int gridIndex = UnityEngine.Random.Range(0, grids.Count);
        Grid2D sampledGrid = grids[gridIndex];
        List<Node> validNodes = sampledGrid.GetValidNodes();
        int nodeIndex = UnityEngine.Random.Range(2, validNodes.Count);

        Node initialNode = validNodes[nodeIndex];
        initialNode.SetInitialIgnite();

        if (captureRandomCondition)
        {
            FireConfig config = new FireConfig();
            FireStartCell startCell = new FireStartCell
            {
                gridIndex = gridIndex,
                nodeIndex = nodeIndex
            };
            config.fireSimulation.startCells.Add(startCell);
            SaveFireConfig(config);
        }
        return;
    }
    private void JSONInititalize(string fileName)
    {
        foreach (Grid2D grid in grids)
        {
            grid.Reset();
        }
        FireConfig config = LoadFireConfig(fileName);
        if (config == null || config.fireSimulation.startCells.Count == 0)
        {
            Debug.LogError("Fire configuration is empty or not loaded correctly.");
            return;
        }
        int gridIndex = config.fireSimulation.startCells[0].gridIndex;
        int nodeIndex = config.fireSimulation.startCells[0].nodeIndex;

        Grid2D targetGrid = grids[gridIndex];
        List<Node> validNodes = targetGrid.GetValidNodes();
        Node targetNode = validNodes[nodeIndex];
        targetNode.SetInitialIgnite();
        return;
    }
    private void SpreadFire()
    {
        foreach (Grid2D grid in grids)
        {
            grid.TransitionFireState();
            grid.UpdateState();
        }
    }
    private void SpreadSmoke()
    {
        foreach (Grid2D grid in grids)
        {
            grid.TransitionSmokeState();
            grid.UpdateState();
        }
    }

    private void SaveFireConfig(FireConfig config)
    {
        string json = JsonUtility.ToJson(config, true);  // pretty print
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(Application.dataPath, $"fire_config_{timestamp}.json");
        File.WriteAllText(filePath, json);
    }

    private FireConfig LoadFireConfig(string fileName)
    {
        fileName = $"{fileName}.json";
        string filePath = Path.Combine(Application.dataPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        FireConfig config = JsonUtility.FromJson<FireConfig>(json);
        return config;
    }
}
