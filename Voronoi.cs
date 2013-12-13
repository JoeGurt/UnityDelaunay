/*
 * The author of this software is Steven Fortune.  Copyright (c) 1994 by AT&T
 * Bell Laboratories.
 * Permission to use, copy, modify, and distribute this software for any
 * purpose without fee is hereby granted, provided that this entire notice
 * is included in all copies of any software which is or includes a copy
 * or modification of this software and in all copies of the supporting
 * documentation for such software.
 * THIS SOFTWARE IS BEING PROVIDED "AS IS", WITHOUT ANY EXPRESS OR IMPLIED
 * WARRANTY.  IN PARTICULAR, NEITHER THE AUTHORS NOR AT&T MAKE ANY
 * REPRESENTATION OR WARRANTY OF ANY KIND CONCERNING THE MERCHANTABILITY
 * OF THIS SOFTWARE OR ITS FITNESS FOR ANY PARTICULAR PURPOSE.
 */

using System;
using System.Collections.Generic;
using UnityEngine;



namespace Delaunay
{
	
	
	public  class Voronoi
	{
		private SiteList _sites;
		private Dictionary<Vector2, Site> _sitesIndexedByLocation;
		private List<Triangle> _triangles;
		private List<Edge> _edges;

		
		// TODO generalize this so it doesn't have to be a Rect;
		// then we can make the fractal voronois-within-voronois
		private Rect _plotBounds;
        public Rect plotBounds {
            get {
                return _plotBounds;
            }
        }
		
		public void dispose()
		{
			int i, n;
			if (_sites != null)
			{
				_sites.dispose();
				_sites = null;
			}
			if (_triangles != null)
			{
				n = _triangles.Count;
				for (i = 0; i < n; ++i)
				{
					_triangles[i].dispose();
				}
				_triangles.Clear();
				_triangles = null;
			}
			if (_edges != null)
			{
				n = _edges.Count;
				for (i = 0; i < n; ++i)
				{
					_edges[i].dispose();
				}
				_edges.Clear();
				_edges = null;
			}
			_plotBounds = new Rect();
			_sitesIndexedByLocation = null;
		}
		
		public Voronoi(List<Vector2> Vector2s, List<uint> colors, Rect plotBounds)
		{
			_sites = new SiteList();
			_sitesIndexedByLocation = new Dictionary<Vector2, Site>();
			addSites(Vector2s, colors);
			_plotBounds = plotBounds;
			_triangles = new List<Triangle>();
			_edges = new List<Edge>();
			fortunesAlgorithm();
		}
		
		private void addSites(List<Vector2> Vector2s, List<uint> colors)
		{
			uint length = (uint)Vector2s.Count;
			for (uint i = 0; i < length; ++i)
			{
				addSite(Vector2s[(int)i], colors != null ? colors[(int)i] : 0, (int)i);
			}
		}
		
		private void addSite(Vector2 p, uint color, int index)
		{
            //throw new NotImplementedException("This was modified, might not work");
            System.Random random = new System.Random();
			float weight = (float)random.NextDouble() * 100;
			Site site = Site.create(p, index, weight, color);
			_sites.push(site);
			_sitesIndexedByLocation[p] = site;
		}

                public List<Edge> edges()
                {
                	return _edges;
                }
          
		public List<Vector2> region(Vector2 p)
		{
			Site site = _sitesIndexedByLocation[p];
			if (site == null)
			{
				return new List<Vector2>();
			}
			return site.region(_plotBounds);
		}

          // TODO: bug: if you call this before you call region(), something goes wrong :(
		public List<Vector2> neighborSitesForSite(Vector2 coord)
		{
			List<Vector2> Vector2s = new List<Vector2>();
			Site site = _sitesIndexedByLocation[coord];
			if (site == null)
			{
				return Vector2s;
			}
			List<Site> sites = site.neighborSites();
			foreach (Site neighbor in sites)
			{
				Vector2s.Add(neighbor.coord());
			}
			return Vector2s;
		}

		public List<Circle> circles()
		{
			return _sites.circles();
		}
		
		public List<LineSegment> voronoiBoundaryForSite(Vector2 coord)
		{
            return visibleLineSegmentsClass.visibleLineSegments(selectEdgesForSiteVector2Class.selectEdgesForSiteVector2(coord, _edges));
		}

		public List<LineSegment> delaunayLinesForSite(Vector2 coord)
		{
            return delaunayLinesForEdgesClass.delaunayLinesForEdges(selectEdgesForSiteVector2Class.selectEdgesForSiteVector2(coord, _edges));
		}
		
		public List<LineSegment> voronoiDiagram()
		{
            return visibleLineSegmentsClass.visibleLineSegments(_edges);
		}
		
		public List<LineSegment> delaunayTriangulation()
        {
            return delaunayTriangulation(null);
        }
		public List<LineSegment> delaunayTriangulation(BitmapData keepOutMask)
		{
            return delaunayLinesForEdgesClass.delaunayLinesForEdges(selectNonIntersectingEdgesClass.selectNonIntersectingEdges(keepOutMask, _edges));
		}
		
		public List<LineSegment> hull()
		{
            return delaunayLinesForEdgesClass.delaunayLinesForEdges(mhullEdges());
		}
		
		private List<Edge> mhullEdges()
		{
			return _edges.Filter(myTestHullEdges);
		}
		
			bool myTestHullEdges(Edge edge, int index, List<Edge> vector)
			{
				return (edge.isPartOfConvexHull());
			}

		public List<Vector2> hullVector2sInOrder()
		{
			List<Edge> hullEdges = mhullEdges();
			
			List<Vector2> Vector2s = new List<Vector2>();
			if (hullEdges.Count == 0)
			{
				return Vector2s;
			}
			
			EdgeReorderer reorderer = new EdgeReorderer(hullEdges, typeof(Site));
			hullEdges = reorderer.edges;
			List<LR> orientations = reorderer.edgeOrientations;
			reorderer.dispose();
			
			LR orientation;

			int n = hullEdges.Count;
			for (int i = 0; i < n; ++i)
			{
				Edge edge = hullEdges[i];
				orientation = orientations[i];
				Vector2s.Add(edge.site(orientation).coord());
			}
			return Vector2s;
		}

        public List<LineSegment> spanningTree(BitmapData keepOutMask)
        {
            return spanningTree("minimum", keepOutMask);
        }

        public List<LineSegment> spanningTree()
        {
            return spanningTree("minimum", null);
        }

        public List<LineSegment> spanningTree(string type)
        {
            return spanningTree(type, null);
        }
		public List<LineSegment> spanningTree(string type, BitmapData keepOutMask)
		{
            List<Edge> edges = selectNonIntersectingEdgesClass.selectNonIntersectingEdges(keepOutMask, _edges);
            List<LineSegment> segments = delaunayLinesForEdgesClass.delaunayLinesForEdges(edges);
            return kruskalClass.kruskal(segments, type);
		}

		public List<List<Vector2>> regions()
		{
			return _sites.regions(_plotBounds);
		}
		
		public List<uint> siteColors()
        {
            return siteColors(null);
        }
		public List<uint> siteColors(BitmapData referenceImage)
		{
			return _sites.siteColors(referenceImage);
		}
		
		/**
		 * 
		 * @param proximityMap a BitmapData whose regions are filled with the site index values; see PlaneVector2sCanvas::fillRegions()
		 * @param x
		 * @param y
		 * @return coordinates of nearest Site to (x, y)
		 * 
		 */
		public Vector2 nearestSiteVector2(BitmapData proximityMap, float x, float y)
		{
			return _sites.nearestSiteVector2(proximityMap, x, y);
		}
		
		public List<Vector2> siteCoords()
		{
			return _sites.siteCoords();
		}

		private void fortunesAlgorithm()
		{
			Site newSite, bottomSite, topSite, tempSite;
			Vertex v, vertex;
			Vector2 newintstar = Vector2.zero;
			LR leftRight;
			Halfedge lbnd, rbnd, llbnd, rrbnd, bisector;
			Edge edge;
			
			Rect dataBounds = _sites.getSitesBounds();
			
			int sqrt_nsites = (int)(Math.Sqrt(_sites.length + 4));
			HalfedgePriorityQueue heap = new HalfedgePriorityQueue(dataBounds.y, dataBounds.height, sqrt_nsites);
			EdgeList edgeList = new EdgeList(dataBounds.x, dataBounds.width, sqrt_nsites);
			List<Halfedge> halfEdges = new List<Halfedge>();
			List<Vertex> vertices = new List<Vertex>();
			
			Site bottomMostSite = _sites.next();
			newSite = _sites.next();
			
			for (;;)
			{
				if (heap.empty() == false)
				{
					newintstar = heap.min();
				}
			
				if (newSite != null 
				&&  (heap.empty() || compareByYThenX(newSite, newintstar) < 0))
				{
					/* new site is smallest */
					//trace("smallest: new site " + newSite);
					
					// Step 8:
					lbnd = edgeList.edgeListLeftNeighbor(newSite.coord());	// the Halfedge just to the left of newSite
					//trace("lbnd: " + lbnd);
					rbnd = lbnd.edgeListRightNeighbor;		// the Halfedge just to the right
					//trace("rbnd: " + rbnd);
					bottomSite = rightRegion(lbnd, bottomMostSite);		// this is the same as leftRegion(rbnd)
					// this Site determines the region containing the new site
					//trace("new Site is in region of existing site: " + bottomSite);
					
					// Step 9:
					edge = Edge.createBisectingEdge(bottomSite, newSite);
					//trace("new edge: " + edge);
					_edges.Add(edge);
					
					bisector = Halfedge.create(edge, LR.LEFT);
					halfEdges.Add(bisector);
					// inserting two Halfedges into edgeList static readonlyitutes Step 10:
					// insert bisector to the right of lbnd:
					edgeList.insert(lbnd, bisector);
					
					// first half of Step 11:
					if ((vertex = Vertex.intersect(lbnd, bisector)) != null) 
					{
						vertices.Add(vertex);
						heap.remove(lbnd);
						lbnd.vertex = vertex;
						lbnd.ystar = vertex.y + newSite.dist(vertex);
						heap.insert(lbnd);
					}
					
					lbnd = bisector;
					bisector = Halfedge.create(edge, LR.RIGHT);
					halfEdges.Add(bisector);
					// second Halfedge for Step 10:
					// insert bisector to the right of lbnd:
					edgeList.insert(lbnd, bisector);
					
					// second half of Step 11:
					if ((vertex = Vertex.intersect(bisector, rbnd)) != null)
					{
						vertices.Add(vertex);
						bisector.vertex = vertex;
						bisector.ystar = vertex.y + newSite.dist(vertex);
						heap.insert(bisector);	
					}
					
					newSite = _sites.next();	
				}
				else if (heap.empty() == false) 
				{
					/* intersection is smallest */
					lbnd = heap.extractMin();
					llbnd = lbnd.edgeListLeftNeighbor;
					rbnd = lbnd.edgeListRightNeighbor;
					rrbnd = rbnd.edgeListRightNeighbor;
					bottomSite = leftRegion(lbnd, bottomMostSite);
					topSite = rightRegion(rbnd, bottomMostSite);
					// these three sites define a Delaunay triangle
					// (not actually using these for anything...)
					//_triangles.Add(new Triangle(bottomSite, topSite, rightRegion(lbnd)));
					
					v = lbnd.vertex;
					v.setIndex();
					lbnd.edge.setVertex(lbnd.leftRight, v);
					rbnd.edge.setVertex(rbnd.leftRight, v);
					edgeList.remove(lbnd); 
					heap.remove(rbnd);
					edgeList.remove(rbnd); 
					leftRight = LR.LEFT;
					if (bottomSite.y > topSite.y)
					{
						tempSite = bottomSite; bottomSite = topSite; topSite = tempSite; leftRight = LR.RIGHT;
					}
					edge = Edge.createBisectingEdge(bottomSite, topSite);
					_edges.Add(edge);
					bisector = Halfedge.create(edge, leftRight);
					halfEdges.Add(bisector);
					edgeList.insert(llbnd, bisector);
					edge.setVertex(LR.other(leftRight), v);
					if ((vertex = Vertex.intersect(llbnd, bisector)) != null)
					{
						vertices.Add(vertex);
						heap.remove(llbnd);
						llbnd.vertex = vertex;
						llbnd.ystar = vertex.y + bottomSite.dist(vertex);
						heap.insert(llbnd);
					}
					if ((vertex = Vertex.intersect(bisector, rrbnd)) != null)
					{
						vertices.Add(vertex);
						bisector.vertex = vertex;
						bisector.ystar = vertex.y + bottomSite.dist(vertex);
						heap.insert(bisector);
					}
				}
				else
				{
					break;
				}
			}
			
			// heap should be empty now
			heap.dispose();
			edgeList.dispose();
			
			foreach (Halfedge halfEdge in halfEdges)
			{
				halfEdge.reallyDispose();
			}
			halfEdges.Clear();
			
			// we need the vertices to clip the edges
			foreach (Edge edge2 in _edges)
			{
				edge2.clipVertices(_plotBounds);
			}
			// but we don't actually ever use them again!
			foreach (Vertex vertex2 in vertices)
			{
				vertex2.dispose();
			}
			vertices.Clear();
		}

        Site leftRegion(Halfedge he, Site bottomMostSite)
			{
				Edge edge = he.edge;
				if (edge == null)
				{
					return bottomMostSite;
				}
				return edge.site(he.leftRight);
			}

        Site rightRegion(Halfedge he, Site bottomMostSite)
			{
				Edge edge = he.edge;
				if (edge == null)
				{
					return bottomMostSite;
				}
				return edge.site(LR.other(he.leftRight));
			}

		internal static float compareByYThenX(Site s1, Site s2)
		{
			if (s1.y < s2.y) return -1;
			if (s1.y > s2.y) return 1;
			if (s1.x < s2.x) return -1;
			if (s1.x > s2.x) return 1;
			return 0;
		}

        internal static float compareByYThenX(Site s1, Vector2 s2)
        {
            if (s1.y < s2.y) return -1;
            if (s1.y > s2.y) return 1;
            if (s1.x < s2.x) return -1;
            if (s1.x > s2.x) return 1;
            return 0;
        }

	}
}