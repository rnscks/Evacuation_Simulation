using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

public class SupAgent : Agent
{
    public int maxSectionNumber = 28;
    public EscapeAgentManager escapeAgentManager;
    public List<GameObject> floors = new List<GameObject>();
    public bool isRandomHeuristic = false;
    public bool isCustomHeuristic = false;
    private List<GameObject> planes = new List<GameObject>();

    public override void Initialize() { }
    public override void OnEpisodeBegin()
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

        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            planeObject.cost = 1.0f;
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        float[] fireObs = CollectFireObsArray();
        float[] planeCostObs = CollectPlaneCostArray();
        float[] escapeAgentObs = CollectEscapeAgentObsArray();


        sensor.AddObservation(fireObs);
        sensor.AddObservation(planeCostObs);
        sensor.AddObservation(escapeAgentObs);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        int targetSection = actions.DiscreteActions[0];
        if (targetSection > maxSectionNumber)
        {
            return;
        }
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            if (planeObject.sectionNumber == targetSection)
            {
                planeObject.cost = 0.5f;
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (isRandomHeuristic == true)
        {
            int action = UnityEngine.Random.Range(0, 30);
            actionsOut.DiscreteActions.Array[0] = action;
        }
        else if (isCustomHeuristic == true)
        {
            CustomHeuristic(actionsOut);
        }
        else
        {
            actionsOut.DiscreteActions.Array[0] = 29;
        }
    }

    private void CustomHeuristic(in ActionBuffers actionsOut)
    {
        float maxDangerScore = float.MinValue;
        for (int i = 0; i <= maxSectionNumber; i++)
        {
            float cost = CalPlaneCost(i);
            if (cost <= 0.5f) continue;

            float temperature = CalSectionTemperature(i);
            float visibleDistance = CalSectionVisibleDistance(i);
            float smokeDensity = CalSectionSmokeDensity(i);
            float crowdDensity = CalCrowdDensity(i);

            float dangerScore = temperature + (1 - visibleDistance) + smokeDensity + crowdDensity + CalBasicDangerScore(i);
            Debug.Log($"Section {i}: Temperature={temperature}, VisibleDistance={visibleDistance}, SmokeDensity={smokeDensity}, CrowdDensity={crowdDensity}, DangerScore={dangerScore}");
            if (dangerScore < 0.4f) continue;
            if (dangerScore > maxDangerScore)
            {
                maxDangerScore = dangerScore;
                actionsOut.DiscreteActions.Array[0] = i;
            }
        }

        if (maxDangerScore == float.MinValue)
        {
            actionsOut.DiscreteActions.Array[0] = 29; // Default action if no valid section found
        }
    }

    private float[] CollectFireObsArray()
    {
        List<float> obsList = new List<float>();
        for (int i = 0; i <= maxSectionNumber; i++)
        {
            float temperature = CalSectionTemperature(i);
            float visibleDistance = CalSectionVisibleDistance(i);
            float smokeDensity = CalSectionSmokeDensity(i);
            obsList.Add(temperature);
            obsList.Add(visibleDistance);
            obsList.Add(smokeDensity);
        }
        return obsList.ToArray();
    }
    private float[] CollectEscapeAgentObsArray()
    {
        List<float> obsList = new List<float>();
        for (int i = 0; i <= maxSectionNumber; i++)
        {
            float crowdDensity = CalCrowdDensity(i);
            obsList.Add(crowdDensity);
        }

        return obsList.ToArray();
    }
    private float[] CollectPlaneCostArray()
    {
        List<float> obsList = new List<float>();
        for (int i = 0; i <= maxSectionNumber; i++)
        {
            float cost = CalPlaneCost(i);
            obsList.Add(cost);
        }

        return obsList.ToArray();
    }
    private float CalSectionTemperature(int sectionNumber)
    {
        float totalTemperature = 0f;
        float totalArea = 0f;

        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            if (planeObject.sectionNumber == sectionNumber)
            {
                MeshRenderer meshRenderer = planeObject.GetComponent<MeshRenderer>();
                Vector3 minCorner = meshRenderer.bounds.min;
                Vector3 maxCorner = meshRenderer.bounds.max;
                float area = (maxCorner.x - minCorner.x) * (maxCorner.z - minCorner.z);
                totalArea += area;
                totalTemperature += planeObject.GetTemperature() * area;
            }
        }

        float result = totalArea > 0f ? totalTemperature / totalArea : 0f;
        result = result > 0f ? result / 100.0f : 0f;
        return result;
    }
    private float CalSectionVisibleDistance(int sectionNumber)
    {
        float totalVisibleDistance = 0f;
        float totalArea = 0f;

        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            if (planeObject.sectionNumber == sectionNumber)
            {
                MeshRenderer meshRenderer = planeObject.GetComponent<MeshRenderer>();
                Vector3 minCorner = meshRenderer.bounds.min;
                Vector3 maxCorner = meshRenderer.bounds.max;
                float area = (maxCorner.x - minCorner.x) * (maxCorner.z - minCorner.z);
                totalArea += area;
                totalVisibleDistance += planeObject.GetVisibleDistance() * area;
            }
        }

        float result = totalArea > 0f ? totalVisibleDistance / totalArea : 0f;
        result = result > 0f ? result / 30.0f : 0f;
        return result;
    }
    private float CalSectionSmokeDensity(int sectionNumber)
    {
        float totalSmokeDensity = 0f;
        float totalArea = 0f;

        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            if (planeObject.sectionNumber == sectionNumber)
            {
                MeshRenderer meshRenderer = planeObject.GetComponent<MeshRenderer>();
                Vector3 minCorner = meshRenderer.bounds.min;
                Vector3 maxCorner = meshRenderer.bounds.max;
                float area = (maxCorner.x - minCorner.x) * (maxCorner.z - minCorner.z);
                totalArea += area;
                totalSmokeDensity += planeObject.GetSmokeDensity() * area;
            }
        }

        float result = totalArea > 0f ? totalSmokeDensity / totalArea : 0f;
        result = result > 0f ? result / 2800.0f : 0f; // Normalize to a range of 0 to 1 
        return result;
    }
    private float CalCrowdDensity(int sectionNumber)
    {
        float totalAgentCounts = escapeAgentManager.nAgent;
        float agentCounts = 0f;


        foreach (GameObject agent in escapeAgentManager.GetEscapeAgents())
        {
            EscapeAgentController controller = agent.GetComponent<EscapeAgentController>();
            if (controller.GetSectionNumber() == sectionNumber)
            {
                agentCounts++;
            }
        }

        return totalAgentCounts > 0f ? agentCounts / totalAgentCounts : 0f;
    }
    private float CalPlaneCost(int sectionNumber)
    {
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            if (planeObject.sectionNumber == sectionNumber)
            {
                return planeObject.cost;
            }
        }
        return 1.0f;
    }
    private float CalBasicDangerScore(int sectionNumber)
    {
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            if (planeObject.sectionNumber != sectionNumber) continue;
            string floorName = planeObject.gameObject.name;
            Match match = Regex.Match(floorName, @"^F(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int floor))
            {
                return floor * 0.1f;
            }
            Debug.LogWarning($"Failed to parse floor number from {floorName}");
        }

        return 0.0f;
    }
}
