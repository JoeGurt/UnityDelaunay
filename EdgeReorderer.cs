using System;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
	
	
	internal  class EdgeReorderer
	{
		private List<Edge> _edges;
		private List<LR> _edgeOrientations;
		public List<Edge> edges
		{
            get {
			return _edges;
            }
		}
		public List<LR> edgeOrientations
		{
            get {
			return _edgeOrientations;
            }
		}
		
		public EdgeReorderer( List<Edge> origEdges, Type criterion)
		{
			if (criterion != typeof(Vertex) && criterion != typeof(Site))
			{
				throw new ArgumentException("Edges: criterion must be Vertex or Site");
			}
			_edges = new List<Edge>();
			_edgeOrientations = new List<LR>();
			if (origEdges.Count > 0)
			{
				_edges = reorderEdges(origEdges, criterion);
			}
		}
		
		public void dispose()
		{
			_edges = null;
			_edgeOrientations = null;
		}

		private List<Edge> reorderEdges(List<Edge> origEdges, Type criterion)
		{
			int i;
			int j;
			int n = origEdges.Count;
			Edge edge;
			// we're going to reorder the edges in order of traversal
			List<bool> done = new List<bool>(n);
			int nDone = 0;
			for (int o = 0; o < n; o++)
			{
				done.Add(false);
			}
			List<Edge> newEdges = new List<Edge>();
			
			i = 0;
			edge = origEdges[i];
			newEdges.Add(edge);
			_edgeOrientations.Add(LR.LEFT);
			ICoord firstVector2 = null;
            ICoord lastVector2 = null;
            if ((criterion == typeof(Vertex)))
            {
                firstVector2 = edge.leftVertex;
                lastVector2 = edge.rightVertex;
            }
            else
            {
                firstVector2 = edge.leftSite;
                lastVector2 = edge.rightSite;
            }
			
			if (firstVector2 == Vertex.VERTEX_AT_INFINITY || lastVector2 == Vertex.VERTEX_AT_INFINITY)
			{
				return new List<Edge>();
			}
			
			done[i] = true;
			++nDone;
			
			while (nDone < n)
			{
				for (i = 1; i < n; ++i)
				{
					if (done[i])
					{
						continue;
					}
					edge = origEdges[i];
                    ICoord leftVector2 = null;
                    ICoord rightVector2 = null;
                    if ((criterion == typeof(Vertex)))
                    {
                        leftVector2 = edge.leftVertex;
                        rightVector2 = edge.rightVertex;
                    }
                    else
                    {
                        leftVector2 = edge.leftSite;
                        rightVector2 = edge.rightSite;
                    }
					if (leftVector2 == Vertex.VERTEX_AT_INFINITY || rightVector2 == Vertex.VERTEX_AT_INFINITY)
					{
						return new List<Edge>();
					}
					if (leftVector2 == lastVector2)
					{
						lastVector2 = rightVector2;
						_edgeOrientations.Add(LR.LEFT);
						newEdges.Add(edge);
						done[i] = true;
					}
					else if (rightVector2 == firstVector2)
					{
						firstVector2 = leftVector2;
                        _edgeOrientations.Unshift(LR.LEFT);
                        newEdges.Unshift(edge);
						done[i] = true;
					}
					else if (leftVector2 == firstVector2)
					{
						firstVector2 = rightVector2;
                        _edgeOrientations.Unshift(LR.RIGHT);
                        newEdges.Unshift(edge);
						done[i] = true;
					}
					else if (rightVector2 == lastVector2)
					{
						lastVector2 = leftVector2;
						_edgeOrientations.Add(LR.RIGHT);
						newEdges.Add(edge);
						done[i] = true;
					}
					if (done[i])
					{
						++nDone;
					}
				}
			}
			
			return newEdges;
		}

	}
}