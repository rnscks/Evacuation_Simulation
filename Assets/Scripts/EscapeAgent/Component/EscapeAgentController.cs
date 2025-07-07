using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeAgentController : MonoBehaviour
{
    public GameObject currentPlane;
    public GameObject nextPlane = null;
    public bool isArrival = false;
    private EscapeAgentMovement escapeAgentMovement;

    private void Awake()
    {
        escapeAgentMovement = GetComponent<EscapeAgentMovement>();
    }
    // Update is called once per frame
    void Update()
    {
        isArrival = escapeAgentMovement.IsArrived();
        nextPlane = GetNextPlane();
        MoveToPlane(nextPlane);

        if (escapeAgentMovement.IsArrived())
        {
            currentPlane = nextPlane;
        }

    }

    private GameObject GetNextPlane()
    {
        List<GameObject> neighbors = currentPlane.GetComponent<PlaneObjects>().neighborPlanes;
        GameObject downPlane = currentPlane.GetComponent<PlaneObjects>().downPlane;
        GameObject upPlane = currentPlane.GetComponent<PlaneObjects>().upPlane;

        List<GameObject> allNeighbors = new List<GameObject>(neighbors);
        if (downPlane != null)
        {
            allNeighbors.Add(downPlane);
        }
        if (upPlane != null)
        {
            allNeighbors.Add(upPlane);
        }

        foreach (GameObject neighbor in allNeighbors)
        {
            if (neighbor == currentPlane)
            {
                Debug.LogError(neighbor.name + currentPlane.name + "CurrentNode is Neighbor");
            }
        }

        GameObject nextPlane = null;
        foreach (GameObject neighbor in allNeighbors)
        {
            if (nextPlane == null)
            {
                nextPlane = neighbor;
                continue;
            }

            PlaneObjects neighborPlaneObject = neighbor.GetComponent<PlaneObjects>();
            if (neighborPlaneObject.isExist == true)
            {
                nextPlane = neighbor;
                return nextPlane;
            }

            PlaneObjects nextPlaneObject = nextPlane.GetComponent<PlaneObjects>();
            if (neighborPlaneObject.IsFire() == false &&
                nextPlaneObject.IsFire() == true)
            {
                nextPlane = neighbor;
            }
            else if (neighborPlaneObject.IsFire() == true &&
                     nextPlaneObject.IsFire() == false)
            {
                continue;
            }
            else if (neighborPlaneObject.IsSmoke() == true &&
                     nextPlaneObject.IsSmoke() == false)
            {
                continue;
            }
            else if (neighborPlaneObject.IsSmoke() == false &&
                     nextPlaneObject.IsSmoke() == true)
            {
                nextPlane = neighbor;
            }
            else if (nextPlaneObject.cost < neighborPlaneObject.cost)
            {
                nextPlane = neighbor;
            }
            else if (nextPlaneObject.cost == neighborPlaneObject.cost)
            {
                // float randomDecider = Random.Range(0.0f, 1.0f);
                Vector3 directionToNextNode = (nextPlaneObject.GetCenterPosition() - transform.position).normalized;
                Vector3 directionToNeighbor = (neighborPlaneObject.GetCenterPosition() - transform.position).normalized;
                Vector3 destination = escapeAgentMovement.destination;
                Vector3 directionToDestination = (destination - transform.position).normalized;

                if (Vector3.Dot(directionToNextNode, directionToDestination) < Vector3.Dot(directionToNeighbor, directionToDestination))
                {
                    nextPlane = neighbor;
                }
            }

        }
        return nextPlane;
    }

    private void MoveToPlane(GameObject dstPlane)
    {
        Vector3 srcPoint = dstPlane.GetComponent<PlaneObjects>().GetCenterPosition();
        escapeAgentMovement.MoveTo(srcPoint);
    }
    public void SetCurrentPlane(GameObject plane) => currentPlane = plane;
    public GameObject GetCurrentPlane() => currentPlane;

    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }

    public Vector3 GetDestinationPosition()
    {
        return escapeAgentMovement.destination;
    }

    public int GetSectionNumber()
    {
        return currentPlane.GetComponent<PlaneObjects>().sectionNumber;
    }

    public Vector3 GetNormalizedDirectionToDestination()
    {
        Vector3 direction = escapeAgentMovement.destination - transform.position;
        return direction.normalized;
    }
}
