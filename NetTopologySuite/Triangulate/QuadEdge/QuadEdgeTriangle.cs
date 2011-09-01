using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// Models a triangle formed from <see cref="QuadEdge"/>s in a <see cref="QuadEdgeSubdivision"/>. Provides methods to
    /// access the topological and geometric properties of the triangle. Triangle edges are oriented CCW.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <version>1.0</version>
    public class QuadEdgeTriangle
    {

        /// <summary>
        /// Tests whether the point pt is contained in the triangle defined by 3 <see cref="Vertex"/>es.
        /// </summary>
        /// <param name="tri">an array containing at least 3 Vertexes</param>
        /// <param name="pt">the point to test</param>
        /// <returns>true if the point is contained in the triangle</returns>
        public static bool Contains(Vertex[] tri, ICoordinate pt)
        {
            var ring = new[]
                {
                    tri[0].Coordinate, 
                    tri[1].Coordinate, 
                    tri[2].Coordinate, 
                    tri[0].Coordinate
                };
            return CGAlgorithms.IsPointInRing(pt, ring);
        }

        /// <summary>
        /// Tests whether the point pt is contained in the triangle defined by 3 <see cref="QuadEdge"/>es.
        /// </summary>
        /// <param name="tri">an array containing at least 3 QuadEdges</param>
        /// <param name="pt">the point to test</param>
        /// <returns>true if the point is contained in the triangle</returns>
        public static bool Contains(QuadEdge[] tri, ICoordinate pt)
        {
            var ring = new []
                                    {
                                        tri[0].Orig.Coordinate,
                                        tri[1].Orig.Coordinate,
                                        tri[2].Orig.Coordinate,
                                        tri[0].Orig.Coordinate
                                    };
            return CGAlgorithms.IsPointInRing(pt, ring);
        }

        public static IGeometry ToPolygon(Vertex[] v)
        {
            var ringPts = new []
                                       {
                                           v[0].Coordinate,
                                           v[1].Coordinate,
                                           v[2].Coordinate,
                                           v[0].Coordinate
                                       };
            var fact = new GeometryFactory();
            var ring = fact.CreateLinearRing(ringPts);
            var tri = fact.CreatePolygon(ring, null);
            return tri;
        }

        public static IGeometry ToPolygon(QuadEdge[] e)
        {
            var ringPts = new []
                                       {
                                           e[0].Orig.Coordinate,
                                           e[1].Orig.Coordinate,
                                           e[2].Orig.Coordinate,
                                           e[0].Orig.Coordinate
                                       };
            var fact = new GeometryFactory();
            ILinearRing ring = fact.CreateLinearRing(ringPts);
            IPolygon tri = fact.CreatePolygon(ring, null);
            return tri;
        }

        /*
        /// <summary>
        /// Finds the next index around the triangle. Index may be an edge or vertex index.
        /// </summary>
        /// <param name="index" />
        /// <returns />
        public static int NextIndex(int index)
        {
            return index = (index + 1)%3;
        }
         */
        private QuadEdge[] _edge;

        public QuadEdgeTriangle(QuadEdge[] edge)
        {
            this._edge = (QuadEdge[]) edge.Clone();
        }

        public void Kill()
        {
            this._edge = null;
        }

        public bool IsLive()
        {
            return this._edge != null;
        }

        public QuadEdge[] GetEdges()
        {
            return this._edge;
        }

        public QuadEdge GetEdge(int i)
        {
            return this._edge[i];
        }

        public Vertex GetVertex(int i)
        {
            return this._edge[i].Orig;
        }

        /// <summary>
        /// Gets the vertices for this triangle.
        /// </summary>
        /// <returns>a new array containing the triangle vertices</returns>
        public Vertex[] GetVertices()
        {
            var vert = new Vertex[3];
            for (int i = 0; i < 3; i++)
            {
                vert[i] = GetVertex(i);
            }
            return vert;
        }

        public ICoordinate GetCoordinate(int i)
        {
            return this._edge[i].Orig.Coordinate;
        }

        /// <summary>
        /// Gets the index for the given edge of this triangle
        /// </summary>
        /// <param name="e">a QuadEdge</param>
        /// <returns>the index of the edge in this triangle,
        /// or -1 if the edge is not an edge of this triangle
        /// </returns>
        public int GetEdgeIndex(QuadEdge e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this._edge[i] == e)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index for the edge that starts at vertex v.
        /// </summary>
        /// <param name="v">the vertex to find the edge for</param>
        /// <returns>the index of the edge starting at the vertex, 
        /// or -1 if the vertex is not in the triangle
        /// </returns>
        public int GetEdgeIndex(Vertex v)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this._edge[i].Orig == v)
                    return i;
            }
            return -1;
        }

        public void GetEdgeSegment(int i, LineSegment seg)
        {
            seg.P0 = this._edge[i].Orig.Coordinate;
            int nexti = (i + 1)%3;
            seg.P1 = this._edge[nexti].Orig.Coordinate;
        }

        public ICoordinate[] GetCoordinates()
        {
            var pts = new ICoordinate[4];
            for (int i = 0; i < 3; i++)
            {
                pts[i] = this._edge[i].Orig.Coordinate;
            }
            pts[3] = new Coordinate(pts[0]);
            return pts;
        }

        public bool Contains(ICoordinate pt)
        {
            var ring = GetCoordinates();
            return CGAlgorithms.IsPointInRing(pt, ring);
        }

        public IGeometry GetGeometry(GeometryFactory fact)
        {
            ILinearRing ring = fact.CreateLinearRing(GetCoordinates());
            IPolygon tri = fact.CreatePolygon(ring, null);
            return tri;
        }

        public override String ToString()
        {
            return GetGeometry(new GeometryFactory()).ToString();
        }

        /// <summary>
        /// Tests whether this triangle is adjacent to the outside of the subdivision.
        /// </summary>
        /// <returns>true if the triangle is adjacent to the subdivision exterior</returns>
        public bool IsBorder()
        {
            for (int i = 0; i < 3; i++)
            {
                if (GetAdjacentTriangleAcrossEdge(i) == null)
                    return true;
            }
            return false;
        }

        public bool IsBorder(int i)
        {
            return GetAdjacentTriangleAcrossEdge(i) == null;
        }

        public QuadEdgeTriangle GetAdjacentTriangleAcrossEdge(int edgeIndex)
        {
            return (QuadEdgeTriangle) GetEdge(edgeIndex).Sym.Data;
        }

        public int GetAdjacentTriangleEdgeIndex(int i)
        {
            return GetAdjacentTriangleAcrossEdge(i).GetEdgeIndex(GetEdge(i).Sym);
        }

        public IList<QuadEdgeTriangle> GetTrianglesAdjacentToVertex(int vertexIndex)
        {
            // Assert: isVertex
            var adjTris = new List<QuadEdgeTriangle>();

            QuadEdge start = GetEdge(vertexIndex);
            QuadEdge qe = start;
            do
            {
                var adjTri = (QuadEdgeTriangle) qe.Data;
                if (adjTri != null)
                {
                    adjTris.Add(adjTri);
                }
                qe = qe.ONext;
            } while (qe != start);

            return adjTris;

        }

        /// <summary>
        /// Gets the neighbours of this triangle. If there is no neighbour triangle, the array element is
        /// <code>null</code>
        /// </summary>
        /// <returns>an array containing the 3 neighbours of this triangle</returns>
        public QuadEdgeTriangle[] GetNeighbours()
        {
            var neigh = new QuadEdgeTriangle[3];
            for (int i = 0; i < 3; i++)
            {
                neigh[i] = (QuadEdgeTriangle) GetEdge(i).Sym.Data;
            }
            return neigh;
        }
    }
}