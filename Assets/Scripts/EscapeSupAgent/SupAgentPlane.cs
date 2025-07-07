using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class SupAgentPlane : Agent
{
    public enum Cost { High, Medium, Low };
    public Cost cost = Cost.Low;

    public EscapeAgentManager escapeAgentManager;
    public List<SupAgentPlane> neighbors = new List<SupAgentPlane>();
    public List<GameObject> planes = new List<GameObject>();
    public int sectionNumber = 0;
    public bool isRandomHeuristic = false;

    public override void Initialize() { }
    public override void OnEpisodeBegin()
    {
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            planeObject.cost = 1.0f;
        }

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        float[] meObs = CollectMeCostArray();
        float[] fireObs = CollectFireObsArray();
        float[] neighborObs = CollectNeighborObsArray();
        float[] exitObs = CollectEscapeAgentObsArray();

        sensor.AddObservation(meObs);
        sensor.AddObservation(fireObs);
        sensor.AddObservation(exitObs);
        sensor.AddObservation(neighborObs);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        cost = Cost.Low; // Default cost

        switch (action)
        {
            case 0: // Move left
                cost = Cost.Low;
                break;
            case 1: // Move right
                cost = Cost.Medium;
                break;
        }

        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObjects = plane.GetComponent<PlaneObjects>();
            if (cost == Cost.Low)
            {
                planeObjects.cost = 1.0f; // High cost
            }
            else if (cost == Cost.Medium)
            {
                planeObjects.cost = 0.5f; // Medium cost
            }
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (isRandomHeuristic == true)
        {
            int action = UnityEngine.Random.Range(0, 2);
            actionsOut.DiscreteActions.Array[0] = action;
        }
        else
        {
            int action = 0;
            actionsOut.DiscreteActions.Array[0] = action;
        }
    }
    private float[] CollectFireObsArray()
    {
        List<float> obsList = new List<float>();
        List<PlaneObjects> planeObjectsList = new List<PlaneObjects>();
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            planeObjectsList.Add(planeObject);
        }

        float totalArea = 0f;
        float totalTemperature = 0f;
        float totalSmokeDensity = 0f;
        float totalVisibleDistance = 0f;
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            MeshRenderer meshRenderer = planeObject.GetComponent<MeshRenderer>();
            Vector3 minCorner = meshRenderer.bounds.min;
            Vector3 maxCorner = meshRenderer.bounds.max;
            float area = (maxCorner.x - minCorner.x) * (maxCorner.z - minCorner.z);
            totalArea += area;
            totalTemperature += planeObject.GetTemperature() * area;
            totalSmokeDensity += planeObject.GetSmokeDensity() * area;
            totalVisibleDistance += planeObject.GetVisibleDistance() * area;
        }

        float maxTemperature = 100f; // Assuming maximum temperature is 1000 degrees
        float maxSmokeDensity = 2800f; // Assuming maximum smoke density is 1
        float maxVisibleDistance = 30f; // Assuming maximum visible distance is 30 meters
        obsList.Add((totalTemperature / totalArea) / maxTemperature);
        obsList.Add((totalVisibleDistance / totalArea) / maxVisibleDistance);
        obsList.Add((totalSmokeDensity / totalArea) / maxSmokeDensity);

        return obsList.ToArray();
    }
    private float[] CollectNeighborObsArray()
    {
        List<float> obsList = new List<float>();
        foreach (SupAgentPlane neighbor in neighbors)
        {
            obsList.Add(neighbor.cost == Cost.Low ? 1f : (neighbor.cost == Cost.Medium ? 0.5f : 0f));
        }

        while (obsList.Count < 4)
        {
            obsList.Add(0f);
        }

        return obsList.ToArray();
    }
    private float[] CollectMeCostArray()
    {
        float[] obs = new float[] {
            cost == Cost.Low ? 1f : (cost == Cost.Medium ? 0.5f : 0f)
        };
        return obs;
    }
    private float[] CollectEscapeAgentObsArray()
    {
        List<float> obsList = new List<float>();
        int totalAgents = 0;
        float xDir = 0f;
        float zDir = 0f;
        float yDir = 0f;
        List<GameObject> escapeAgents = escapeAgentManager.GetEscapeAgents();
        foreach (GameObject agent in escapeAgents)
        {
            EscapeAgentController controller = agent.GetComponent<EscapeAgentController>();
            int sectionNumber = controller.GetSectionNumber();
            if (this.sectionNumber == sectionNumber)
            {
                continue;
            }
            Vector3 direction = controller.GetNormalizedDirectionToDestination();

            xDir += direction.x;
            zDir += direction.z;
            yDir += direction.y;
            totalAgents += 1;
        }

        float totalArea = 0f;
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            MeshRenderer meshRenderer = planeObject.GetComponent<MeshRenderer>();
            Vector3 minCorner = meshRenderer.bounds.min;
            Vector3 maxCorner = meshRenderer.bounds.max;
            totalArea += (maxCorner.x - minCorner.x) * (maxCorner.z - minCorner.z);
        }

        if (totalArea > 0f)
        {
            obsList.Add(totalAgents / totalArea);
        }
        else
        {
            obsList.Add(0f);
        }
        if (totalAgents > 0)
        {
            obsList.Add(xDir / totalAgents);
            obsList.Add(zDir / totalAgents);
            obsList.Add(yDir / totalAgents);
        }
        else
        {
            obsList.Add(0f);
            obsList.Add(0f);
            obsList.Add(0f);
        }

        return obsList.ToArray();
    }

}
