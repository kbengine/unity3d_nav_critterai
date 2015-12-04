/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using UnityEngine;
using org.critterai.geom;
using org.critterai.u3d;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides methods useful for debugging navigation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All draw methods use GL, so they should generally be called from within the 
    /// OnRenderObject() method.
    /// </para>
    /// </remarks>
    public static class NavDebug
    {
        #region Static Config

        /// <summary>
        /// The alpha to use for surface fill.
        /// </summary>
        public static float surfaceAlpha = 0.25f;

        /// <summary>
        /// The color to use when drawing corner visualizations.
        /// </summary>
        public static Color cornerColor = 
            new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.66f);

        /// <summary>
        /// The scale to use for corner markers.
        /// </summary>
        public static float cornerScale = 0.2f;

        /// <summary>
        /// The color to use when drawing position visualizations.
        /// </summary>
        public static Color positionColor = 
            new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.5f);

        /// <summary>
        /// The scale to use for position markers.
        /// </summary>
        public static float positionScale = 0.8f;

        /// <summary>
        /// The base color to use when drawing goal visualizations.
        /// </summary>
        public static Color goalColor = 
            new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f);

        /// <summary>
        /// The scale to use for goal markers.
        /// </summary>
        public static float goalScale = 0.8f;

        /// <summary>
        /// The color to use for drawing visualizations that overlay polygons.
        /// </summary>
        public static Color polygonOverlayColor = 
            new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.33f);

        #endregion

        #region Navmesh

        /// <summary>
        /// Draws a debug visualization of the navigation mesh.
        /// </summary>
        /// <param name="mesh">The mesh to draw.</param>
        /// <param name="colorByArea">
        /// If true, will be colored by polygon area. If false, will be colored by tile index.
        /// </param>
        public static void Draw(Navmesh mesh, bool colorByArea)
        {
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                Draw(mesh.GetTile(i), null, null, 0, colorByArea ? -1 : i);
            }
        }

        /// <summary>
        /// Draws a debug visualization of the specified navigation mesh tile.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is safe to call on empty tile locations.
        /// </para>
        /// </remarks>
        /// <param name="mesh">The mesh.</param>
        /// <param name="tx">The tile grid x-location.</param>
        /// <param name="tz">The tile grid z-location.</param>
        /// <param name="layer">The tile layer.</param>
        /// <param name="colorByArea">
        /// If true, will be colored by polygon area. If false, will be colored by tile index.
        /// </param>
        public static void Draw(Navmesh mesh, int tx, int tz, int layer, bool colorByArea)
        {
            NavmeshTile tile = mesh.GetTile(tx, tz, layer);

            if (tile == null)
                // No tile at grid location.
                return;
            
            Draw(tile, null, null, 0
                , colorByArea ? -1 : tx.GetHashCode() ^ tz.GetHashCode() ^ layer.GetHashCode());
        }

        /// <summary>
        /// Draws a debug visualization of the navigation mesh with the closed nodes highlighted.
        /// </summary>
        /// <param name="mesh">The mesh to draw.</param>
        /// <param name="query">The query which provides the list of closed nodes.</param>
        public static void Draw(Navmesh mesh, NavmeshQuery query)
        {
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                Draw(mesh.GetTile(i), query, null, 0, i);
            }
        }
        
        /// <summary>
        /// Draws a debug visualization of the navigation mesh with the specified polygons 
        /// highlighted.
        /// </summary>
        /// <param name="mesh">The mesh to draw.</param>
        /// <param name="markPolys">
        /// The references of the polygons that should be highlighted.
        /// </param>
        /// <param name="polyCount">
        /// The number of polygons in the <paramref name="markPolys"/> array.
        /// </param>
        public static void Draw(Navmesh mesh, uint[] markPolys, int polyCount)
        {
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                Draw(mesh.GetTile(i)
                    , null
                    , markPolys
                    , polyCount
                    , i);
            }
        }

        /// <summary>
        /// Draws a debug visualization of a corridor.
        /// </summary>
        /// <param name="mesh">The navigation mesh associated with the corridor.</param>
        /// <param name="corridor">The corridor to draw.</param>
        public static void Draw(Navmesh mesh, PathCorridorData corridor)
        {
            if (corridor.pathCount == 0)
                return;

            DebugDraw.SimpleMaterial.SetPass(0);

            Vector3[] tileVerts = null;

            for (int iPoly = 0; iPoly < corridor.pathCount; iPoly++)
            {
                NavmeshTile tile;
                NavmeshPoly poly;
                mesh.GetTileAndPoly(corridor.path[iPoly], out tile, out poly);

                if (poly.Type == NavmeshPolyType.OffMeshConnection)
                    continue;

                NavmeshTileHeader header = tile.GetHeader();
                if (tileVerts == null
                    || tileVerts.Length < 3 * header.vertCount)
                {
                    // Resize.
                    tileVerts = new Vector3[header.vertCount];
                }

                tile.GetVerts(tileVerts);

                GL.Begin(GL.TRIANGLES);
                GL.Color(polygonOverlayColor);

                int pA = poly.indices[0];
                for (int i = 2; i < poly.vertCount; i++)
                {
                    int pB = poly.indices[i - 1];
                    int pC = poly.indices[i];

                    GL.Vertex(tileVerts[pA]);
                    GL.Vertex(tileVerts[pB]);
                    GL.Vertex(tileVerts[pC]);
                }

                GL.End();

                // Not drawing boundaries since it would obscure other agent
                // debug data.
            }

            Vector3 v = corridor.position;
            DebugDraw.XMarker(v, positionScale, positionColor);
            DebugDraw.Circle(v, positionScale, positionColor);
            DebugDraw.Circle(v, positionScale * 0.5f, positionColor);
            DebugDraw.Circle(v, positionScale * 0.25f, positionColor);

            v = corridor.target;
            DebugDraw.XMarker(v, goalScale, goalColor);
            DebugDraw.Circle(v, goalScale, goalColor);
            DebugDraw.Circle(v, goalScale * 0.5f, goalColor);
            DebugDraw.Circle(v, goalScale * 0.25f, goalColor);
        }

        private static Color GetStandardColor(uint polyRef, int polyArea, int colorId
            , NavmeshQuery query, uint[] markPolys, int markPolyCount)
        {
            Color result;

            if ((query != null && query.IsInClosedList(polyRef))
                || IsInList(polyRef, markPolys, markPolyCount) != -1)
            {
                result = polygonOverlayColor;
            }
            else
            {
                if (colorId == -1)
                {
                    if (polyArea == 0)
                        result = new Color(0, 0.75f, 1, surfaceAlpha);
                    else
                        result = ColorUtil.IntToColor(polyArea, surfaceAlpha);
                }
                else
                    result = ColorUtil.IntToColor(colorId, surfaceAlpha);
            }

            return result;
        }

        /// <summary>
        /// Draws a debug visualization of an individual navmesh tile.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The tile will be checked to see if it is in use before it is drawn.  So there is no 
        /// need for caller to do so.
        /// </para>
        /// </remarks>
        private static void Draw(NavmeshTile tile
            , NavmeshQuery query, uint[] markPolys, int markPolyCount
            , int colorId)
        {
            NavmeshTileHeader header = tile.GetHeader();

            // Keep this check.  Less trouble for clients.
            if (header.polyCount < 1)
                return;

            DebugDraw.SimpleMaterial.SetPass(0);

            uint polyBase = tile.GetBasePolyRef();

            NavmeshPoly[] polys = new NavmeshPoly[header.polyCount];
            tile.GetPolys(polys);
             
            Vector3[] verts = new Vector3[header.vertCount];
            tile.GetVerts(verts);

            NavmeshDetailMesh[] meshes = 
                new NavmeshDetailMesh[header.detailMeshCount];
            tile.GetDetailMeshes(meshes);

            byte[] detailTris = new byte[header.detailTriCount * 4];
            tile.GetDetailTris(detailTris);

            Vector3[] detailVerts = new Vector3[header.detailVertCount];
            tile.GetDetailVerts(detailVerts);

            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < header.polyCount; i++)
            {
                NavmeshPoly poly = polys[i];

                if (poly.Type == NavmeshPolyType.OffMeshConnection)
                    continue;

                NavmeshDetailMesh mesh = meshes[i];

                Color color = GetStandardColor(polyBase | (uint)i
                    , poly.Area, colorId
                    , query, markPolys, markPolyCount);

                GL.Color(color);

                for (int j = 0; j < mesh.triCount; j++)
                {
                    int pTri = (int)(mesh.triBase + j) * 4;

                    for (int k = 0; k < 3; k++)
                    {
                        // Note: iVert and pVert refer to different
                        // arrays.
                        int iVert = detailTris[pTri + k];
                        if (iVert < poly.vertCount)
                        {
                            // Get the vertex from the main vertices.
                            int pVert = poly.indices[iVert];
                            GL.Vertex(verts[pVert]);
                        }
                        else
                        {
                            // Get the vertex from the detail vertices.
                            int pVert = (int)
                                (mesh.vertBase + iVert - poly.vertCount);
                            GL.Vertex(detailVerts[pVert]);
                        }
                    }
                }
            }
            GL.End();

            NavmeshLink[] links = new NavmeshLink[header.maxLinkCount];
            tile.GetLinks(links);

            GL.Begin(GL.LINES);

            DrawPolyBoundaries(header
                , polys
                , verts
                , meshes
                , detailTris
                , detailVerts
                , links
                , new Color(0, 0.2f, 0.25f, 0.13f)
                , true);

            DrawPolyBoundaries(header
                , polys
                , verts
                , meshes
                , detailTris
                , detailVerts
                , links
                , new Color(0.65f, 0.2f, 0, 0.9f)
                , false);

            if (header.connCount == 0)
            {
                GL.End();
                return;
            }

            NavmeshConnection[] conns = new NavmeshConnection[header.connCount];
            tile.GetConnections(conns);

            for (int i = 0; i < header.polyCount; i++)
            {
                NavmeshPoly poly = polys[i];

                if (poly.Type != NavmeshPolyType.OffMeshConnection)
                    continue;

                Color color = GetStandardColor(polyBase | (uint)i
                    , poly.Area, colorId
                    , query, markPolys, markPolyCount);

                // Note: Alpha of less than one doesn't look good because connections tend to
                // overlay a lot of geometry, resulting is off color transitions.
                color.a = 1;

                GL.Color(color);

                NavmeshConnection conn = conns[i - header.connBase];

			    Vector3 va = verts[poly.indices[0]];
			    Vector3 vb = verts[poly.indices[1]];

			    // Check to see if start and end end-points have links.
			    bool startSet = false;
			    bool endSet = false;
			    for (uint k = poly.firstLink; k != Navmesh.NullLink; k = links[k].next)
			    {
				    if (links[k].edge == 0)
					    startSet = true;
				    if (links[k].edge == 1)
					    endSet = true;
			    }
    			
                // For linked endpoints: Draw a line between on-mesh location and endpoint, 
                // and draw circle at the endpoint.
                // For un-linked endpoints: Draw a small red x-marker.

                if (startSet)
                {
                    GL.Vertex(va);
                    GL.Vertex(conn.endpoints[0]);
                    DebugDraw.AppendCircle(conn.endpoints[0], conn.radius);
                }
                else
                {
                    GL.Color(Color.red);
                    DebugDraw.AppendXMarker(conn.endpoints[0], 0.1f);
                    GL.Color(color);
                }

                if (endSet)
                {
                    GL.Vertex(vb);
                    GL.Vertex(conn.endpoints[1]);
                    DebugDraw.AppendCircle(conn.endpoints[1], conn.radius);
                }
                else
                {
                    GL.Color(Color.red);
                    DebugDraw.AppendXMarker(conn.endpoints[1], 0.1f);
                    GL.Color(color);
                }

                DebugDraw.AppendArc(conn.endpoints[0], conn.endpoints[1]
                    , 0.25f
                    , conn.IsBiDirectional ? 0.6f : 0
                    , 0.6f);
            }

            GL.End();
        }

        /// <summary>
        /// Returns the index of the polygon reference within the list, or
        /// -1 if it was not found.
        /// </summary>
        private static int IsInList(uint polyRef
            , uint[] polyList
            , int polyCount)
        {
            if (polyList == null)
                return -1;

            for (int i = 0; i < polyCount; i++)
            {
                if (polyList[i] == polyRef)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Draws the polygon boundary lines based on the height detail.
        /// </summary>
        private static void DrawPolyBoundaries(NavmeshTileHeader header
            , NavmeshPoly[] polys
            , Vector3[] verts
            , NavmeshDetailMesh[] meshes
            , byte[] detailTris
            , Vector3[] detailVerts
            , NavmeshLink[] links
            , Color color
            , bool inner)
        {
            const float thr = 0.01f * 0.01f;

            for (int i = 0; i < header.polyCount; i++)
            {
                NavmeshPoly poly = polys[i];

                if (poly.Type == NavmeshPolyType.OffMeshConnection)
                    continue;

                NavmeshDetailMesh mesh = meshes[i];
                Vector3[] tv = new Vector3[3];

                for (int j = 0, nj = (int)poly.vertCount; j < nj; j++)
                {
                    Color c = color;  // Color may change.
                    if (inner)
                    {
                        if (poly.neighborPolyRefs[j] == 0)
                            continue;
                        if ((poly.neighborPolyRefs[j]
                            & Navmesh.ExternalLink) != 0)
                        {
                            bool con = false;
                            for (uint k = poly.firstLink
                                ; k != Navmesh.NullLink
                                ; k = links[k].next)
                            {
                                if (links[k].edge == j)
                                {
                                    con = true;
                                    break;
                                }
                            }
                            if (con)
                                c = new Color(1, 1, 1, 0.2f);
                            else
                                c = new Color(0, 0, 0, 0.2f);
                        }
                        else
                            c = new Color(0, 0.2f, 0.25f, 0.13f);
                    }
                    else
                    {
                        if (poly.neighborPolyRefs[j] != 0)
                            continue;
                    }

                    GL.Color(c);

                    int pVertA = poly.indices[j];
                    int pVertB = poly.indices[(j + 1) % nj];

                    for (int k = 0; k < mesh.triCount; k++)
                    {
                        int pTri = (int)((mesh.triBase + k) * 4);
                        for (int m = 0; m < 3; m++)
                        {
                            int iVert = detailTris[pTri + m];
                            if (iVert < poly.vertCount)
                            {
                                int pv = poly.indices[iVert];
                                tv[m] = verts[pv];
                            }
                            else
                            {
                                int pv = (int)(mesh.vertBase 
                                    + (iVert - poly.vertCount));
                                tv[m] = detailVerts[pv];
                            }
                        }
                        for (int m = 0, n = 2; m < 3; n = m++)
                        {
                            if (((detailTris[pTri + 3] >> (n * 2)) & 0x3) == 0)
                                // Skip inner detail edges.
                                continue;

                            float distN = Line2.GetPointLineDistanceSq(
                                new Vector2(tv[n].x, tv[n].z)
                                , new Vector2(verts[pVertA].x, verts[pVertA].z)
                                , new Vector2(verts[pVertB].x, verts[pVertB].z));
                            float distM = Line2.GetPointLineDistanceSq(
                                new Vector2(tv[m].x, tv[m].z)
                                , new Vector2(verts[pVertA].x, verts[pVertA].z)
                                , new Vector2(verts[pVertB].x, verts[pVertB].z));

                            if (distN < thr && distM < thr)
                            {
                                GL.Vertex(tv[n]);
                                GL.Vertex(tv[m]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the 3D centroids of the provided navigation mesh polygons.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a polygon does not exist within the mesh, its associated centroid will not 
        /// be altered.  So some centroid data will be invalid if  <paramref name="polyCount"/> 
        /// is not equal to the result count.
        /// </para>
        /// </remarks>
        /// <param name="mesh">The navigation mesh containing the polygons.</param>
        /// <param name="polyRefs">The references of the polygons.</param>
        /// <param name="polyCount">The number of polygons.</param>
        /// <param name="centroids">
        /// The centroids for the polygons. [Length: >= polyCount] (Out)
        /// </param>
        /// <returns>The actual number of polygons found within the mesh. </returns>
        public static int GetCentroids(Navmesh mesh
            , uint[] polyRefs
            , int polyCount
            , Vector3[] centroids)
        {
            int resultCount = 0;
            int count = mesh.GetMaxTiles();
            for (int i = 0; i < count; i++)
            {
                resultCount += GetCentroids(mesh.GetTile(i)
                    , polyRefs
                    , polyCount
                    , centroids);
                if (resultCount == polyRefs.Length)
                    break;
            }
            return resultCount;
        }

        /// <summary>
        /// Gets the centroids for the polygons that are part of the tile.
        /// </summary>
        private static int GetCentroids(NavmeshTile tile
            , uint[] polyRefs
            , int polyCount
            , Vector3[] centroids)
        {
            NavmeshTileHeader header = tile.GetHeader();

            if (header.polyCount < 1)
                return 0;

            uint polyBase = tile.GetBasePolyRef();

            NavmeshPoly[] polys = new NavmeshPoly[header.polyCount];
            tile.GetPolys(polys);

            Vector3[] verts = new Vector3[header.vertCount];
            tile.GetVerts(verts);

            int resultCount = 0;

            for (int i = 0; i < header.polyCount; i++)
            {
                uint polyRef = polyBase | (uint)i;

                int iResult = IsInList(polyRef, polyRefs, polyCount);

                if (iResult == -1)
                    continue;

                resultCount++;

                NavmeshPoly poly = polys[i];

                centroids[iResult] = GetCentroid(verts, poly.indices, poly.vertCount);
            }

            return resultCount;
        }

        /// <summary>
        /// Gets the centroid for a polygon.
        /// </summary>
        private static Vector3 GetCentroid(Vector3[] verts
            , ushort[] indices
            , int vertCount)
        {
            // Reference:
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            Vector3 result = new Vector3();

            for (int i = 0; i < vertCount; i++)
            {
                int p = (ushort)indices[i];
                result += verts[p];
            }

            result.x /= vertCount;
            result.y /= vertCount;
            result.z /= vertCount;

            return result;
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Draws a debug visualization of corner data.
        /// </summary>
        /// <param name="corners">The corners to draw.</param>
        public static void Draw(CornerData corners)
        {
            if (corners.cornerCount == 0)
                return;

            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(cornerColor);

            for (int i = 0; i < corners.cornerCount; i++)
            {
                DebugDraw.AppendXMarker(corners.verts[i], cornerScale);
            }

            GL.End();
        }

        #endregion
    }
}
