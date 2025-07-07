using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node
{
    public int i = 0;
    public int j = 0;
    public List<Node> neighbors = new List<Node>();
    public Node upNeighbor;
    public Node downNeighbor;

    private Vector3 cornerMax;
    private Vector3 cornerMin;
    private Vector3 center;

    // 상하 전이에 대한 필드
    public float upToTransitionStaticProb = 0.5f;
    public float downToTransitionStaticProb = 0.3f;

    // 화재 상태에 대한 필드
    public enum FireState { Free, Burning, Burned }
    public FireState fireState = FireState.Free; // 0: free, 1: burning, 2: burned
    public FireState prevFireState = FireState.Free; // 0: free, 1: burning, 2: burned
    public string cellMaterial = "PVC";
    public int nFirePersistence = 10;


    // 연기 상태에 대한 필드
    public enum SmokeState { Free, Smoking }
    public SmokeState smokeState = SmokeState.Free;
    public SmokeState prevSmokeState = SmokeState.Free;
    public Vector2 windVector = Vector2.up;

    public Node(Vector3 cornerMin, Vector3 cornerMax, Vector2 windVector, int nFirePersistence = 10)
    {
        this.cornerMax = cornerMax;
        this.cornerMin = cornerMin;
        this.center = (cornerMax + cornerMin) / 2f;
        this.nFirePersistence = nFirePersistence;
        this.windVector = windVector;
    }
    public Vector3 GetCenter()
    {
        return center;
    }
    public Tuple<Vector3, Vector3> GetCorners()
    {
        return new Tuple<Vector3, Vector3>(cornerMin, cornerMax);
    }
    public bool IsSmoking(bool isPrevState = false)
    {
        if (isPrevState)
            return prevSmokeState == SmokeState.Smoking;
        return smokeState == SmokeState.Smoking;
    }
    public bool IsBurning(bool isPrevState = false)
    {
        if (isPrevState)
            return prevFireState == FireState.Burning;
        return fireState == FireState.Burning;
    }
    public bool IsBurned(bool isPrevState = false)
    {
        if (isPrevState)
            return prevFireState == FireState.Burned;
        return fireState == FireState.Burned;
    }
    public void SetInitialIgnite()
    {
        fireState = FireState.Burning;
        smokeState = SmokeState.Smoking;
    }
    public void SetOnFire()
    {
        if (IsBurning() == false)
            fireState = FireState.Burning;
    }
    public void SetBunred()
    {
        if (IsBurning() == true)
            fireState = FireState.Burned;
    }
    public void SetOnSmoking()
    {
        if (IsSmoking() == false)
            smokeState = SmokeState.Smoking;
    }
    public void UpdateState()
    {
        prevFireState = fireState;
        prevSmokeState = smokeState;
    }

    public void TransitionFireState(float k1 = 1.0f, float k2 = 0.7f, float staticProb = 0.1f)
    {
        if (IsBurning())
        {
            nFirePersistence--;
            if (nFirePersistence <= 0)
            {
                SetBunred();
            }
        }
        else if (IsBurned() == false)
        {
            float maxProb = 0f;
            foreach (var nei in neighbors)
            {
                if (nei.IsBurning(isPrevState: true))
                {
                    if (cellMaterial == "Concrete")
                    {
                        staticProb = 0.0f;
                    }
                    else if (cellMaterial == "PVC")
                    {
                        staticProb = 0.3f;
                    }
                    float prob = CalculateCombinedProb(from: nei, k1: k1, k2: k2, staticProb: staticProb);
                    if (prob > maxProb) maxProb = prob;
                }
            }
            if (maxProb > UnityEngine.Random.value) SetOnFire();

            if (upNeighbor != null && upNeighbor.IsBurning(isPrevState: true))
            {
                if (UnityEngine.Random.value < upToTransitionStaticProb)
                {
                    SetOnFire();
                }
            }

            if (downNeighbor != null && downNeighbor.IsBurning(isPrevState: true))
            {
                if (UnityEngine.Random.value < downToTransitionStaticProb)
                {
                    SetOnFire();
                }
            }
        }
    }
    public void TransitionSmokeState(float k1 = 1.7f, float k2 = 1.0f, float staticProb = 0.3f)
    {
        if (smokeState == SmokeState.Free)
        {
            if (IsBurning())
            {
                smokeState = SmokeState.Smoking;
                return;
            }
            float maxProb = 0f;
            foreach (var nei in neighbors)
            {
                if (nei.IsSmoking(isPrevState: true))
                {
                    float prob = CalculateCombinedProb(from: nei, k1: k1, k2: k2, staticProb: staticProb);
                    if (prob > maxProb) maxProb = prob;
                }
            }

            if (maxProb > UnityEngine.Random.value)
            {
                smokeState = SmokeState.Smoking;
            }

            if (upNeighbor != null && upNeighbor.IsSmoking(isPrevState: true))
            {
                if (UnityEngine.Random.value < upToTransitionStaticProb)
                {
                    smokeState = SmokeState.Smoking;
                }
            }
            if (downNeighbor != null && downNeighbor.IsSmoking(isPrevState: true))
            {
                if (UnityEngine.Random.value < downToTransitionStaticProb)
                {
                    smokeState = SmokeState.Smoking;
                }
            }
        }
    }
    public float CalculateCombinedProb(Node from, float k1 = 1.0f, float k2 = 0.7f, float staticProb = 0.1f)
    {
        Vector2 d = new Vector2(i - from.i, j - from.j);
        float windNorm = windVector.magnitude;
        if (windNorm == 0)
            return staticProb;

        float a = k1 * windNorm;
        float b = 0.5f * k2 * windNorm;
        float theta = Mathf.Atan2(windVector.y, windVector.x);
        float xc = Mathf.Cos(theta) * Mathf.Sqrt(a * a - b * b);
        float yc = Mathf.Sin(theta) * Mathf.Sqrt(a * a - b * b);
        Epllise ellipse = new Epllise(new Vector2(xc, yc), a, b, theta * Mathf.Rad2Deg);
        Line line = new Line(Vector2.zero, d);
        float segmentLength = ellipse.IntersectionLength(line);
        return staticProb + segmentLength * k2;
    }
    public bool IsOnFloor(GameObject floor, float rayLength = 500.0f)
    {
        MeshCollider[] colliders = floor.GetComponentsInChildren<MeshCollider>();

        foreach (MeshCollider collider in colliders)
        {
            Ray ray = new Ray(center, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayLength))
            {
                if (hit.collider == collider)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Reset()
    {
        fireState = FireState.Free;
        smokeState = SmokeState.Free;
        prevFireState = FireState.Free;
        prevSmokeState = SmokeState.Free;
        nFirePersistence = 10; // 초기화 시 화재 지속 시간 재설정
    }

    public float GetTemperature()
    {
        float wTotal = 0.0f;
        float maxTemperature = 100.0f;

        if (IsBurning())
        {
            wTotal += 0.2f;
        }

        foreach (Node nei in neighbors)
        {
            if (nei.IsBurning())
            {
                wTotal += 0.1f;
            }
        }

        return wTotal * maxTemperature;
    }

    public float GetVisibleDistance()
    {
        float wTotal = 0.0f;
        float maxVisibleDistance = 30.0f;

        if (IsSmoking())
        {
            wTotal += 0.2f;
        }

        foreach (Node nei in neighbors)
        {
            if (nei.IsSmoking())
            {
                wTotal += 0.1f;
            }
        }
        maxVisibleDistance -= wTotal * 30.0f;
        return maxVisibleDistance;
    }

    public float GetSmokeDensity()
    {
        float wTotal = 0.0f;
        float maxSmokeDensity = 2800.0f;

        if (IsSmoking())
        {
            wTotal += 0.2f;
        }

        foreach (Node nei in neighbors)
        {
            if (nei.IsSmoking())
            {
                wTotal += 0.1f;
            }
        }
        return wTotal * maxSmokeDensity;
    }
}
