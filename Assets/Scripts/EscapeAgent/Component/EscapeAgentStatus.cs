using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;

public class EscapeAgentStatus : MonoBehaviour
{
    public float smokeHp = 120f;
    public bool isDead = false;
    public bool IsSmoking = false;
    public bool IsBurning = false;
    public float visibleDistance = 30.0f;
    public FireSimulationManager fireSimulationManager;


    void Start()
    {

    }

    void Update()
    {
        Node srcNode = fireSimulationManager.GetNodeFromPosition(transform.position);
        if (isDead == true) return;

        visibleDistance = srcNode.GetVisibleDistance();
        if (srcNode.IsBurning() == true)
        {
            isDead = true;
            IsBurning = true;
        }
        else
        {
            IsBurning = false;
        }

        if (srcNode.IsSmoking() == true)
        {
            IsSmoking = true;
            smokeHp -= Time.deltaTime;
            if (smokeHp <= 0)
            {
                isDead = true;
            }
        }
        else
        {
            IsSmoking = false;
        }
    }

    public int HowToDie()
    {
        if (IsBurning)
        {
            return 1; // Agent dies from burning
        }
        else if (smokeHp <= 0)
        {
            return 2; // Agent dies from smoke
        }
        return 0;
    }
}
