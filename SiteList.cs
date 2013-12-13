using System;
using System.Collections.Generic;
using UnityEngine;
	
	

namespace Delaunay
{

	internal  class SiteList : IDisposable
	{
		private List<Site> _sites;
		private uint _currentIndex;

        private bool _sorted;

        public void Dispose()
        {
            dispose();
        }
		
		public SiteList()
		{
			_sites = new List<Site>();
			_sorted = false;
		}
		
		public void dispose()
		{
			if (_sites != null)
			{
				foreach (Site site in _sites)
				{
					site.dispose();
				}
				_sites.Clear();
				_sites = null;
			}
		}
		
		public uint push(Site site)
		{
			_sorted = false;
			return (uint)_sites.Push(site);
		}
		

    public uint length {
    get {
    return (uint)_sites.Count;
}
}
		
		public Site next()
		{
			if (_sorted == false)
			{
				throw new Exception("SiteList::next():  sites have not been sorted");
			}
			if (_currentIndex < _sites.Count)
			{
				return _sites[(int)_currentIndex++];
			}
			else
			{
				return null;
			}
		}

		internal Rect getSitesBounds()
		{
			if (_sorted == false)
			{
				Site.sortSites(_sites);
				_currentIndex = 0;
				_sorted = true;
			}
			float xmin, xmax, ymin, ymax;
			if (_sites.Count == 0)
			{
				return new Rect(0, 0, 0, 0);
			}
			xmin = float.MaxValue;
			xmax = float.MinValue;
            foreach (Site site in _sites)
			{
				if (site.x < xmin)
				{
					xmin = site.x;
				}
				if (site.x > xmax)
				{
					xmax = site.x;
				}
			}
			// here's where we assume that the sites have been sorted on y:
			ymin = _sites[0].y;
			ymax = _sites[_sites.Count - 1].y;
			
			return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
		}

		public List<uint> siteColors()
        {
            return siteColors(null);
        }
		public List<uint> siteColors(BitmapData referenceImage)
		{
            List<uint> colors = new List<uint>();
            foreach (Site site in _sites)
			{
				colors.Add(referenceImage != null ? referenceImage.getPixel(site.x, site.y) : site.color);
			}
			return colors;
		}

		public List<Vector2> siteCoords()
		{
			List<Vector2> coords = new List<Vector2>();
			foreach (Site site in _sites)
			{
				coords.Add(site.coord());
			}
			return coords;
		}

		/**
		 * 
		 * @return the largest circle centered at each site that fits in its region;
		 * if the region is infinite, return a circle of radius 0.
		 * 
		 */
		public List<Circle> circles()
		{
			List<Circle> circles = new List<Circle>();
			foreach (Site site in _sites)
			{
				float radius = 0;
				Edge nearestEdge = site.nearestEdge();

                throw new NotImplementedException();
                //!nearestEdge.isPartOfConvexHull() && (radius = nearestEdge.sitesDistance() * 0.5);
				circles.Add(new Circle(site.x, site.y, radius));
			}
			return circles;
		}

		public List<List<Vector2>> regions(Rect plotBounds)
		{
			List<List<Vector2>> regions = new List<List<Vector2>>();
			foreach (Site site in _sites)
			{
				regions.Add(site.region(plotBounds));
			}
			return regions;
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
			uint index = proximityMap.getPixel(x, y);
			if (index > _sites.Count - 1)
			{
				return Vector2.zero;
			}
			return _sites[(int)index].coord();
		}
		
	}
}