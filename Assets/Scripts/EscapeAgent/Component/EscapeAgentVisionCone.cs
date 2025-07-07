using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeAgentVisionCone : MonoBehaviour
{
    public float densityOfCrowd = 0;
    public float viewRadius = 1f;
    public float viewAngle = 120f;
    public int resolution = 100;
    public Color visionColor = Color.green;
    private List<Transform> visibleTargets = new List<Transform>();
    public LayerMask targetMask;

    void Start()
    {
        InvokeRepeating("FindVisibleTargets", 0.2f, 0.2f);
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        Vector3 forward = transform.forward;
        Vector3 position = transform.position;

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target == transform) continue;

            Vector3 dirToTarget = target.position - position;
            float dstToTarget = dirToTarget.magnitude;

            // 시야 범위 내에 있는지 확인
            if (dstToTarget <= viewRadius)
            {
                float angleToTarget = Vector3.Angle(forward, dirToTarget);

                // 시야각 내에 있는지 확인
                if (angleToTarget < viewAngle / 2)
                {
                    // 장애물 체크
                    if (!Physics.Raycast(position, dirToTarget, dstToTarget, ~targetMask))
                        visibleTargets.Add(target);
                }
            }
        }
        float visibleArea = Mathf.Pow(viewRadius, 2) * Mathf.PI * (viewAngle / 360);
        densityOfCrowd = visibleTargets.Count / visibleArea;
    }
}
