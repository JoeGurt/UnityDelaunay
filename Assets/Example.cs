using Delaunay;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    void Start()
    {
        List<Vector2> points = GetRandomPoints();
        Rect size = new Rect(0, 0, 100, 100);
        Voronoi voronoi = new Voronoi(points, null, size);
        foreach (Vector2 point in points)
        {
            List<Vector2> region = voronoi.Region(point);
            for (int i = 0; i < region.Count; i++)
            {
                Vector2 next = i < region.Count - 1 ? region[i + 1] : region[0];
                Debug.DrawLine(region[i], next, Color.white, 1000);
            }
        }
    }

    private List<Vector2> GetRandomPoints()
    {
        List<Vector2> points = new List<Vector2>();
        int border = 10;
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(border, 100 - border * 2);
            float y = Random.Range(border, 100 - border * 2);
            points.Add(new Vector2(x, y));
        }
        return points;
    }
}