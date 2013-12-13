using System.Collections.Generic;
	
using UnityEngine;

namespace Delaunay
{
    public class visibleLineSegmentsClass
    {
        static internal List<LineSegment> visibleLineSegments(List<Edge> edges)
        {
            List<LineSegment> segments = new List<LineSegment>();

            foreach (Edge edge in edges)
            {
                if (edge.visible)
                {
                    Vector2 p1 = edge.clippedEnds[LR.LEFT];
                    Vector2 p2 = edge.clippedEnds[LR.RIGHT];
                    segments.Add(new LineSegment(p1, p2));
                }
            }

            return segments;
        }
    }

}