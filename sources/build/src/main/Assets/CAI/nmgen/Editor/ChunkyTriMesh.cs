/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using org.critterai.interop;
using org.critterai.geom;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents a triangle mesh with area data, spatially divided into nodes for more efficient 
    /// querying.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Objects of this type are created using the <see cref="ChunkyTriMeshBuilder"/> class.
    /// </para>
    /// <para>
    /// Storage is optimized for use by the NMGen build process across muliple threads. 
    /// It is not efficient for other uses.
    /// </para>
    /// <para>
    /// The content of a node can be inspected as follows:
    /// </para>
    /// <code>
    /// // Where lmesh is a TriangleMesh object and lareas is a byte array obtained
    /// // from ChunkyTriMesh.ExtractMesh.
    /// for (int j = 0; j &lt; node.count; j++, i++)
    /// {
    ///     int pTri = (node.i + j) * 3;
    ///     
    ///     int iVertA = lmesh.tris[pTri + 0];
    ///     int iVertB = lmesh.tris[pTri + 1];
    ///     int iVertC = lmesh.tris[pTri + 2];
    ///     
    ///     byte triArea = lareas[node.i + j];
    ///     
    ///     Vector3 vertA = new Vector3(lmesh.verts[iVertA]
    ///         , lmesh.verts[iVertA]
    ///         , lmesh.verts[iVertA]);
    ///         
    ///     // Repeat for vertB and vertC...
    ///     
    ///     // Use the vertices...
    /// }
    /// </code>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
	public sealed class ChunkyTriMesh
	{
        internal IntPtr verts;
        internal IntPtr tris;
        internal IntPtr areas;

        private ChunkyTriMeshNode[] mNodes;
        private readonly int mNodeCount;
        private readonly int mTriCount;
        private readonly int mVertCount;

        /// <summary>
        /// The number of spacial nodes in the mesh.
        /// </summary>
        public int NodeCount { get { return mNodeCount; } }

        /// <summary>
        /// The number of triangles in the mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will always be > 0.  Empty meshes cannot be created.
        /// </para>
        /// </remarks>
        public int TriCount { get { return mTriCount; } }

        internal ChunkyTriMesh(Vector3[] verts
            , int vertCount
            , int[] tris
            , byte[] areas
            , int triCount
            , ChunkyTriMeshNode[] nodes
            , int nodeCount)
        {
            int size = sizeof(float) * vertCount * 3;
            this.verts = UtilEx.GetBuffer(size, false);
            float[] fverts = Vector3Util.Flatten(verts, vertCount); // Bleh.
            Marshal.Copy(fverts, 0, this.verts, vertCount * 3);

            size = sizeof(int) * tris.Length;
            this.tris = UtilEx.GetBuffer(size, false);
            Marshal.Copy(tris, 0, this.tris, tris.Length);

            size = sizeof(byte) * areas.Length;
            this.areas = UtilEx.GetBuffer(size, false);
            Marshal.Copy(areas, 0, this.areas, areas.Length);

            mTriCount = triCount;
            mVertCount = vertCount;
            mNodes = nodes;
            mNodeCount = nodeCount;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~ChunkyTriMesh()
        {
            Dispose();
        }

        /// <summary>
        /// Gets all nodes overlapped by the specified bounds.
        /// </summary>
        /// <param name="xmin">The minimum x-bounds.</param>
        /// <param name="zmin">The minimum z-bounds.</param>
        /// <param name="xmax">The maximum x-bounds.</param>
        /// <param name="zmax">The maximum z-bounds.</param>
        /// <param name="resultNodes">The list to append the result to.</param>
        /// <returns>The number of result nodes appended to the result list.</returns>
        public int GetChunks(float xmin, float zmin, float xmax, float zmax
            , List<ChunkyTriMeshNode> resultNodes)
        {
            if (tris == IntPtr.Zero || resultNodes == null)
                return 0;

            int result = 0;
            int i = 0;
            while (i < mNodeCount)
            {
                ChunkyTriMeshNode node = mNodes[i];

                bool overlap = node.Overlaps(xmin, zmin, xmax, zmax);

                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    resultNodes.Add(node);
                    result += node.count;
                }

                if (overlap || isLeafNode)
                    i++;
                else
                {
                    int escapeIndex = -node.i;
                    i += escapeIndex;
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts all triangles from the mesh. (Not efficient.)
        /// </summary>
        /// <param name="areas">The areas for each triangle.</param>
        /// <returns>The triangle mesh data.</returns>
        public TriangleMesh ExtractMesh(out byte[] areas)
        {
            if (this.tris == IntPtr.Zero)
            {
                areas = null;
                return null;
            }

            TriangleMesh result = new TriangleMesh();
            result.triCount = mTriCount;
            result.vertCount = mVertCount;

            float[] lverts = new float[mVertCount * 3];
            Marshal.Copy(this.verts, lverts, 0, mVertCount * 3);
            result.verts = Vector3Util.GetVectors(lverts);

            result.tris = new int[mTriCount * 3];
            Marshal.Copy(this.tris, result.tris, 0, mTriCount * 3);

            areas = new byte[mTriCount];
            Marshal.Copy(this.areas, areas, 0, mTriCount);

            return result;
        }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (tris == IntPtr.Zero); } }

        /// <summary>
        /// Frees all resources and marks object as disposed.
        /// </summary>
        public void Dispose()
        {
            if (tris != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(verts);
                Marshal.FreeHGlobal(tris);
                Marshal.FreeHGlobal(areas);
                verts = IntPtr.Zero;
                tris = IntPtr.Zero;
                areas = IntPtr.Zero;
            }
        }
    }
}
