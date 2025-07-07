using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Epllise
{

    public double a;
    public double b;
    public double theta;

    public Vector2 center;

    public Line line;

    public Epllise(Vector2 center, double a, double b, double theta = 0)
    {
        this.center = center;
        this.a = a;
        this.b = b;
        this.theta = theta;
    }

    public Vector2[] GetPoints(int resolution = 50)
    {
        Vector2[] points = new Vector2[resolution];
        float angleRadians = (float)(theta * Mathf.Deg2Rad);

        double deltaTheta = 2 * Mathf.PI / resolution;
        for (int i = 0; i < resolution; i++)
        {
            double t = i * deltaTheta;
            double x = a * Mathf.Cos((float)t);
            double y = b * Mathf.Sin((float)t);

            // 회전 변환
            double rotatedX = x * Mathf.Cos((float)angleRadians) - y * Mathf.Sin((float)angleRadians);
            double rotatedY = x * Mathf.Sin((float)angleRadians) + y * Mathf.Cos((float)angleRadians);
            points[i] = new Vector2((float)(rotatedX + center.x), (float)(rotatedY + center.y));
        }
        return points;
    }

    public Vector2[] IntersectionPoints(Line line)
    {
        Vector2[] points = GetPoints();
        Vector2[] projections = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            projections[i] = line.Projection(points[i]);
        }

        double threshold = 0.005; // 허용 오차
        List<Vector2> intersections = new List<Vector2>();
        for (int i = 0; i < projections.Length; i++)
        {
            for (int j = 0; j < points.Length; j++)
            {
                if (Vector2.Distance(projections[i], points[j]) < threshold)
                {
                    intersections.Add(points[j]);
                }
            }
        }
        return intersections.ToArray();
    }

    public float IntersectionLength(Line line)
    {
        Vector2[] intersections = IntersectionPoints(line);
        if (intersections.Length == 0)
            return 0f;

        Vector2 lineDir = (line.d - center).normalized;
        Vector2[] normalizedIntersections = new Vector2[intersections.Length];
        for (int i = 0; i < intersections.Length; i++)
        {
            normalizedIntersections[i] = (intersections[i] - center).normalized;
        }

        float maxDot = float.NegativeInfinity;
        Vector2 selectedPoint = intersections[0];

        foreach (Vector2 pt in intersections)
        {
            Vector2 toPt = (pt - center).normalized;
            float dot = Vector2.Dot(toPt, lineDir);
            if (dot > maxDot)
            {
                maxDot = dot;
                selectedPoint = pt;
            }
        }

        return selectedPoint.magnitude;
    }

    public float CalculateCombinedProb(float k1 = 1.0f, float k2 = 0.7f)
    {
        float staticProb = 0.1f; // 예시로 0.1 사용

        float segmentLength = IntersectionLength(line);
        return staticProb + segmentLength * k2;
    }


    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     Vector2[] points = GetPoints();
    //     for (int i = 0; i < points.Length - 1; i++)
    //     {
    //         Gizmos.DrawLine(points[i], points[i + 1]);
    //     }

    //     // Draw the center point
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawSphere(center, 0.01f);

    //     // Draw the line if it exists
    //     if (line != null)
    //     {
    //         Gizmos.color = Color.blue;
    //         Vector2[] linePoints = line.GetPoints();
    //         for (int i = 0; i < linePoints.Length - 1; i++)
    //         {
    //             Gizmos.DrawLine(linePoints[i], linePoints[i + 1]);
    //         }

    //         // Draw the intersection points
    //         Vector2[] intersections = IntersectionPoints(line);
    //         Gizmos.color = Color.yellow;

    //         foreach (Vector2 intersection in intersections)
    //         {
    //             Gizmos.DrawSphere(intersection, 0.01f);
    //         }
    //         Debug.Log($"Ellipse: {this}, Line: {line}, Intersections: {intersections.Length} found.");
    //         Debug.Log($"Intersection Length: {IntersectionLength(line)}");
    //         Debug.Log($"Ellipse Center: {center}, a: {a}, b: {b}, theta: {theta} degrees");
    //         Debug.Log($"Test CalculateCombinedProb: {CalculateCombinedProb()}");

    //     }
    // }
}