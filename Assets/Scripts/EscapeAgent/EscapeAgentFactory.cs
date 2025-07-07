using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeAgentFactory : MonoBehaviour
{
    [SerializeField] private GameObject escapeAgentPrefab;
    public FireSimulationManager fireSimulationManager;

    void Start() { }

    void Update() { }

    public GameObject CreateEscapeAgent(Vector3 position, GameObject plane)
    {
        GameObject escapeAgent = Instantiate(escapeAgentPrefab, position, Quaternion.identity);
        escapeAgent.GetComponent<EscapeAgentController>().currentPlane = plane;
        escapeAgent.GetComponent<EscapeAgentStatus>().fireSimulationManager = fireSimulationManager;
        return escapeAgent;
    }
}
