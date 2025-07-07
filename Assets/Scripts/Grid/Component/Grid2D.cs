using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid2D : MonoBehaviour
{
    public int resolution = 100;
    public int nFirePersistence = 10;
    public float fireStaticProb = 0.1f;
    public float smokeStaticProb = 0.3f;
    public GameObject floor;
    public List<GameObject> exits;
    public List<GameObject> neighborGridObjects;
    private List<Node> gridNodes = new List<Node>();
    private List<Node> validNodes = new List<Node>();


    void Awake()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Vector3 cornerMax = meshRenderer.bounds.max;
        Vector3 cornerMin = meshRenderer.bounds.min;

        float stepX = (cornerMax.x - cornerMin.x) / resolution;
        float stepZ = (cornerMax.z - cornerMin.z) / resolution;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                Vector3 nodeCornerMax = new Vector3(cornerMin.x + stepX * (i + 1), cornerMin.y, cornerMin.z + (j + 1) * stepZ);
                Vector3 nodeCornerMin = new Vector3(cornerMin.x + stepX * i, cornerMin.y, cornerMin.z + j * stepZ);
                Node node = new Node(
                    cornerMin: nodeCornerMin,
                    cornerMax: nodeCornerMax,
                    windVector: new Vector2(1.0f, 0.0f),
                    nFirePersistence: nFirePersistence);
                gridNodes.Add(node);
                node.i = i;
                node.j = j;
            }
        }


        foreach (Node node in gridNodes)
        {
            if (node.IsOnFloor(floor))
            {
                validNodes.Add(node);
            }
        }
        foreach (Node node in validNodes)
        {
            node.neighbors = GetNeighbors(node);
        }
    }
    void Start()
    {
        List<Node> exitNodes = new List<Node>();
        foreach (GameObject exit in exits)
        {
            Node exitNode = GetNodeFromPosition(exit.transform.position);
            if (exitNode != null && !exitNodes.Contains(exitNode)) exitNodes.Add(exitNode);
        }

        foreach (GameObject neighborGridObject in neighborGridObjects)
        {
            Grid2D neighborGrid = neighborGridObject.GetComponent<Grid2D>();
            foreach (Node exitNode in exitNodes)
            {
                Node neighborExitNode = neighborGrid.GetNodeFromPosition(exitNode.GetCenter());
                if (neighborExitNode.GetCenter().y > exitNode.GetCenter().y)
                {
                    exitNode.upNeighbor = neighborExitNode;
                }
                else
                {
                    exitNode.downNeighbor = neighborExitNode;
                }
            }
        }

    }

    public void TransitionFireState()
    {
        foreach (Node node in validNodes)
        {
            node.TransitionFireState(k1: 1.0f, k2: 0.7f, staticProb: fireStaticProb);
        }
    }
    public void TransitionSmokeState()
    {
        foreach (Node node in validNodes)
        {
            node.TransitionSmokeState(k1: 1.7f, k2: 1.0f, staticProb: smokeStaticProb);
        }
    }
    public void UpdateState()
    {
        foreach (Node node in validNodes)
        {
            node.UpdateState();
        }
    }
    public List<Node> GetValidNodes()
    {
        return validNodes;
    }
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), // Right
            new Vector2Int(-1, 0), // Left
            new Vector2Int(0, 1), // Up
            new Vector2Int(0, -1), // Down
            new Vector2Int(1, 1), // Up-Right
            new Vector2Int(-1, 1), // Up-Left
            new Vector2Int(1, -1), // Down-Right
            new Vector2Int(-1, -1) // Down-Left
        };

        foreach (Vector2Int direction in directions)
        {
            int i = direction.x + node.i;
            int j = direction.y + node.j;

            if (i >= 0 && i < resolution && j >= 0 && j < resolution)
            {
                Node neighbor = gridNodes[i * resolution + j];
                if (validNodes.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }
    public Node GetNodeFromPosition(Vector3 position)
    {
        float closestDistance = float.MaxValue;
        Node closestNode = null;
        foreach (Node node in validNodes)
        {
            float distance = Vector3.Distance(node.GetCenter(), position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }
        return closestNode;
    }

    public void Reset()
    {
        foreach (Node node in validNodes)
        {
            node.Reset();
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Node node in validNodes)
        {
            Gizmos.color = Color.green;

            if (node.IsSmoking())
            {
                Gizmos.color = Color.black;
            }
            if (node.IsBurning())
            {
                Gizmos.color = Color.red;
            }
            if (node.IsBurned())
            {
                Gizmos.color = Color.gray;
            }
            if (node.IsBurned() || node.IsBurning() || node.IsSmoking())
            {
                Vector3 size = node.GetCorners().Item2 - node.GetCorners().Item1;
                Gizmos.DrawWireCube(node.GetCenter(), new Vector3(size.x, 0.5f, size.z));
            }
        }
    }
}
