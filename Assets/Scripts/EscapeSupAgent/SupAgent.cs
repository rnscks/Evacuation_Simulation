using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SupAgent : Agent
{
    public int maxSectionNumber = 28;
    public EscapeAgentManager escapeAgentManager;
    public List<GameObject> floors = new List<GameObject>();
    public bool isRandomHeuristic = false;
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
        else
        {
            actionsOut.DiscreteActions.Array[0] = 29;
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
        }

        return obsList.ToArray();
    }
}
