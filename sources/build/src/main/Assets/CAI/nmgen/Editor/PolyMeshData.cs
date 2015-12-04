/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using System;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents the mesh data for a <see cref="PolyMesh"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Warning:</b> The serializable attributewill be removed in v0.5. Use 
    /// <see cref="PolyMesh.GetSerializedData"/> instead of serializing this object.
    /// </para>
    /// <para>
    /// Represents a mesh of potentially overlapping convex polygons of between three and 
    /// <see cref="maxVertsPerPoly"/> vertices. The mesh exists within the context of an 
    /// axis-aligned bounding box (AABB) with vertices laid out in an evenly spaced grid 
    /// based on xz-plane and y-axis cells.
    /// </para>
    /// <para>
    /// This class is not compatible with Unity serialization.
    /// </para>
    /// </remarks>
    /// <example>
	///	<para>Iterating the Polygons</para>
	///	<code>      
	///		int[] pTargetVert = new int[2];
	///	
	///		// Loop through the polygons.
	///		for (int iPoly = 0; iPoly &lt;polyCount; iPoly++)
	///		{
	///			int pPoly = iPoly * maxVertsPerPoly * 2;
	///	
	///			// Loop through the edges.
	///			for (int iPolyVert = 0; iPolyVert &lt;maxVertsPerPoly; iPolyVert++)
	///			{
	///				int iv = polys[pPoly + iPolyVert];
	///				
	///				if (iv == NullIndex)
	///					// Soft end of the polygon.
	///					break;
	///					
	///				if (polys[pPoly + maxVertsPerPoly + iPolyVert]
	///						== NullIndex)
	///				{
	///					// The edge is a solid border.
	///				}
	///				else
	///				}
	///					// The edge connects to another polygon.
	///				}
	///	
	///				// Pointer to first edge vertex.
	///				pTargetVert[0] = iv * 3;
	///	
	///				if (iPolyVert + 1 >= maxVertsPerPoly)
	///					// Reached hard end of polygon.  Loop back.
	///					iv = polys[pPoly + 0];
	///				else
	///				{
	///					iv = polys[pPoly + iPolyVert + 1];
	///					if (iv == NullIndex)
	///						// Reached soft send of polygon.  Loop back.
	///						iv = polys[pPoly + 0];
	///				}
	///				// Pointer to second edge vertex.
	///				pTargetVert[1] = iv * 3;
	///	
	///				for (int i = 0; i &lt;2; i++)
	///				{
	///					int p = pTargetVert[i];
	///					int x = verts[p + 0];
	///					int y = verts[p + 1];
	///					int z = verts[p + 2];
	///					float worldX = boundsMin[0] + x * xzCellSize;
	///					float worldY = boundsMin[1] + y * yCellSize;
	///					float worldZ = boundsMin[2] + z * xzCellSize;
	///					// Do something with the vertices.
	///				}
	///			}
	///		}
	///	 </code>
    /// </example>
    /// <seealso cref="PolyMesh"/>
    [Serializable]
    public sealed class PolyMeshData
    {
        /// <summary>
        /// Mesh vertices.
        /// [(x, y, z) * vertCount]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Minimum bounds and cell size is used to convert vertex coordinates into world space.
        /// </para>
        /// <code>
        /// worldX = boundsMin[0] + vertX * xzCellSize
        /// worldY = boundsMin[1] + vertY * yCellSize
        /// worldZ = boundsMin[2] + vertZ * xzCellSize
        /// </code>
        /// </remarks>
        public ushort[] verts;

        /// <summary>
        /// Polygon and neighbor data. [Length: >= polyCount * 2 * maxVertsPerPoly]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each entry is 2 * MaxVertsPerPoly in length.</para>
        /// <para>
        /// The first half of the entry contains the indices of the polygon. The first instance of 
        /// <see cref="PolyMesh.NullIndex"/> indicates the end of the indices for the entry.
        /// </para>
        /// <para>
        /// The second half contains indices to neighbor polygons.  A value of 
        /// <see cref="PolyMesh.NullIndex"/> indicates no connection for the  associated edge. 
        /// (Solid wall.)
        /// </para>
        /// <para>
        /// <b>Example:</b>
        /// </para>
        /// <para>
        /// MaxVertsPerPoly = 6<br/>
        /// For the entry: 
        /// (1, 3, 4, 8, NullIndex, NullIndex, 18, NullIndex, 21, NullIndex, NullIndex, NullIndex)
        /// </para>
        /// <para>
        /// (1, 3, 4, 8) defines a polygon with 4 vertices.<br />
        /// Edge 1->3 is shared with polygon 18.<br />
        /// Edge 4->8 is shared with polygon 21.<br />
        /// Edges 3->4 and 4->8 are border edges not shared with any other polygon.
        /// </para>
        /// </remarks>
        public ushort[] polys;

        /// <summary>
        /// The region id assigned to each polygon. [Length: >= polyCount]
        /// </summary>
        public ushort[] regions;

        /// <summary>
        /// The flags assigned to each polygon. [Length: >= polyCount]
        /// </summary>
        public ushort[] flags;

        /// <summary>
        /// The area assigned to each polygon. [Length: >= polyCount]
        /// </summary>
        /// <remarks>
        /// <para>
        /// During the standard build process, all walkable polygons get the default value 
        /// of <see cref="NMGen.MaxArea"/>. This value can then be changed to meet user 
        /// requirements.
        /// </para>
        /// </remarks>
        public byte[] areas;

        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int polyCount;

        /// <summary>
        /// The maximum vertices per polygon. 
        /// [Limits: 3 &lt;= value &lt;= <see cref="NMGen.MaxAllowedVertsPerPoly"/>]
        /// </summary>
        public int maxVertsPerPoly;

        /// <summary>
        /// The minimum bounds of the mesh's AABB.
        /// </summary>
        public Vector3 boundsMin;

        /// <summary>
        /// The maximum bounds of the mesh's AABB.
        /// </summary>
        public Vector3 boundsMax;

        /// <summary>
        /// The xz-plane cell size. [Limit: >= <see cref="NMGen.MinCellSize"/>]
        /// </summary>
        public float xzCellSize;

        /// <summary>
        /// The y-axis cell height. [Limit: >= <see cref="NMGen.MinCellSize"/>]
        /// </summary>
        public float yCellSize;

        /// <summary>
        /// The AABB border size used to build the mesh. [Limit: >= 0] [Units: XZCellSize]
        /// </summary>
        public int borderSize;

        /// <summary>
        /// The walkable height used to build the mesh.  [Units: World]
        /// </summary>
        public float walkableHeight;

        /// <summary>
        /// The walkable radius used to build the mesh. [Limit: >= 0] [Units: World]
        /// </summary>
        public float walkableRadius;

        /// <summary>
        /// The maximum walkable step used to build the mesh. [Limit: >= 0] [Units: World]
        /// </summary>
        public float walkableStep;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxVerts">
        /// The maximum veritices the vertex buffer can hold. [Limit: >= 3]
        /// </param>
        /// <param name="maxPolys">
        /// The maximum polygons the polygon buffer can hold. [Limit: > 0]
        /// </param>
        /// <param name="maxVertsPerPoly">
        /// The maximum allowed vertices for a polygon.
        /// [Limits: 3 &lt;= value &lt;= <see cref="NMGen.MaxAllowedVertsPerPoly"/>]
        /// </param>
        public PolyMeshData(int maxVerts
            , int maxPolys
            , int maxVertsPerPoly)
        {
            if (maxVerts < 3
                || maxPolys < 1
                || maxVertsPerPoly < 3
                || maxVertsPerPoly > NMGen.MaxAllowedVertsPerPoly)
            {
                return;
            }

            polys = new ushort[maxPolys * 2 * maxVertsPerPoly];
            verts = new ushort[maxVerts * 3];
            areas = new byte[maxPolys];
            flags = new ushort[maxPolys];
            regions = new ushort[maxPolys];

            this.maxVertsPerPoly = maxVertsPerPoly;
        }

        /// <summary>
        /// Clears all object data and resizes the buffers.
        /// </summary>
        /// <param name="maxVerts">
        /// The maximum veritices the vertex buffer can hold. [Limit: >= 3]
        /// </param>
        /// <param name="maxPolys">
        /// The maximum polygons the polygon buffer can hold. [Limit: > 0]
        /// </param>
        /// <param name="maxVertsPerPoly">
        /// The maximum allowed vertices for a polygon.
        /// [Limits: 3 &lt;= value &lt;= <see cref="NMGen.MaxAllowedVertsPerPoly"/>]
        /// </param>
        public void Resize(int maxVerts
            , int maxPolys
            , int maxVertsPerPoly)
        {
            Resize();

            if (maxVerts < 3
                || maxPolys < 1
                || maxVertsPerPoly < 3
                || maxVertsPerPoly > NMGen.MaxAllowedVertsPerPoly)
            {
                return;
            }

            polys = new ushort[maxPolys * 2 * maxVertsPerPoly];
            verts = new ushort[maxVerts * 3];
            areas = new byte[maxPolys];
            flags = new ushort[maxPolys];
            regions = new ushort[maxPolys];

            this.maxVertsPerPoly = maxVertsPerPoly;
        }

        private void Resize()
        {
            vertCount = 0;
            polyCount = 0;
            maxVertsPerPoly = 0;
            boundsMin = Vector3Util.Zero;
            boundsMax = Vector3Util.Zero;
            xzCellSize = 0;
            yCellSize = 0;
            borderSize = 0;
            walkableHeight = 0;
            walkableStep = 0;
            walkableRadius = 0;
            polys = null;
            verts = null;
            areas = null;
            flags = null;
            regions = null;
        }

        /// <summary>
        /// Checks the size of the buffers to see if they are large enough to hold the specified 
        /// data.
        /// </summary>
        /// <param name="vertCount">The maximum vertices the vertex buffer needs to hold.</param>
        /// <param name="polyCount">The maximum polygons the polygon buffer needs to hold.</param>
        /// <param name="maxVertsPerPoly">The maximum allowed vertices for a polygon.</param>
        /// <returns>True if all buffers are large enough to fit the data.</returns>
        public bool CanFit(int vertCount
            , int polyCount
            , int maxVertsPerPoly)
        {
            if (maxVertsPerPoly != this.maxVertsPerPoly
                || polys == null 
                || polys.Length < polyCount * 2 * maxVertsPerPoly
                || verts == null || verts.Length < vertCount * 3
                || areas == null || areas.Length < polyCount
                || flags == null || flags.Length < polyCount
                || regions == null || regions.Length < polyCount)
            {
                return false;
            }
            return true;
        }
    }
}
