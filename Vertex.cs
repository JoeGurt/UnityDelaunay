using UnityEngine;
using System;
using System.Collections.Generic;

namespace Delaunay
{
	
	internal  class Vertex : ICoord
	{
		internal static readonly  Vertex VERTEX_AT_INFINITY = new Vertex(typeof(PrivateConstructorEnforcer), float.NaN, float.NaN);
		
		private static List<Vertex> _pool = new List<Vertex>();
		private static Vertex create(float x, float y)
		{
			if (float.IsNaN(x) || float.IsNaN(y))
			{
				return VERTEX_AT_INFINITY;
			}
			if (_pool.Count > 0)
			{
				return _pool.Pop().init(x, y);
			}
			else
			{
				return new Vertex(typeof(PrivateConstructorEnforcer), x, y);
			}
		}


		private static int _nvertices = 0;
		
		private Vector2 _coord;
public Vector2 coord() {return _coord;}
		private int _vertexIndex;
public int vertexIndex {get{return _vertexIndex;}}
		
		public Vertex(Type pce, float x, float y)
		{
			if (pce != typeof(PrivateConstructorEnforcer))
			{
				throw new Exception("Vertex static readonlyructor is private");
			}
			
			init(x, y);
		}
		
		private Vertex init(float x, float y)
		{
			_coord = new Vector2(x, y);
			return this;
		}
		
		public void dispose()
		{
			_coord = Vector2.zero;
			_pool.Add(this);
		}
		
		public void setIndex()
		{
			_vertexIndex = _nvertices++;
		}
		
		public override string ToString()
		{
			return "Vertex (" + _vertexIndex + ")";
		}

		/**
		 * This is the only way to make a Vertex
		 * 
		 * @param halfedge0
		 * @param halfedge1
		 * @return 
		 * 
		 */
		public static Vertex intersect(Halfedge halfedge0, Halfedge halfedge1)
		{
			Edge edge0, edge1, edge;
			Halfedge halfedge;
			float determinant, intersectionX, intersectionY;
			bool rightOfSite;
		
			edge0 = halfedge0.edge;
			edge1 = halfedge1.edge;
			if (edge0 == null || edge1 == null)
			{
				return null;
			}
			if (edge0.rightSite == edge1.rightSite)
			{
				return null;
			}
		
			determinant = edge0.a * edge1.b - edge0.b * edge1.a;
			if (-1.0e-10 < determinant && determinant < 1.0e-10)
			{
				// the edges are parallel
				return null;
			}
		
			intersectionX = (edge0.c * edge1.b - edge1.c * edge0.b)/determinant;
			intersectionY = (edge1.c * edge0.a - edge0.c * edge1.a)/determinant;
		
			if (Voronoi.compareByYThenX(edge0.rightSite, edge1.rightSite) < 0)
			{
				halfedge = halfedge0; edge = edge0;
			}
			else
			{
				halfedge = halfedge1; edge = edge1;
			}
			rightOfSite = intersectionX >= edge.rightSite.x;
			if ((rightOfSite && halfedge.leftRight == LR.LEFT)
			||  (!rightOfSite && halfedge.leftRight == LR.RIGHT))
			{
				return null;
			}
		
			return Vertex.create(intersectionX, intersectionY);
		}

        public float x {get{return _coord.x;}}
        public float y {get{return _coord.y;}}
		
		
	}
}