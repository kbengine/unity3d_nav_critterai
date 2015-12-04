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
    /// Represents data for a <see cref="PolyMeshDetail"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Warning:</b> The serializable attributewill be removed in v0.5. Use 
    /// <see cref="PolyMeshDetail.GetSerializedData"/> instead of serializing this object.
    /// </para>
    /// <para>
    /// The detail mesh is made up of triangle sub-meshes which provide extra height detail for 
    /// each polygon in its assoicated polygon mesh.
    /// </para>
    /// <para>
    /// See the individual field definitions for details related to the structure of the mesh.
    /// </para>
    /// <para>
    /// Implemented as a class with public fields in order to support Unity serialization.  Care 
    /// must be taken not to set the fields to invalid values.
    /// </para>
    /// </remarks>
    /// <seealso cref="PolyMeshDetail"/>
    [Serializable]
    public sealed class PolyMeshDetailData
    {
        /// <summary>
        /// The sub-mesh data.
        /// [(baseVertIndex, vertCount, baseTriIndex, triCount) * meshCount] 
        /// [Size: >= 4 * meshCount]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Maximum number of vertices per sub-mesh: 127<br/>
        /// Maximum number of triangles per sub-mesh: 255
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// An example of iterating the triangles in a sub-mesh.
        /// </para>
        /// <code>
        ///     // Where iMesh is the index for the sub-mesh within detailMesh
        ///     int pMesh = iMesh * 4;
        ///     int pVertBase = meshes[pMesh + 0] * 3;
        ///     int pTriBase = meshes[pMesh + 2] * 4;
        ///     int tCount = meshes[pMesh + 3];
        ///     int vertX, vertY, vertZ;
        ///
        ///     for (int iTri = 0; iTri &lt; tCount; iTri++)
        ///     {
        ///        for (int iVert = 0; iVert &lt; 3; iVert++)
        ///        {
        ///            int pVert = pVertBase
        ///                + (tris[pTriBase + (iTri * 4 + iVert)] * 3);
        ///            vertX = verts[pVert + 0];
        ///            vertY = verts[pVert + 1];
        ///            vertZ = verts[pVert + 2];
        ///            // Do something with the vertex.
        ///        }
        ///    }
        /// </code>
        /// </example>
        public uint[] meshes;

        /// <summary>
        /// The mesh vertices. 
        /// [Length: >= vertCount]
        /// </summary>
        /// <remarks>
        /// <para>
        /// The vertices are grouped by sub-mesh and will contain duplicates since each sub-mesh is independently defined.
        /// </para>
        /// <para>
        /// The first group of vertices for each sub-mesh are in the same rder as the vertices for 
        /// the sub-mesh's associated  <see cref="PolyMesh"/> polygon.  These vertices are followed
        /// by any additional detail vertices.  So it the associated polygon has 5 vertices, the 
        /// sub-mesh will have a minimum of 5 vertices and the first 5 vertices will be equivalent 
        /// to the 5 polygon vertices.
        /// </para>
        /// </remarks>
        public Vector3[] verts;

        /// <summary>
        /// The mesh triangles.
        /// [vertIndexA, vertIndexB, vertIndexC, flag) * triCount]
        /// [Size: >= 4 * triCount]
        /// </summary>
        /// <remarks>
        /// <para>
        /// The triangles are grouped by sub-mesh.
        /// </para>
        /// <para>
        /// <b>Vertices</b>
        /// </para>
        /// <para>
        /// The vertex indices in the triangle array are local to the sub-mesh, not global.  To 
        /// translate into an global index in the vertices array, the values must be offset by 
        /// the sub-mesh's base vertex index.
        /// </para>
        /// <para>
        /// Example: If the <c>baseVertexIndex</c> for the sub-mesh is 5 and the triangle entry 
        /// is <c>(4, 8, 7, 0)</c>, then the actual indices for the vertices are 
        /// <c>(4 + 5, 8 + 5, 7 + 5)</c>.
        /// </para>
        /// <para>
        /// <b>Flags</b>
        /// </para>
        /// <para>
        /// The flags entry indicates which edges are internal and which are external to the 
        /// sub-mesh.
        /// </para>
        /// <para>
        /// Internal edges connect to other triangles within the same sub-mesh.
        /// External edges represent portals to other sub-meshes or the null region.
        /// </para>
        /// <para>
        /// Each flag is stored in a 2-bit position.  Where position 0 is the lowest 2-bits and 
        /// position 4 is the highest 2-bits:
        /// </para>
        /// <para>
        /// Position 0: Edge AB (>> 0)<br />
        /// Position 1: Edge BC (>> 2)<br />
        /// Position 2: Edge CA (>> 4)<br />
        /// Position 4: Unused
        /// </para>
        /// <para>
        /// Testing can be performed as follows:
        /// </para>
        /// <code>
        /// if (((flag >> 2) &amp; 0x3) == 0)
        /// {
        ///     // Edge BC is an external edge.
        /// }
        /// </code>
        /// </remarks>
        public byte[] tris;

        /// <summary>
        /// The sub-mesh count. [Limit: > 0]
        /// </summary>
        public int meshCount;

        /// <summary>
        /// The vertex count. [Limit: >= 3]
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The triangle count. [Limit > 0]
        /// </summary>
        public int triCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxVerts">
        /// The maximum vertices the vertex buffer will hold. [Limit: >= 3]
        /// </param>
        /// <param name="maxTris">
        /// The maximum triangles the triangle buffer will hold. [Limit: > 0]
        /// </param>
        /// <param name="maxMeshes">
        /// The maximum sub-meshes the mesh buffer will hold. [Limit: > 0]
        /// </param>
        public PolyMeshDetailData(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            if (maxVerts < 3
                || maxTris < 1
                || maxMeshes < 1)
            {
                return;
            }

            meshes = new uint[maxMeshes * 4];
            tris = new byte[maxTris * 4];
            verts = new Vector3[maxVerts];
        }

        /// <summary>
        /// Clears all object data and resizes the buffers.
        /// </summary>
        /// <param name="maxVerts">
        /// The maximum vertices the vertex buffer will hold. [Limit: >= 3]
        /// </param>
        /// <param name="maxTris">
        /// The maximum triangles the triangle buffer will hold. [Limit: > 0]
        /// </param>
        /// <param name="maxMeshes">
        /// The maximum sub-meshes the mesh buffer will hold. [Limit: > 0]
        /// </param>
        public void Reset(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            Reset();

            meshes = new uint[maxMeshes * 4];
            tris = new byte[maxTris * 4];
            verts = new Vector3[maxVerts];
        }

        private void Reset()
        {
            vertCount = 0;
            triCount = 0;
            meshCount = 0;
            meshes = null;
            tris = null;
            verts = null;
        }

        /// <summary>
        /// Checks the size of the buffers to see if they are large enough to hold the specified 
        /// data.
        /// </summary>
        /// <param name="vertCount">The maximum vertices the object needs to hold.</param>
        /// <param name="triCount">The maximum triangles the object needs to hold.</param>
        /// <param name="meshCount">The maximum sub-meshes the object needs to hold.</param>
        /// <returns>True if all buffers are large enough to fit the data.</returns>
        public bool CanFit(int vertCount
            , int triCount
            , int meshCount)
        {
            if (verts == null || verts.Length < vertCount
                || tris == null || tris.Length < triCount * 4
                || meshes == null || meshes.Length < meshCount * 4)
            {
                return false;
            }
            return true;
        }
    }
}
