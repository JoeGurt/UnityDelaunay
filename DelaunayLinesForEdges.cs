using System.Collections.Generic;



namespace Delaunay
{
    public static class delaunayLinesForEdgesClass
    {
        internal static List<LineSegment> delaunayLinesForEdges(List<Edge> edges)
	{
		List<LineSegment> segments = new List<LineSegment>();
		foreach (Edge edge in edges)
		{
			segments.Add(edge.delaunayLine());
		}
		return segments;
	}
    }

}