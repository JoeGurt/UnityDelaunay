using UnityEngine;

using System.Collections.Generic;


namespace Delaunay
{
	public class selectEdgesForSiteVector2Class {
        static Vector2 _coord;
	internal static List<Edge> selectEdgesForSiteVector2(Vector2 coord, List<Edge> edgesToTest)
	{
        _coord = coord;
		return edgesToTest.Filter(myTest);
	}

    static bool myTest(Edge edge, int index, List<Edge> vector)
		{
			return ((edge.leftSite != null && edge.leftSite.coord() == _coord)
			||  (edge.rightSite != null && edge.rightSite.coord() == _coord));
		}
}
}