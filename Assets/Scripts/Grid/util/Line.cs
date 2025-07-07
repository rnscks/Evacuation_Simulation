using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line
{
    public Vector2 p;
    public Vector2 d;

    public Line(Vector2 p, Vector2 d)
    {
        this.p = p;
        this.d = d.normalized; // 방향 벡터를 정규화
    }
    public Vector2[] GetPoints(
        int resolution = 50,
        double lb = -2,
        double ub = 2)
    {
        Vector2[] points = new Vector2[resolution];
        double deltaT = (ub - lb) / resolution;
        for (int i = 0; i < resolution; i++)
        {
            double t = lb + i * deltaT;
            points[i] = p + d * (float)t;
        }
        return points;
    }
    public Vector2 Projection(Vector2 point)
    {
        Vector2 AP = point - p;
        float t = Vector2.Dot(AP, d);
        return p + d * t;
    }
}
