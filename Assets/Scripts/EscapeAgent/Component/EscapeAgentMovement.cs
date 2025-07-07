using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EscapeAgentMovement : MonoBehaviour
{
    public float speed = 2.5f;
    public EscapeAgentVisionCone visionCone;
    public float remainingDistance = 0.0f;

    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    public Vector3 destination { get => navMeshAgent.destination; }

    private void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent.stoppingDistance = 1.0f;
    }

    private void Update()
    {
        AdjustSpeedBasedOnCrowdDensity();
    }

    public void MoveTo(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
    }

    public bool IsArrived()
    {
        float remainingDistance = Vector3.Distance(transform.position, navMeshAgent.destination);
        this.remainingDistance = remainingDistance;
        if (remainingDistance <= navMeshAgent.stoppingDistance + 0.5f)
        {
            return true;
        }

        return false;
    }

    void AdjustSpeedBasedOnCrowdDensity()
    {
        float densityOfCrowd = visionCone.densityOfCrowd;
        float speedReductionFactor = SpeedReductionModel(densityOfCrowd);
        navMeshAgent.speed = speed * speedReductionFactor;
        EscapeAgentStatus escapeAgentStatus = GetComponent<EscapeAgentStatus>();

        if (escapeAgentStatus.IsSmoking == true)
        {
            float visibleDistance = escapeAgentStatus.visibleDistance;

            float finalSpeed = speed * Math.Min(Mathf.Min(1, visibleDistance / 3.0f), speedReductionFactor);
            finalSpeed = Mathf.Max(finalSpeed, 0.2f);
            navMeshAgent.speed = finalSpeed;
        }


    }
    private float SpeedReductionModel(float density)
    {
        if (density < 1)
            return 1;
        else if (1 < density && density <= 5)
            return 0.0673f * Mathf.Pow(density - 1, 2) - 0.419f * (density - 1) + 1;
        else
            return 0.11f;
    }
}
