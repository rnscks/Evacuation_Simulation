using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneObjects : MonoBehaviour
{
    public bool isExist = false;
    public GameObject upPlane;
    public GameObject downPlane;
    public GameObject exit;
    public List<GameObject> neighborPlanes = new List<GameObject>();
    public float cost = 1.0f;
    private List<Node> nodes = new List<Node>();
    public bool isFire = false;
    public bool isSmoke = false;
    public int sectionNumber = 0;
    private int timeStepToStop = 100;
    private int initialTimeStepToStop = 100;

    void Start()
    {

    }
    void Update()
    {

    }

    public Vector3 GetCenterPosition()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Vector3 minCorner = meshRenderer.bounds.min;
        Vector3 maxCorner = meshRenderer.bounds.max;
        Vector3 center = new Vector3((minCorner.x + maxCorner.x) / 2, (minCorner.y + maxCorner.y) / 2, (minCorner.z + maxCorner.z) / 2);
        return center;
    }
    public bool IsOnPlane(Vector3 position)
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Vector3 cornerMax = meshRenderer.bounds.max;
        Vector3 cornerMin = meshRenderer.bounds.min;

        if (position.x < cornerMin.x || position.x > cornerMax.x ||
            position.z < cornerMin.z || position.z > cornerMax.z)
        {
            return false;
        }
        if (position.y < cornerMin.y - 1.0f || position.y > cornerMax.y + 1.0f)
        {
            return false;
        }
        return true;
    }
    public void AddNode(Node node)
    {
        if (!nodes.Contains(node))
        {
            nodes.Add(node);
        }
    }
    public bool IsFire()
    {
        foreach (Node node in nodes)
        {
            if (node.IsBurning() || node.IsBurned())
            {
                isFire = true;
                return true;
            }
        }
        isFire = false;
        return false;
    }
    public bool IsSmoke()
    {
        foreach (Node node in nodes)
        {
            if (node.IsSmoking())
            {
                isSmoke = true;
                return true;
            }
        }
        isSmoke = false;
        return false;
    }

    public float GetTemperature()
    {
        float temperature = 0.0f;
        foreach (Node node in nodes)
        {
            temperature += node.GetTemperature();
        }
        if (nodes.Count > 0)
        {
            temperature /= nodes.Count;
        }
        return temperature;
    }

    public float GetVisibleDistance()
    {
        float visibility = 0.0f;
        foreach (Node node in nodes)
        {
            visibility += node.GetVisibleDistance();
        }
        if (nodes.Count > 0)
        {
            visibility /= nodes.Count;
        }
        return visibility;
    }

    public float GetSmokeDensity()
    {
        float smokeDensity = 0.0f;
        foreach (Node node in nodes)
        {
            smokeDensity += node.GetSmokeDensity();
        }
        if (nodes.Count > 0)
        {
            smokeDensity /= nodes.Count;
        }
        return smokeDensity;
    }


    void OnDrawGizmos()
    {
        if (cost == 0.0f)
        {
            Gizmos.color = Color.red;
        }
        else if (cost == 0.5f)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Vector3 minCorner = meshRenderer.bounds.min;
        Vector3 maxCorner = meshRenderer.bounds.max;

        Vector3 center = new Vector3((minCorner.x + maxCorner.x) / 2, (minCorner.y + maxCorner.y) / 2, (minCorner.z + maxCorner.z) / 2);
        Vector3 size = maxCorner - minCorner;
        size.y = 0.5f;
        Gizmos.DrawCube(center, size);
        Gizmos.DrawSphere(center, 1.0f);
    }

    public void UpdateTimeToStop()
    {
        if (cost == 0.5f)
        {
            timeStepToStop -= 1;
            if (timeStepToStop <= 0)
            {
                cost = 1.0f;
                ResetTimeToStop();
            }
        }
    }
    public void ResetTimeToStop()
    {
        timeStepToStop = initialTimeStepToStop;
    }
}
