using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class SupAgentEntire : Agent
{
    public int maxSectionNumber = 28;
    public EscapeAgentManager escapeAgentManager;
    public List<GameObject> planes = new List<GameObject>();
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
        float[] fireObs = CollectFireObsArray();
        float[] planeCostObs = CollectPlaneCostArray();
        float[] escapeAgentObs = CollectEscapeAgentObsArray();


        sensor.AddObservation(fireObs);
        sensor.AddObservation(planeCostObs);
        sensor.AddObservation(escapeAgentObs);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        List<PlaneObjects> planeObjectsList = new List<PlaneObjects>();
        Debug.Log("OnActionReceived called with actions: " + actions.DiscreteActions);
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            planeObjectsList.Add(planeObject);
        }

        for (int i = 0; i <= maxSectionNumber; i++)
        {
            int action = actions.DiscreteActions[i];

            foreach (PlaneObjects planeObject in planeObjectsList)
            {
                if (planeObject.sectionNumber == i)
                {
                    switch (action)
                    {
                        case 0: // Move left
                            planeObject.cost = 1.0f; // High cost
                            break;
                        case 1: // Move right
                            planeObject.cost = 0.5f; // Medium cost
                            break;
                    }
                }
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

        for (int i = 0; i <= maxSectionNumber; i++)
        {
            float totalArea = 0f;
            float totalTemperature = 0f;
            float totalSmokeDensity = 0f;
            float totalVisibleDistance = 0f;

            foreach (PlaneObjects planeObject in planeObjectsList)
            {
                if (planeObject.sectionNumber == i)
                {
                    MeshRenderer meshRenderer = planeObject.GetComponent<MeshRenderer>();
                    Vector3 minCorner = meshRenderer.bounds.min;
                    Vector3 maxCorner = meshRenderer.bounds.max;
                    float area = (maxCorner.x - minCorner.x) * (maxCorner.z - minCorner.z);
                    totalArea += area;
                    totalTemperature += planeObject.GetTemperature() * area;
                    totalSmokeDensity += planeObject.GetSmokeDensity() * area;
                    totalVisibleDistance += planeObject.GetVisibleDistance() * area;
                }
            }
            float maxTemperature = 100f; // Assuming maximum temperature is 1000 degrees
            float maxSmokeDensity = 2800f; // Assuming maximum smoke density is 1
            float maxVisibleDistance = 30f; // Assuming maximum visible distance is 30 meters
            obsList.Add((totalTemperature / totalArea) / maxTemperature);
            obsList.Add((totalVisibleDistance / totalArea) / maxVisibleDistance);
            obsList.Add((totalSmokeDensity / totalArea) / maxSmokeDensity);
        }
        return obsList.ToArray();
    }

    private float[] CollectPlaneCostArray()
    {
        List<float> obsList = new List<float>();
        List<PlaneObjects> planeObjectsList = new List<PlaneObjects>();
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            planeObjectsList.Add(planeObject);
        }

        for (int i = 0; i <= maxSectionNumber; i++)
        {
            foreach (PlaneObjects planeObject in planeObjectsList)
            {
                if (planeObject.sectionNumber == i)
                {
                    obsList.Add(planeObject.cost);
                    break;
                }
            }
        }

        return obsList.ToArray();
    }

    private float[] CollectEscapeAgentObsArray()
    {
        List<float> obsList = new List<float>();
        List<PlaneObjects> planeObjectsList = new List<PlaneObjects>();
        foreach (GameObject plane in planes)
        {
            PlaneObjects planeObject = plane.GetComponent<PlaneObjects>();
            planeObjectsList.Add(planeObject);
        }
        float[] numberOfAgents = new float[maxSectionNumber + 1];
        float[] xdirection = new float[maxSectionNumber + 1];
        float[] zdirection = new float[maxSectionNumber + 1];
        float[] ydirection = new float[maxSectionNumber + 1];

        List<GameObject> escapeAgents = escapeAgentManager.GetEscapeAgents();
        foreach (GameObject agent in escapeAgents)
        {
            EscapeAgentController controller = agent.GetComponent<EscapeAgentController>();
            int sectionNumber = controller.GetSectionNumber();
            if (sectionNumber < 0)
            {
                continue;
            }
            Vector3 direction = controller.GetNormalizedDirectionToDestination();

            xdirection[sectionNumber] += direction.x;
            zdirection[sectionNumber] += direction.z;
            ydirection[sectionNumber] += direction.y;
            numberOfAgents[sectionNumber] += 1f;
        }

        for (int i = 0; i <= maxSectionNumber; i++)
        {
            float totalArea = 0f;
            float totalAgents = numberOfAgents[i];
            float xDir = xdirection[i];
            float zDir = zdirection[i];
            float yDir = ydirection[i];

            foreach (PlaneObjects planeObject in planeObjectsList)
            {
                if (planeObject.sectionNumber == i)
                {
                    MeshRenderer meshRenderer = planeObject.GetComponent<MeshRenderer>();
                    Vector3 minCorner = meshRenderer.bounds.min;
                    Vector3 maxCorner = meshRenderer.bounds.max;
                    float area = (maxCorner.x - minCorner.x) * (maxCorner.z - minCorner.z);
                    totalArea += area;
                }
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

        }

        return obsList.ToArray();
    }
}
