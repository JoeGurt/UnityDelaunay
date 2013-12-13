
	
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Delaunay
{
	
	
	public  class Site : ICoord
	{
		private static List<Site> _pool = new List<Site>();
		public static Site create(Vector2 p, int index, float weight, uint color)
		{
			if (_pool.Count > 0)
			{
				return _pool.Pop().init(p, index, weight, color);
			}
			else
			{
				return new Site(typeof(PrivateConstructorEnforcer), p, index, weight, color);
			}
		}
		
		internal static void sortSites(List<Site> sites)
		{
			sites.SortFunc(Site.compare);
		}

		/**
		 * sort sites on y, then x, coord
		 * also change each site's _siteIndex to match its new position in the list
		 * so the _siteIndex can be used to identify the site for nearest-neighbor queries
		 * 
		 * haha "also" - means more than one responsibility...
		 * 
		 */
		private static float compare(Site s1, Site s2)
		{
			int returnValue = (int)Voronoi.compareByYThenX(s1, s2);
			
			// swap _siteIndex values if necessary to match new ordering:
			int tempIndex;
			if (returnValue == -1)
			{
				if (s1._siteIndex > s2._siteIndex)
				{
					tempIndex = (int)s1._siteIndex;
					s1._siteIndex = s2._siteIndex;
					s2._siteIndex = (uint)tempIndex;
				}
			}
			else if (returnValue == 1)
			{
				if (s2._siteIndex > s1._siteIndex)
				{
					tempIndex = (int)s2._siteIndex;
					s2._siteIndex = s1._siteIndex;
					s1._siteIndex = (uint)tempIndex;
				}
				
			}
			
			return returnValue;
		}


		private static readonly float EPSILON = .005f;
		private static bool closeEnough(Vector2 p0, Vector2 p1)
		{
			return Vector2.Distance(p0, p1) < EPSILON;
		}
				
		private Vector2 _coord;

        public Vector2 coord() {
            //get {
                return _coord;
            //}
        }
		
		internal uint color;
		internal float weight;
		
		private uint _siteIndex;
		
		// the edges that define this Site's Voronoi region:
		private List<Edge> _edges;

        internal List<Edge> edges {
            get {
                return _edges;
            }
        }
		// which end of each edge hooks up with the previous edge in _edges:
		private List<LR> _edgeOrientations;
		// ordered list of Vector2s that define the region clipped to bounds:
		private List<Vector2> _region;

		public  Site(Type pce, Vector2 p, int index, float weight, uint color)
		{
			if (pce != typeof(PrivateConstructorEnforcer))
			{
				throw new Exception("Site static readonlyructor is private");
			}
			init(p, index, weight, color);
		}
		
		private Site init(Vector2 p, int index, float weight, uint color)
		{
			_coord = p;
			_siteIndex = (uint)index;
			this.weight = weight;
			this.color = color;
			_edges = new List<Edge>();
			_region = null;
			return this;
		}
		
		public override string ToString()
		{
			return "Site " + _siteIndex + ": " + coord().ToString();
		}
		
		private void move(Vector2 p)
		{
			clear();
			_coord = p;
		}
		
		public void dispose()
		{
			_coord = Vector2.zero;
			clear();
			_pool.Add(this);
		}
		
		private void clear()
		{
			if (_edges != null)
			{
				_edges.Clear();
				_edges = null;
			}
			if (_edgeOrientations != null)
			{
				_edgeOrientations.Clear();
				_edgeOrientations = null;
			}
			if (_region != null)
			{
				_region.Clear();
				_region = null;
			}
		}
		
		internal void addEdge(Edge edge)
		{
			_edges.Add(edge);
		}
		
		internal Edge nearestEdge()
		{
			_edges.SortFunc(Edge.compareSitesDistances);
			return _edges[0];
		}
		
		internal List<Site> neighborSites()
		{
			if (_edges == null || _edges.Count == 0)
			{
				return new List<Site>();
			}
			if (_edgeOrientations == null)
			{ 
				reorderEdges();
			}
			List<Site> list = new List<Site>();
			foreach (Edge edge in _edges)
			{
				list.Add(neighborSite(edge));
			}
			return list;
		}
			
		private Site neighborSite(Edge edge)
		{
			if (this == edge.leftSite)
			{
				return edge.rightSite;
			}
			if (this == edge.rightSite)
			{
				return edge.leftSite;
			}
			return null;
		}
		
		internal List<Vector2> region(Rect clippingBounds)
		{
			if (_edges == null || _edges.Count == 0)
			{
				return new List<Vector2>();
			}
			if (_edgeOrientations == null)
			{ 
				reorderEdges();
				_region = clipToBounds(clippingBounds);
				if ((new Polygon(_region)).winding() == Winding.CLOCKWISE)
				{
					_region.Reverse();
				}
			}
			return _region;
		}
		
		private void reorderEdges()
		{
			//trace("_edges:", _edges);
			EdgeReorderer reorderer = new EdgeReorderer(_edges, typeof(Vertex));
			_edges = reorderer.edges;
			//trace("reordered:", _edges);
			_edgeOrientations = reorderer.edgeOrientations;
			reorderer.dispose();
		}
		
		private List<Vector2> clipToBounds(Rect bounds)
		{
			List<Vector2> Vector2s = new List<Vector2>();
			int n = _edges.Count;
			int i = 0;
			Edge edge;
			while (i < n && ((_edges[i] as Edge).visible == false))
			{
				++i;
			}
			
			if (i == n)
			{
				// no edges visible
				return new List<Vector2>();
			}
			edge = _edges[i];
			LR orientation = _edgeOrientations[i];
			Vector2s.Add(edge.clippedEnds[orientation]);
			Vector2s.Add(edge.clippedEnds[LR.other(orientation)]);
			
			for (int j = i + 1; j < n; ++j)
			{
				edge = _edges[j];
				if (edge.visible == false)
				{
					continue;
				}
				connect(Vector2s, j, bounds);
			}
			// close up the polygon by adding another corner Vector2 of the bounds if needed:
			connect(Vector2s, i, bounds, true);
			
			return Vector2s;
		}
		
		private void connect(List<Vector2>Vector2s, int j, Rect bounds)
        {
            connect(Vector2s, j, bounds, false);
        }
		private void connect(List<Vector2>Vector2s, int j, Rect bounds, bool closingUp)
		{
			Vector2 rightVector2 = Vector2s[Vector2s.Count - 1];
			Edge newEdge = _edges[j] as Edge;
			LR newOrientation = _edgeOrientations[j];
			// the Vector2 that  must be connected to rightVector2:
			Vector2 newVector2 = newEdge.clippedEnds[newOrientation];
			if (!closeEnough(rightVector2, newVector2))
			{
				// The Vector2s do not coincide, so they must have been clipped at the bounds;
				// see if they are on the same border of the bounds:
				if (rightVector2.x != newVector2.x
				&&  rightVector2.y != newVector2.y)
				{
					// They are on different borders of the bounds;
					// insert one or two corners of bounds as needed to hook them up:
					// (NOTE this will not be correct if the region should take up more than
					// half of the bounds rect, for then we will have gone the wrong way
					// around the bounds and included the smaller part rather than the larger)
					int rightCheck = BoundsCheck.check(rightVector2, bounds);
					int newCheck = BoundsCheck.check(newVector2, bounds);
					float px, py;
                    //throw new NotImplementedException("Modified, might not work");
					if (rightCheck == BoundsCheck.RIGHT)
					{
						px = bounds.right;
                        if (newCheck == BoundsCheck.BOTTOM)
						{
							py = bounds.bottom;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.TOP)
						{
							py = bounds.top;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.LEFT)
						{
							if (rightVector2.y - bounds.y + newVector2.y - bounds.y < bounds.height)
							{
								py = bounds.top;
							}
							else
							{
								py = bounds.bottom;
							}
							Vector2s.Add(new Vector2(px, py));
							Vector2s.Add(new Vector2(bounds.left, py));
						}
					}
                    else if (rightCheck == BoundsCheck.LEFT)
					{
						px = bounds.left;
                        if (newCheck == BoundsCheck.BOTTOM)
						{
							py = bounds.bottom;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.TOP)
						{
							py = bounds.top;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.RIGHT)
						{
							if (rightVector2.y - bounds.y + newVector2.y - bounds.y < bounds.height)
							{
								py = bounds.top;
							}
							else
							{
								py = bounds.bottom;
							}
							Vector2s.Add(new Vector2(px, py));
							Vector2s.Add(new Vector2(bounds.right, py));
						}
					}
                    else if (rightCheck == BoundsCheck.TOP)
					{
						py = bounds.top;
                        if (newCheck == BoundsCheck.RIGHT)
						{
							px = bounds.right;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.LEFT)
						{
							px = bounds.left;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.BOTTOM)
						{
							if (rightVector2.x - bounds.x + newVector2.x - bounds.x < bounds.width)
							{
								px = bounds.left;
							}
							else
							{
								px = bounds.right;
							}
							Vector2s.Add(new Vector2(px, py));
							Vector2s.Add(new Vector2(px, bounds.bottom));
						}
					}
                    else if (rightCheck == BoundsCheck.BOTTOM)
					{
						py = bounds.bottom;
                        if (newCheck == BoundsCheck.RIGHT)
						{
							px = bounds.right;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.LEFT)
						{
							px = bounds.left;
							Vector2s.Add(new Vector2(px, py));
						}
                        else if (newCheck == BoundsCheck.TOP)
						{
							if (rightVector2.x - bounds.x + newVector2.x - bounds.x < bounds.width)
							{
								px = bounds.left;
							}
							else
							{
								px = bounds.right;
							}
							Vector2s.Add(new Vector2(px, py));
							Vector2s.Add(new Vector2(px, bounds.top));
						}
					}
				}
				if (closingUp)
				{
					// newEdge's ends have already been added
					return;
				}
				Vector2s.Add(newVector2);
			}
            Vector2 newRightVector2 = newEdge.clippedEnds[LR.other(newOrientation)];
			if (!closeEnough(Vector2s[0], newRightVector2))
			{
				Vector2s.Add(newRightVector2);
			}
		}
								

internal float x
{
get {
return _coord.x;
}
}

internal float y
{
get {
return _coord.y;
}
}
		
		internal float dist(ICoord p)
		{
			return Vector2.Distance(p.coord(), this._coord);
		}

	}
}


	
	 class BoundsCheck
	{
		public static readonly int TOP = 1;
		public static readonly int BOTTOM = 2;
		public static readonly int LEFT = 4;
		public static readonly int RIGHT = 8;
		
		/**
		 * 
		 * @param Vector2
		 * @param bounds
		 * @return an int with the appropriate bits set if the Vector2 lies on the corresponding bounds lines
		 * 
		 */
		public static int check(Vector2 point, Rect bounds)
		{
            int value = 0;
			if (point.x == bounds.left)
			{
				value |= LEFT;
			}
			if (point.x == bounds.right)
			{
				value |= RIGHT;
			}
			if (point.y == bounds.top)
			{
				value |= TOP;
			}
			if (point.y == bounds.bottom)
			{
				value |= BOTTOM;
			}
			return value;
		}
		
		public BoundsCheck()
		{
			throw new Exception("BoundsCheck static readonlyructor unused");
		}

	}